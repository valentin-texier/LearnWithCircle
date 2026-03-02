using LearnWithCircle.LearningCircles;

namespace LearnWithCircle;

public partial class MainPage : ContentPage
{
    private const double TabletWidthThreshold = 600;

    private const double MinCarouselPhone = 240;
    private const double MinCarouselPhoneLandscape = 160;
    private const double MinCarouselTablet = 380;
    private const double MinCarouselTabletLandscape = 300;

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

        // Astuce : On force la Grid à faire au moins la taille de l'écran.
        // Cela permet à la Row="1" (Carousel) avec "*" de prendre tout l'espace disponible
        // sans avoir besoin de calculs manuels.
        MainLayout.MinimumHeightRequest = height;

        bool isLandscape = width > height;
        bool isTablet = width >= TabletWidthThreshold;

        ApplyStyleAdaptations(isTablet, isLandscape);

        // On définit juste la taille MINIMUM du carrousel.
        // La Grid lui donnera plus d'espace si disponible (grâce au Row="*").
        // Si l'espace manque, le ScrollView s'activera.
        LearningCircleCarousel.MinimumHeightRequest = (isTablet, isLandscape) switch
        {
            (true, false) => MinCarouselTablet,
            (true, true) => MinCarouselTabletLandscape,
            (false, true) => MinCarouselPhoneLandscape,
            _ => MinCarouselPhone
        };
    }

    private void ApplyStyleAdaptations(bool isTablet, bool isLandscape)
    {
        TitleLabel.FontSize = isTablet ? 28 : 20;
        SubtitleLabel.FontSize = isTablet ? 18 : 14;
        HelpLabel.FontSize = isTablet ? 18 : 14;
        TutorialButton.FontSize = isTablet ? 20 : 16;
        TutorialButton.Padding = isTablet ? new Thickness(24, 14) : new Thickness(16, 10);

        if (isLandscape && !isTablet)
        {
            HeaderSection.Padding = new Thickness(24, 10);
            TutorialSection.Padding = new Thickness(0, 10, 0, 8);
        }
        else
        {
            HeaderSection.Padding = new Thickness(16, 20);
            TutorialSection.Padding = new Thickness(0, 24, 0, 16);
        }

        double peekInset = (isTablet, isLandscape) switch
        {
            (true, _) => 80,
            (false, true) => 55,
            _ => 40
        };
        LearningCircleCarousel.PeekAreaInsets = new Thickness(peekInset, 0);
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