using LearnWithCircle.LearningCircles;

namespace LearnWithCircle;

public partial class MainPage : ContentPage
{
    private const double TabletWidthThreshold = 600;
    private const double CarouselHeightPhone = 320;
    private const double CarouselHeightPhoneLandscape = 220;
    private const double CarouselHeightTablet = 500;
    private const double CarouselHeightTabletLandscape = 420;

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

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0 || height <= 0)
            return;

        bool isLandscape = width > height;
        bool isTablet = width >= TabletWidthThreshold;

        AdaptLayout(width, height, isTablet, isLandscape);
    }

    private void AdaptLayout(double width, double height, bool isTablet, bool isLandscape)
    {
        double carouselHeight = (isTablet, isLandscape) switch
        {
            (true, false)  => CarouselHeightTablet,
            (true, true)   => CarouselHeightTabletLandscape,
            (false, true)  => CarouselHeightPhoneLandscape,
            _              => CarouselHeightPhone
        };
        LearningCircleCarousel.HeightRequest = carouselHeight;

        double titleFontSize    = isTablet ? 28 : 20;
        double subtitleFontSize = isTablet ? 18 : 14;
        double helpFontSize     = isTablet ? 18 : 14;
        double buttonFontSize   = isTablet ? 20 : 16;

        TitleLabel.FontSize    = titleFontSize;
        SubtitleLabel.FontSize = subtitleFontSize;
        HelpLabel.FontSize     = helpFontSize;
        TutorialButton.FontSize = buttonFontSize;

        TutorialButton.Padding = isTablet
            ? new Thickness(24, 14)
            : new Thickness(16, 10);


        double estimatedContentHeight =
            60
            + 80      
            + carouselHeight + 8
            + 120
            + 40;

        double spacerHeight = Math.Max(0, height - estimatedContentHeight);
        FooterSpacer.HeightRequest = spacerHeight;

        if (isLandscape && !isTablet)
        {
            HeaderSection.Padding  = new Thickness(24, 12);
            TutorialSection.Padding = new Thickness(0, 12, 0, 8);
        }
        else
        {
            HeaderSection.Padding  = new Thickness(16, 20);
            TutorialSection.Padding = new Thickness(0, 24, 0, 16);
        }
    }

    private void OnCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e)
    {
        if (e.CurrentItem is LearningCircleModel circle)
            SetHeader(circle);
    }

    private void SetHeader(LearningCircleModel circle)
    {
        TitleLabel.Text    = circle.Title;
        SubtitleLabel.Text = circle.Subtitle;
    }

    private async void OnTutorialClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///GettingStarted");
    }

    private async void OnCircleTapped(object sender, EventArgs e)
    {
        if (sender is not Image image || image.BindingContext is not LearningCircleModel circle)
            return;

        await Shell.Current.GoToAsync(nameof(LearningCircleDetailPage), new Dictionary<string, object>
        {
            ["Circle"] = circle
        });
    }
}