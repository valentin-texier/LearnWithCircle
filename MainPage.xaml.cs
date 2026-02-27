using LearnWithCircle.LearningCircles;

namespace LearnWithCircle;

public partial class MainPage : ContentPage
{
    private const double TabletWidthThreshold = 600;

    private const double MinCarouselPhone           = 240;
    private const double MinCarouselPhoneLandscape  = 160;
    private const double MinCarouselTablet          = 380;
    private const double MinCarouselTabletLandscape = 300;

    private double _headerMeasuredHeight   = 0;
    private double _tutorialMeasuredHeight = 0;
    private double _footerMeasuredHeight   = 0;

    public MainPage()
    {
        InitializeComponent();

        HeaderSection.SizeChanged   += OnFixedZoneSizeChanged;
        TutorialSection.SizeChanged += OnFixedZoneSizeChanged;
        FooterLabel.SizeChanged     += OnFixedZoneSizeChanged;

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

    private void OnFixedZoneSizeChanged(object? sender, EventArgs e)
    {
        if (sender == HeaderSection)
            _headerMeasuredHeight = HeaderSection.Height;
        else if (sender == TutorialSection)
            _tutorialMeasuredHeight = TutorialSection.Height;
        else if (sender == FooterLabel)
            _footerMeasuredHeight = FooterLabel.Height + 20;

        if (_headerMeasuredHeight > 0 && _tutorialMeasuredHeight > 0 && _footerMeasuredHeight > 0
            && Width > 0 && Height > 0)
        {
            UpdateCarouselHeight(Width, Height);
        }
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (width <= 0 || height <= 0)
            return;

        bool isLandscape = width > height;
        bool isTablet    = width >= TabletWidthThreshold;

        ApplyStyleAdaptations(isTablet, isLandscape);
        UpdateCarouselHeight(width, height);
    }

    private void ApplyStyleAdaptations(bool isTablet, bool isLandscape)
    {
        TitleLabel.FontSize     = isTablet ? 28 : 20;
        SubtitleLabel.FontSize  = isTablet ? 18 : 14;
        HelpLabel.FontSize      = isTablet ? 18 : 14;
        TutorialButton.FontSize = isTablet ? 20 : 16;
        TutorialButton.Padding  = isTablet ? new Thickness(24, 14) : new Thickness(16, 10);

        if (isLandscape && !isTablet)
        {
            HeaderSection.Padding   = new Thickness(24, 10);
            TutorialSection.Padding = new Thickness(0, 10, 0, 8);
        }
        else
        {
            HeaderSection.Padding   = new Thickness(16, 20);
            TutorialSection.Padding = new Thickness(0, 24, 0, 16);
        }

        double peekInset = (isTablet, isLandscape) switch
        {
            (true,  _)    => 80,
            (false, true) => 55,
            _             => 40
        };
        LearningCircleCarousel.PeekAreaInsets = new Thickness(peekInset, 0);
    }

    private void UpdateCarouselHeight(double width, double height)
    {
        bool isLandscape = width > height;
        bool isTablet    = width >= TabletWidthThreshold;

        if (_headerMeasuredHeight <= 0 || _tutorialMeasuredHeight <= 0 || _footerMeasuredHeight <= 0)
            return;

        double fixedHeight = _headerMeasuredHeight
                           + _tutorialMeasuredHeight
                           + _footerMeasuredHeight
                           + 8;

        double available = height - fixedHeight;

        double minCarousel = (isTablet, isLandscape) switch
        {
            (true,  false) => MinCarouselTablet,
            (true,  true)  => MinCarouselTabletLandscape,
            (false, true)  => MinCarouselPhoneLandscape,
            _              => MinCarouselPhone
        };

        LearningCircleCarousel.HeightRequest = Math.Max(minCarousel, available);
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