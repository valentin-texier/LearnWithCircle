using Microsoft.Maui.ApplicationModel;

namespace LearnWithCircle;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(LearningCircleDetailPage), typeof(LearningCircleDetailPage));
	}

    private async void OnWikipediaClicked(object sender, EventArgs e)
    {
        await Launcher.OpenAsync(new Uri("https://w.wiki/FGiY"));
    }

	private async void OnTeacherClicked(object sender, EventArgs e)
    {
        await Launcher.OpenAsync(new Uri("https://m-bakni.github.io/LearnwithCircle/"));
    }
}
