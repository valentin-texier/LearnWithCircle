#if IOS || MACCATALYST
using Microsoft.Maui.Handlers;
using System.ComponentModel;
using System.Diagnostics;
using UIKit;

namespace LearnWithCircle.Controls;

public partial class RotatableImageHandler : ImageHandler
{
    private const double MinVisiblePart = 8d;
    private UIPinchGestureRecognizer? _pinchRecognizer;
    private UIPanGestureRecognizer? _rotateRecognizer;
    private UIPanGestureRecognizer? _panRecognizer;
    private double _panStartX;
    private double _panStartY;
    private double _pinchStartScale;
    private double _pinchStartTranslationX;
    private double _pinchStartTranslationY;
    private double _pinchFocusX;
    private double _pinchFocusY;
    private double _rotateStart;
    private double _rotateStartAngle;
    private long _lastGestureTimestamp;

    protected override void ConnectHandler(UIImageView platformView)
    {
        base.ConnectHandler(platformView);

        platformView.UserInteractionEnabled = true;
        platformView.MultipleTouchEnabled = true;
        Log("connect");
        _pinchRecognizer = new UIPinchGestureRecognizer(OnPinch)
        {
            ShouldRecognizeSimultaneously = (_, _) => true,
            CancelsTouchesInView = false
        };
        _rotateRecognizer = new UIPanGestureRecognizer(OnRotate)
        {
            MinimumNumberOfTouches = 1,
            MaximumNumberOfTouches = 1,
            ShouldRecognizeSimultaneously = (_, _) => true,
            CancelsTouchesInView = false
        };
        _panRecognizer = new UIPanGestureRecognizer(OnPan)
        {
            MinimumNumberOfTouches = 3,
            MaximumNumberOfTouches = 3,
            ShouldRecognizeSimultaneously = (_, _) => true,
            CancelsTouchesInView = false
        };
        platformView.AddGestureRecognizer(_pinchRecognizer);
        platformView.AddGestureRecognizer(_rotateRecognizer);
        platformView.AddGestureRecognizer(_panRecognizer);
        if (VirtualView is RotatableImage view)
        {
            view.PropertyChanged += OnVirtualViewPropertyChanged;
            ApplyRasterization(view, platformView);
        }
    }

    protected override void DisconnectHandler(UIImageView platformView)
    {
        if (VirtualView is RotatableImage view)
            view.PropertyChanged -= OnVirtualViewPropertyChanged;

        if (_pinchRecognizer is not null)
        {
            platformView.RemoveGestureRecognizer(_pinchRecognizer);
            _pinchRecognizer.Dispose();
            _pinchRecognizer = null;
        }

        if (_rotateRecognizer is not null)
        {
            platformView.RemoveGestureRecognizer(_rotateRecognizer);
            _rotateRecognizer.Dispose();
            _rotateRecognizer = null;
        }

        if (_panRecognizer is not null)
        {
            platformView.RemoveGestureRecognizer(_panRecognizer);
            _panRecognizer.Dispose();
            _panRecognizer = null;
        }

        Log("disconnect");
        base.DisconnectHandler(platformView);
    }

