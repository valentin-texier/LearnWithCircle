namespace LearnWithCircle;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
        window.Page.FlowDirection = FlowDirection.RightToLeft;
        return window;
		
	}
}