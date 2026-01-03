using LearnWithCircle.LearningCircles;

namespace LearnWithCircle;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        var circles = await LearningCircleService.LoadCirclesAsync();
        if (circles.Count == 0)
            return;

        LearningCircleCarousel.ItemsSource = circles;

        SetHeader(circles[0]);
    }

    private void OnCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e)
    {
        if (e.CurrentItem is LearningCircleModel circle)
            SetHeader(circle);
    }

    private void SetHeader(LearningCircleModel circle)
    {
        TitleLabel.Text = circle.Title;
        SubtitleLabel.Text = circle.Subtitle;
    }

    private void OnTutorialTapped(object sender, EventArgs e)
    {
        Uri uri = new Uri("https://www.youtube.com/watch?v=BkPuuFP6SCc&list=PLeARUV9L8_w0MdAhJVIREsZZuHZ4ldv9o&index=1");
        Launcher.Default.OpenAsync(uri);
    }
}
