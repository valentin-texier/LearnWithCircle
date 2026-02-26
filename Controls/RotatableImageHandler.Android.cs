#if ANDROID
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Handlers;
using System.ComponentModel;
using System.Diagnostics;

namespace LearnWithCircle.Controls;

public partial class RotatableImageHandler : ImageHandler
{
    private const double MinScaleEpsilon = 0.01;
    private const double MinVisiblePart = 8d;
    private int _activeMode;
    private float _rotateStartAngle;
    private double _rotateStartRotation;
    private float _pinchStartSpan;
    private double _pinchStartScale;
    private double _startTranslationX;
    private double _startTranslationY;
    private double _pinchFocusX;
    private double _pinchFocusY;
    private int _pinchPointerId1 = -1;
    private int _pinchPointerId2 = -1;
    private float _startPanX;
    private float _startPanY;
    private float _density = 1f;
    private Bitmap? _rasterizedBitmap;
    private Drawable? _originalDrawable;
    private int _lastRasterWidth;
    private int _lastRasterHeight;
    private double _lastRasterScale;
    private bool _isRasterized;
    private long _lastGestureTimestamp;

    protected override void ConnectHandler(ImageView platformView)
    {
        base.ConnectHandler(platformView);
        Log("connect");
        platformView.Clickable = true;
        platformView.Focusable = true;
        platformView.FocusableInTouchMode = true;
        platformView.LongClickable = true;
        _density = platformView.Context?.Resources?.DisplayMetrics?.Density ?? 1f;
        platformView.LayoutChange += OnLayoutChange;
        if (VirtualView is RotatableImage view)
        {
            view.PropertyChanged += OnVirtualViewPropertyChanged;
            UpdateRasterization(view, platformView, true);
        }
        platformView.Touch += OnTouch;
    }

    protected override void DisconnectHandler(ImageView platformView)
    {
        platformView.Touch -= OnTouch;
        platformView.LayoutChange -= OnLayoutChange;
        if (VirtualView is RotatableImage view)
            view.PropertyChanged -= OnVirtualViewPropertyChanged;
        DisposeRasterization();
        Log("disconnect");
        base.DisconnectHandler(platformView);
    }

    private void OnLayoutChange(object? sender, Android.Views.View.LayoutChangeEventArgs e)
    {
        if (VirtualView is not RotatableImage view)
            return;

        if (sender is not ImageView platformView)
            return;

        UpdateRasterization(view, platformView, false);
    }

