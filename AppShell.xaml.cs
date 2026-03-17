using Microsoft.Maui.ApplicationModel;

namespace LearnWithCircle;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(LearningCircleDetailPage), typeof(LearningCircleDetailPage));
        Routing.RegisterRoute(nameof(GettingStarted), typeof(GettingStarted));
        Routing.RegisterRoute(nameof(LicensePage), typeof(LicensePage));
	}

    private async void OnWikipediaClicked(object sender, EventArgs e)
    {
        await Launcher.OpenAsync(new Uri("https://w.wiki/FGiY"));
    }

	private async void OnTeacherClicked(object sender, EventArgs e)
    {
        await Launcher.OpenAsync(new Uri("https://m-bakni.github.io/LearnwithCircle/"));
    }

    private async void OnGettingStartedClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(GettingStarted));
    }

    private async void OnLicenseClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LicensePage));
    }
}