    private void OnVirtualViewPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not RotatableImage view)
            return;

        if (PlatformView is not UIImageView platformView)
            return;

        if (e.PropertyName == RotatableImage.EnableRasterizationProperty.PropertyName ||
            e.PropertyName == RotatableImage.RasterizationScaleProperty.PropertyName)
        {
            ApplyRasterization(view, platformView);
        }
    }

    private static void ApplyRasterization(RotatableImage view, UIImageView platformView)
    {
        platformView.Layer.ShouldRasterize = view.EnableRasterization;
        platformView.Layer.RasterizationScale = (nfloat)Math.Max(1d, view.RasterizationScale);
    }

    private void OnPinch(UIPinchGestureRecognizer recognizer)
    {
        if (VirtualView is not RotatableImage view)
            return;

        if (recognizer.NumberOfTouches != 2)
            return;

        if (recognizer.State == UIGestureRecognizerState.Began)
        {
            _pinchStartScale = view.Scale;
            _pinchStartTranslationX = view.TranslationX;
            _pinchStartTranslationY = view.TranslationY;
            var center = GetCenter(recognizer.View);
            var location = recognizer.LocationInView(recognizer.View);
            var focusX = location.X - center.X;
            var focusY = location.Y - center.Y;
            var rotated = RotateToScreen(focusX, focusY, view.Rotation);
            _pinchFocusX = rotated.X;
            _pinchFocusY = rotated.Y;
            _lastGestureTimestamp = 0;
            Log("pinch start");
        }

        if (recognizer.State == UIGestureRecognizerState.Changed)
        {
            if (!ShouldProcessGesture(view))
                return;

            var factor = recognizer.Scale;
            var adjusted = 1 + (factor - 1) * view.PinchSensitivity;
            var targetScale = Math.Clamp(_pinchStartScale * adjusted, view.MinScale, view.MaxScale);
            var nextScale = view.GestureSmoothing > 0
                ? ApplySmoothing(view.Scale, targetScale, view.GestureSmoothing)
                : targetScale;
            if (Math.Abs(nextScale - view.Scale) > 0.0001)
                view.Scale = nextScale;
            var deltaScale = _pinchStartScale <= 0 ? 1 : nextScale / _pinchStartScale;
            var targetX = _pinchStartTranslationX + (1 - deltaScale) * _pinchFocusX;
            var targetY = _pinchStartTranslationY + (1 - deltaScale) * _pinchFocusY;
            ApplyTranslation(view, targetX, targetY, recognizer.View);

            if (nextScale <= view.MinScale + 0.01)
            {
                view.TranslationX = 0;
                view.TranslationY = 0;
            }
        }
        else if (recognizer.State == UIGestureRecognizerState.Ended ||
                 recognizer.State == UIGestureRecognizerState.Cancelled)
        {
            Log("pinch end");
        }
    }

    private void OnRotate(UIPanGestureRecognizer recognizer)
    {
        if (VirtualView is not RotatableImage view)
            return;

        if (recognizer.NumberOfTouches != 1)
            return;

        if (recognizer.State == UIGestureRecognizerState.Began)
        {
            _rotateStart = view.Rotation;
            _rotateStartAngle = GetAngleToCenter(recognizer);
            _lastGestureTimestamp = 0;
            Log("rotation start");
        }

        if (recognizer.State == UIGestureRecognizerState.Changed)
        {
            if (!ShouldProcessGesture(view))
                return;

            var angle = GetAngleToCenter(recognizer);
            var delta = NormalizeDelta(angle - _rotateStartAngle);
            var deltaDegrees = delta * (180 / Math.PI) * view.RotationSensitivity;
            var target = NormalizeDegrees(_rotateStart + deltaDegrees);
            if (view.GestureSmoothing > 0)
                view.Rotation = ApplyRotationSmoothing(view.Rotation, target, view.GestureSmoothing);
            else
                view.Rotation = target;
        }
        else if (recognizer.State == UIGestureRecognizerState.Ended ||
                 recognizer.State == UIGestureRecognizerState.Cancelled)
        {
            Log("rotation end");
        }
    }

    private void OnPan(UIPanGestureRecognizer recognizer)
    {
        if (VirtualView is not RotatableImage view)
            return;

        if (recognizer.NumberOfTouches != 3)
            return;

        if (view.Scale <= view.MinScale + 0.01)
            return;

        if (recognizer.State == UIGestureRecognizerState.Began)
        {
            _panStartX = view.TranslationX;
            _panStartY = view.TranslationY;
            _lastGestureTimestamp = 0;
            Log("pan start");
        }

        if (recognizer.State == UIGestureRecognizerState.Changed)
        {
            if (!ShouldProcessGesture(view))
                return;

            var translation = recognizer.TranslationInView(recognizer.View);
            // Compensate for element rotation so pan follows viewport axes.
            var compensated = RotateToScreen(translation.X, translation.Y, view.Rotation);
            // Map-like pan: scale displacement with zoom so high zoom does not feel "stuck".
            var panMultiplier = view.PanSensitivity * Math.Max(1d, view.Scale);
            var targetX = _panStartX + compensated.X * panMultiplier;
            var targetY = _panStartY + compensated.Y * panMultiplier;
            ApplyTranslation(view, targetX, targetY, recognizer.View);
        }
        else if (recognizer.State == UIGestureRecognizerState.Ended ||
                 recognizer.State == UIGestureRecognizerState.Cancelled)
        {
            Log("pan end");
        }
    }

#if DEBUG
    static partial void LogPlatform(string message)
    {
        Console.WriteLine($"[RotatableImage][iOS] {message}");
    }
