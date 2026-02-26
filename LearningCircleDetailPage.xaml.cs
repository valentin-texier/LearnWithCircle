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

}
