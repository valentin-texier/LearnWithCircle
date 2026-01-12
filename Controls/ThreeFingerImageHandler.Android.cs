#if ANDROID
using Android.Views;
using Android.Widget;

namespace LearnWithCircle.Controls;

public partial class ThreeFingerImageHandler
{
    private int _tapTimeout;
    private float _touchSlop;
    private bool _hadThreeFingers;
    private bool _isCanceled;
    private long _tapStart;
    private float _startX;
    private float _startY;

    protected override void ConnectHandler(ImageView platformView)
    {
        base.ConnectHandler(platformView);
        var config = ViewConfiguration.Get(platformView.Context);
        _tapTimeout = ViewConfiguration.TapTimeout;
        _touchSlop = config.ScaledTouchSlop;
        platformView.Touch += OnTouch;
    }

    protected override void DisconnectHandler(ImageView platformView)
    {
        platformView.Touch -= OnTouch;
        base.DisconnectHandler(platformView);
    }

    private void OnTouch(object? sender, Android.Views.View.TouchEventArgs e)
    {
        if (VirtualView is not ThreeFingerImage view)
            return;

        var ev = e.Event;
        switch (ev.ActionMasked)
        {
            case MotionEventActions.Down:
                Reset();
                break;
            case MotionEventActions.PointerDown:
                if (ev.PointerCount == 3)
                {
                    _hadThreeFingers = true;
                    _tapStart = ev.EventTime;
                    var avg = GetAveragePosition(ev);
                    _startX = avg.X;
                    _startY = avg.Y;
                }
                else if (ev.PointerCount > 3)
                {
                    _isCanceled = true;
                }
                break;
            case MotionEventActions.Move:
                if (_hadThreeFingers && !_isCanceled)
                {
                    var avg = GetAveragePosition(ev);
                    var dx = avg.X - _startX;
                    var dy = avg.Y - _startY;
                    if ((dx * dx + dy * dy) > _touchSlop * _touchSlop)
                        _isCanceled = true;
                }
                break;
            case MotionEventActions.Up:
                if (_hadThreeFingers && !_isCanceled)
                {
                    if ((ev.EventTime - _tapStart) <= _tapTimeout)
                        view.RaiseThreeFingerTapped();
                }
                Reset();
                break;
            case MotionEventActions.Cancel:
                Reset();
                break;
        }
    }

    private void Reset()
    {
        _hadThreeFingers = false;
        _isCanceled = false;
        _tapStart = 0;
        _startX = 0;
        _startY = 0;
    }

    private static (float X, float Y) GetAveragePosition(MotionEvent ev)
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
}
#endif
