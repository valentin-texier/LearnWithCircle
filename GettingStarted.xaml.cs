namespace LearnWithCircle;

public partial class GettingStarted : ContentPage
{
	public GettingStarted()
	{
		InitializeComponent();
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
