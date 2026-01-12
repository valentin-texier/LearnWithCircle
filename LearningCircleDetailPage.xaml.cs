using LearnWithCircle.LearningCircles;

namespace LearnWithCircle;

public partial class LearningCircleDetailPage : ContentPage, IQueryAttributable
{
    private const double MinScale = 1;
    private const double MaxScale = 5;
    private const double RotationStep = 30;

    private double _startScale = 1;

    public LearningCircleDetailPage()
    {
        InitializeComponent();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Circle", out var value) && value is LearningCircleModel circle)
        {
            BindingContext = circle;
            Title = circle.Title;
        }
    }

    private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        if (sender is not View view)
            return;

        if (e.Status == GestureStatus.Started)
        {
            _startScale = view.Scale;
            return;
        }

        if (e.Status == GestureStatus.Running)
        {
            var newScale = _startScale * e.Scale;
            view.Scale = Math.Clamp(newScale, MinScale, MaxScale);
        }
    }

    private void OnThreeFingerTapped(object sender, EventArgs e)
    {
        if (sender is not View view)
            return;

        view.Rotation = (view.Rotation + RotationStep) % 360;
    }
}
