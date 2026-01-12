#if IOS || MACCATALYST
using UIKit;

namespace LearnWithCircle.Controls;

public partial class ThreeFingerImageHandler
{
    private UITapGestureRecognizer? _tapRecognizer;

    protected override void ConnectHandler(UIImageView platformView)
    {
        base.ConnectHandler(platformView);

        platformView.UserInteractionEnabled = true;
        _tapRecognizer = new UITapGestureRecognizer(() =>
        {
            if (VirtualView is ThreeFingerImage view)
                view.RaiseThreeFingerTapped();
        })
        {
            NumberOfTouchesRequired = 3,
            NumberOfTapsRequired = 1
        };
        _tapRecognizer.CancelsTouchesInView = false;
        platformView.AddGestureRecognizer(_tapRecognizer);
    }

    protected override void DisconnectHandler(UIImageView platformView)
    {
        if (_tapRecognizer is not null)
        {
            platformView.RemoveGestureRecognizer(_tapRecognizer);
            _tapRecognizer.Dispose();
            _tapRecognizer = null;
        }

        base.DisconnectHandler(platformView);
    }
}
#endif