    private void OnVirtualViewPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not RotatableImage view)
            return;

        if (PlatformView is not ImageView platformView)
            return;

        if (e.PropertyName == RotatableImage.EnableRasterizationProperty.PropertyName ||
            e.PropertyName == RotatableImage.RasterizationScaleProperty.PropertyName ||
            e.PropertyName == "Source")
        {
            UpdateRasterization(view, platformView, true);
        }
    }

    private void UpdateRasterization(RotatableImage view, ImageView platformView, bool force)
    {
        if (!view.EnableRasterization)
        {
            RestoreOriginalDrawable(platformView);
            return;
        }

        if (platformView.Width <= 0 || platformView.Height <= 0)
            return;

        var drawable = platformView.Drawable;
        if (drawable is null)
            return;

        if (!IsRasterizedDrawable(drawable))
        {
            _originalDrawable = drawable;
            _isRasterized = false;
        }

        var source = _originalDrawable ?? drawable;
        var scale = Math.Max(1d, view.RasterizationScale);
        var targetWidth = (int)Math.Max(1, platformView.Width * scale);
        var targetHeight = (int)Math.Max(1, platformView.Height * scale);

        if (!force && _isRasterized && targetWidth == _lastRasterWidth && targetHeight == _lastRasterHeight &&
            Math.Abs(_lastRasterScale - scale) < 0.001)
        {
            return;
        }

        RasterizeDrawable(platformView, source, targetWidth, targetHeight, scale);
    }

    private void RasterizeDrawable(ImageView platformView, Drawable source, int targetWidth, int targetHeight, double scale)
    {
        if (targetWidth <= 0 || targetHeight <= 0)
            return;

        var bitmap = Bitmap.CreateBitmap(targetWidth, targetHeight, Bitmap.Config.Argb8888);
        using var canvas = new Canvas(bitmap);
        source.SetBounds(0, 0, targetWidth, targetHeight);
        source.Draw(canvas);
        platformView.SetImageBitmap(bitmap);

        _rasterizedBitmap?.Dispose();
        _rasterizedBitmap = bitmap;
        _isRasterized = true;
        _lastRasterWidth = targetWidth;
        _lastRasterHeight = targetHeight;
        _lastRasterScale = scale;
        Log($"rasterized {targetWidth}x{targetHeight}");
    }

    private void RestoreOriginalDrawable(ImageView platformView)
    {
        if (!_isRasterized || _originalDrawable is null)
            return;

        platformView.SetImageDrawable(_originalDrawable);
        _isRasterized = false;
        _lastRasterWidth = 0;
        _lastRasterHeight = 0;
        _lastRasterScale = 0;
        _rasterizedBitmap?.Dispose();
        _rasterizedBitmap = null;
        Log("rasterize disabled");
    }

    private void DisposeRasterization()
    {
        _rasterizedBitmap?.Dispose();
        _rasterizedBitmap = null;
        _originalDrawable = null;
        _isRasterized = false;
        _lastRasterWidth = 0;
        _lastRasterHeight = 0;
        _lastRasterScale = 0;
    }

    private bool IsRasterizedDrawable(Drawable drawable)
    {
        return drawable is BitmapDrawable bitmapDrawable &&
               _rasterizedBitmap is not null &&
               bitmapDrawable.Bitmap == _rasterizedBitmap;
    }

    private void OnTouch(object? sender, Android.Views.View.TouchEventArgs e)
    {
        if (VirtualView is not RotatableImage view)
            return;

        var ev = e.Event;
        var platformView = sender as Android.Views.View;

        switch (ev.ActionMasked)
        {
            case MotionEventActions.Down:
                ResetAll();
                EnsureMode(view, ev, platformView);
                Log("touch down");
                break;
            case MotionEventActions.PointerDown:
                EnsureMode(view, ev, platformView);
                break;
            case MotionEventActions.Move:
                EnsureMode(view, ev, platformView);
                if (_activeMode == 3)
                    UpdatePan(view, ev, platformView);
                else if (_activeMode == 2)
                    UpdatePinch(view, ev, platformView);
                else if (_activeMode == 1)
                    UpdateRotate(view, ev, platformView);
                break;
            case MotionEventActions.PointerUp:
                if (ev.PointerCount - 1 < 3)
                    ResetPan();
                if (ev.PointerCount - 1 < 2)
                    ResetPinch();
                if (ev.PointerCount - 1 < 1)
                    ResetRotate();
                break;
            case MotionEventActions.Up:
            case MotionEventActions.Cancel:
                ResetAll();
                ResetPan();
                Log("touch end");
                break;
        }

        e.Handled = true;
    }

    private void EnsureMode(RotatableImage view, MotionEvent ev, Android.Views.View? platformView)
    {
        if (ev.PointerCount >= 3)
        {
            if (_activeMode != 3)
            {
                StartPan(view, ev);
                ResetRotate();
                ResetPinch();
            }
            return;
        }

        if (ev.PointerCount == 2)
        {
            if (_activeMode != 2)
            {
                StartPinch(view, ev, platformView);
                ResetRotate();
            }
            return;
        }

        if (ev.PointerCount == 1 && _activeMode != 1)
        {
            StartRotate(view, ev, platformView);
            ResetPinch();
        }
    }

    private void StartRotate(RotatableImage view, MotionEvent ev, Android.Views.View? platformView)
    {
        _rotateStartRotation = view.Rotation;
        _rotateStartAngle = GetAngleToCenter(ev, platformView);
        _activeMode = 1;
        _pinchStartScale = view.Scale;
        _lastGestureTimestamp = 0;
        Log("rotate start");
    }

    private void UpdateRotate(RotatableImage view, MotionEvent ev, Android.Views.View? platformView)
    {
        if (_activeMode != 1)
            return;

        if (!ShouldProcessGesture(view))
            return;

        var angle = GetAngleToCenter(ev, platformView);
        var delta = NormalizeDelta(angle - _rotateStartAngle);
        var deltaDegrees = delta * (180f / (float)Math.PI) * (float)view.RotationSensitivity;
        if (Math.Abs(deltaDegrees) > 0.01)
        {
            var target = NormalizeDegrees(_rotateStartRotation + deltaDegrees);
            if (view.GestureSmoothing > 0)
                view.Rotation = ApplyRotationSmoothing(view.Rotation, target, view.GestureSmoothing);
            else
                view.Rotation = target;
        }
    }

    private void StartPinch(RotatableImage view, MotionEvent ev, Android.Views.View? platformView)
    {
        _pinchPointerId1 = ev.GetPointerId(0);
        _pinchPointerId2 = ev.GetPointerId(1);
        _pinchStartSpan = GetSpan(ev);
        _pinchStartScale = view.Scale;
        _startTranslationX = view.TranslationX;
        _startTranslationY = view.TranslationY;
        var focus = GetMidpoint(ev);
        var center = GetCenter(platformView);
        var focusX = (focus.X / _density) - center.X;
        var focusY = (focus.Y / _density) - center.Y;
        var rotated = RotateToScreen(focusX, focusY, view.Rotation);
        _pinchFocusX = rotated.X;
        _pinchFocusY = rotated.Y;
        _activeMode = 2;
        _lastGestureTimestamp = 0;
        Log("pinch start");
    }

    private void UpdatePinch(RotatableImage view, MotionEvent ev, Android.Views.View? platformView)
    {
        if (_activeMode != 2)
            return;

        if (!ShouldProcessGesture(view))
            return;

        var span = GetSpan(ev);
        if (_pinchStartSpan <= 0 || span <= 0)
            return;

        var ratio = span / _pinchStartSpan;
        var adjusted = 1f + (ratio - 1f) * (float)view.PinchSensitivity;
        var targetScale = Math.Clamp(_pinchStartScale * adjusted, view.MinScale, view.MaxScale);
        var nextScale = view.GestureSmoothing > 0
            ? ApplySmoothing(view.Scale, targetScale, view.GestureSmoothing)
            : targetScale;
        if (Math.Abs(nextScale - view.Scale) > 0.0001)
            view.Scale = nextScale;

        var deltaScale = _pinchStartScale <= 0 ? 1 : nextScale / _pinchStartScale;
        var targetX = _startTranslationX + (1 - deltaScale) * _pinchFocusX;
        var targetY = _startTranslationY + (1 - deltaScale) * _pinchFocusY;
        ApplyTranslation(view, targetX, targetY, platformView);

        if (nextScale <= view.MinScale + MinScaleEpsilon)
        {
            view.TranslationX = 0;
            view.TranslationY = 0;
        }
    }

    private void StartPan(RotatableImage view, MotionEvent ev)
    {
        var centroid = GetCentroid(ev);
        _startPanX = centroid.X;
        _startPanY = centroid.Y;
        _startTranslationX = view.TranslationX;
        _startTranslationY = view.TranslationY;
        _activeMode = 3;
        _lastGestureTimestamp = 0;
        Log("pan start");
    }

    private void UpdatePan(RotatableImage view, MotionEvent ev, Android.Views.View? platformView)
    {
        if (_activeMode != 3)
            return;

        if (!ShouldProcessGesture(view))
            return;

        var centroid = GetCentroid(ev);
        var dx = (centroid.X - _startPanX) / _density;
        var dy = (centroid.Y - _startPanY) / _density;
        // Compensate for element rotation so pan follows viewport axes.
        var compensated = RotateToScreen(dx, dy, view.Rotation);

        var targetX = _startTranslationX + compensated.X * view.PanSensitivity;
        var targetY = _startTranslationY + compensated.Y * view.PanSensitivity;
        ApplyTranslation(view, targetX, targetY, platformView);
    }

    private void ApplyTranslation(RotatableImage view, double targetX, double targetY, Android.Views.View? platformView)
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

    private (double X, double Y) ClampTranslation(RotatableImage view, double targetX, double targetY, Android.Views.View? platformView)
    {
        if (platformView is null || platformView.Width <= 0 || platformView.Height <= 0)
            return (targetX, targetY);

        var viewportWidth = platformView.Width / _density;
        var viewportHeight = platformView.Height / _density;
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

    private float GetAngleToCenter(MotionEvent ev, Android.Views.View? platformView)
    {
        if (ev.PointerCount == 0 || platformView is null)
            return 0;

        var center = GetCenter(platformView);
        var x = (ev.GetX(0) / _density) - center.X;
        var y = (ev.GetY(0) / _density) - center.Y;
        return (float)Math.Atan2(y, x);
    }

    private static (double X, double Y) GetCenter(Android.Views.View? platformView)
    {
        if (platformView is null)
            return (0, 0);

        return (platformView.Width / 2d / platformView.Resources.DisplayMetrics.Density,
                platformView.Height / 2d / platformView.Resources.DisplayMetrics.Density);
    }

    private static (float X, float Y) GetMidpoint(MotionEvent ev)
    {
        if (ev.PointerCount < 2)
            return (0, 0);

        var x = (ev.GetX(0) + ev.GetX(1)) / 2f;
        var y = (ev.GetY(0) + ev.GetY(1)) / 2f;
        return (x, y);
    }

    private float GetSpan(MotionEvent ev)
    {
        if (ev.PointerCount < 2)
            return 0;

        var index1 = 0;
        var index2 = 1;
        if (_pinchPointerId1 != -1 && _pinchPointerId2 != -1)
        {
            var i1 = ev.FindPointerIndex(_pinchPointerId1);
            var i2 = ev.FindPointerIndex(_pinchPointerId2);
            if (i1 != -1 && i2 != -1)
            {
                index1 = i1;
                index2 = i2;
            }
        }

        var x = ev.GetX(index1) - ev.GetX(index2);
        var y = ev.GetY(index1) - ev.GetY(index2);
        return (float)Math.Sqrt(x * x + y * y);
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

    private static (float X, float Y) GetCentroid(MotionEvent ev)
    {
        var count = ev.PointerCount;
        if (count == 0)
            return (0, 0);

        float sumX = 0;
        float sumY = 0;
        for (var i = 0; i < count; i++)
        {
            sumX += ev.GetX(i);
            sumY += ev.GetY(i);
        }

        return (sumX / count, sumY / count);
    }

    private static float NormalizeDelta(float delta)
    {
        while (delta > Math.PI)
            delta -= (float)(2 * Math.PI);
        while (delta < -Math.PI)
            delta += (float)(2 * Math.PI);
        return delta;
    }

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

    private void ResetRotate()
    {
        if (_activeMode == 1)
            Log("rotate end");
        _rotateStartAngle = 0;
        _rotateStartRotation = 0;
        if (_activeMode == 1)
            _activeMode = 0;
    }

    private void ResetPan()
    {
        if (_activeMode == 3)
            Log("pan end");
        _startPanX = 0;
        _startPanY = 0;
        if (_activeMode == 3)
            _activeMode = 0;
    }

    private void ResetPinch()
    {
        if (_activeMode == 2)
            Log("pinch end");
        _pinchPointerId1 = -1;
        _pinchPointerId2 = -1;
        _pinchStartSpan = 0;
        _pinchStartScale = 1;
        if (_activeMode == 2)
            _activeMode = 0;
    }

    private void ResetAll()
    {
        _activeMode = 0;
        _pinchPointerId1 = -1;
        _pinchPointerId2 = -1;
        _pinchStartSpan = 0;
        _rotateStartAngle = 0;
        _pinchStartScale = 1;
        _rotateStartRotation = 0;
        _startTranslationX = 0;
        _startTranslationY = 0;
        _pinchFocusX = 0;
        _pinchFocusY = 0;
        _lastGestureTimestamp = 0;
    }

#if DEBUG
    static partial void LogPlatform(string message)
    {
        Android.Util.Log.Debug("RotatableImage", message);
    }
#endif
}
#endif
