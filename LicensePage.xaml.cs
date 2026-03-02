namespace LearnWithCircle;

public partial class LicensePage : ContentPage
{
	public LicensePage()
	{
		InitializeComponent();
	}

	private async void OnLinkTapped(object sender, EventArgs e)
	{
		try
		{
			var url = "https://creativecommons.org/licenses/by/4.0/legalcode";
			var uri = new Uri(url);
			await Launcher.Default.OpenAsync(uri);
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to open link: {ex.Message}", "OK");
		}
	}
}