using LearnWithCircle.LearningCircles;

namespace LearnWithCircle;

public partial class LearningCircleDetailPage : ContentPage, IQueryAttributable
{
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

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0 || height <= 0)
            return;
            
        double size = Math.Min(width, height);
        WheelImage.WidthRequest  = size;
        WheelImage.HeightRequest = size;
    }
}