#endif

    private static double NormalizeDegrees(double value)
    {
        var result = value % 360;
        return result < 0 ? result + 360 : result;
    }

    private bool ShouldProcessGesture(RotatableImage view)
    {
        var throttleMs = view.GestureThrottleMs;
        if (throttleMs <= 0)
            return true;

        var now = Stopwatch.GetTimestamp();
        if (_lastGestureTimestamp == 0)
        {
            _lastGestureTimestamp = now;
            return true;
        }

        var elapsedMs = (now - _lastGestureTimestamp) * 1000d / Stopwatch.Frequency;
        if (elapsedMs < throttleMs)
            return false;

        _lastGestureTimestamp = now;
        return true;
    }

    private static double ApplySmoothing(double current, double target, double smoothing)
    {
        if (smoothing <= 0 || smoothing >= 1)
            return target;

        return current + (target - current) * smoothing;
    }

    private static double ApplyRotationSmoothing(double current, double target, double smoothing)
    {
        if (smoothing <= 0 || smoothing >= 1)
            return target;

        var delta = NormalizeRotationDelta(target - current);
        return NormalizeDegrees(current + delta * smoothing);
    }

    private static double NormalizeRotationDelta(double delta)
    {
        while (delta > 180)
            delta -= 360;
        while (delta < -180)
            delta += 360;
        return delta;
    }

    private void ApplyTranslation(RotatableImage view, double targetX, double targetY, UIView? platformView)
    {
        var clampedTarget = ClampTranslation(view, targetX, targetY, platformView);
        if (view.GestureSmoothing > 0)
        {
            var smoothedX = ApplySmoothing(view.TranslationX, clampedTarget.X, view.GestureSmoothing);
            var smoothedY = ApplySmoothing(view.TranslationY, clampedTarget.Y, view.GestureSmoothing);
            var clampedSmoothed = ClampTranslation(view, smoothedX, smoothedY, platformView);
            view.TranslationX = clampedSmoothed.X;
            view.TranslationY = clampedSmoothed.Y;
        }
        else
        {
            view.TranslationX = clampedTarget.X;
            view.TranslationY = clampedTarget.Y;
        }
    }

    private static (double X, double Y) ClampTranslation(RotatableImage view, double targetX, double targetY, UIView? platformView)
    {
        if (platformView is null)
            return (targetX, targetY);

        var viewportWidth = platformView.Bounds.Width;
        var viewportHeight = platformView.Bounds.Height;
        if (viewportWidth <= 0 || viewportHeight <= 0)
            return (targetX, targetY);

        var baseWidth = view.Width > 0 ? view.Width : (view.WidthRequest > 0 ? view.WidthRequest : viewportWidth);
        var baseHeight = view.Height > 0 ? view.Height : (view.HeightRequest > 0 ? view.HeightRequest : viewportHeight);
        var scaledWidth = Math.Max(1d, baseWidth * Math.Max(view.Scale, view.MinScale));
        var scaledHeight = Math.Max(1d, baseHeight * Math.Max(view.Scale, view.MinScale));

        var visibleX = Math.Min(MinVisiblePart, scaledWidth);
        var visibleY = Math.Min(MinVisiblePart, scaledHeight);
        var maxX = Math.Max(0d, (viewportWidth + scaledWidth) / 2d - visibleX);
        var maxY = Math.Max(0d, (viewportHeight + scaledHeight) / 2d - visibleY);
        return (Math.Clamp(targetX, -maxX, maxX), Math.Clamp(targetY, -maxY, maxY));
    }

    private static double NormalizeDelta(double delta)
    {
        while (delta > Math.PI)
            delta -= 2 * Math.PI;
        while (delta < -Math.PI)
            delta += 2 * Math.PI;
        return delta;
    }

    private static (double X, double Y) GetCenter(UIView? view)
    {
        if (view is null)
            return (0, 0);

        var bounds = view.Bounds;
        return (bounds.Width / 2d, bounds.Height / 2d);
    }

    private static double GetAngleToCenter(UIPanGestureRecognizer recognizer)
    {
        var center = GetCenter(recognizer.View);
        var location = recognizer.LocationInView(recognizer.View);
        var x = location.X - center.X;
        var y = location.Y - center.Y;
        return Math.Atan2(y, x);
    }

    private static (double X, double Y) RotateToScreen(double dx, double dy, double rotationDegrees)
    {
        if (Math.Abs(rotationDegrees) < 0.001)
            return (dx, dy);

        var radians = rotationDegrees * (Math.PI / 180d);
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        var x = (dx * cos) - (dy * sin);
        var y = (dx * sin) + (dy * cos);
        return (x, y);
    }
}
#endif
