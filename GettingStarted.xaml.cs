namespace LearnWithCircle;

public partial class GettingStarted : ContentPage
{
	private const double TabletWidthThreshold = 600;

	public GettingStarted()
	{
		InitializeComponent();
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);

		if (width <= 0 || height <= 0)
			return;

		bool isLandscape = width > height;
		bool isTablet = width >= TabletWidthThreshold;

		ApplyStyleAdaptations(isTablet, isLandscape);
	}

	private void ApplyStyleAdaptations(bool isTablet, bool isLandscape)
	{
		TitleLabel.FontSize = isTablet ? 28 : 20;
		SubtitleLabel.FontSize = isTablet ? 18 : 14;

		if (isLandscape && !isTablet)
		{
			HeaderSection.Padding = new Thickness(24, 10);
		}
		else
		{
			HeaderSection.Padding = new Thickness(16, 20);
		}
	}

	private async void OnVideoClicked(object sender, EventArgs e)
	{
		if (sender is ImageButton btn && btn.CommandParameter is string url)
		{
			try
			{
				var uri = new Uri(url);
				await Launcher.Default.OpenAsync(uri);
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Failed to open video: {ex.Message}", "OK");
			}
		}
	}
}
