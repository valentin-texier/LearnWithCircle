using LearnWithCircle.LearningCircles;
using System.Collections;

namespace LearnWithCircle;

public partial class MainPage : ContentPage
{
    private const double TabletWidthThreshold = 600;
    private const double MinCarouselPhone = 220;
    private const double MinCarouselPhoneLandscape = 160;
    private const double MinCarouselTablet = 340;
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
        UpdateArrows();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0 || height <= 0)
            return;

        MainLayout.MinimumHeightRequest = height;

        bool isLandscape = width > height;
        bool isTablet = Math.Min(width, height) >= TabletWidthThreshold;

        ApplyStyleAdaptations(isTablet, isLandscape);

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

        double fixedHeaderHeight = isTablet ? 180 : (isLandscape ? 120 : 150);
        HeaderBorder.HeightRequest = fixedHeaderHeight;

        if (isLandscape && !isTablet)
        {
            HeaderSection.Padding = new Thickness(24, 4);
            TutorialSection.Padding = new Thickness(0, 24, 0, 8);
            Resources["CircleMargin"] = new Thickness(200, 60);
        }
        else if (!isTablet)
        {
            HeaderSection.Padding = new Thickness(16, 8);
            TutorialSection.Padding = new Thickness(0, 32, 0, 10);
            Resources["CircleMargin"] = new Thickness(12, 0);
        }
        else
        {
            HeaderSection.Padding = new Thickness(16, 12);
            TutorialSection.Padding = new Thickness(0, 32, 0, 6);
            Resources["CircleMargin"] = new Thickness(12, 0);
        }
    }

    private void OnCurrentItemChanged(object? sender, CurrentItemChangedEventArgs e)
    {
        if (e.CurrentItem is LearningCircleModel circle)
            SetHeader(circle);
            
        UpdateArrows();
    }

    private void UpdateArrows()
    {
        if (LearningCircleCarousel.ItemsSource is not IList items)
            return;

        LeftArrow.IsVisible = LearningCircleCarousel.Position > 0;
        RightArrow.IsVisible = LearningCircleCarousel.Position < items.Count - 1;
    }

    private void OnScrollLeft(object sender, EventArgs e)
    {
        if (LearningCircleCarousel.Position > 0)
        {
            LearningCircleCarousel.Position--;
        }
    }

    private void OnScrollRight(object sender, EventArgs e)
    {
        if (LearningCircleCarousel.ItemsSource is IList items && LearningCircleCarousel.Position < items.Count - 1)
        {
            LearningCircleCarousel.Position++;
        }
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

    private async void OnLicenseClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///LicensePage");
    }
}