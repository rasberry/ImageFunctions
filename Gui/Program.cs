using Avalonia;
using Avalonia.ReactiveUI;
using ImageFunctions.Core;

namespace ImageFunctions.Gui;

sealed class Program
{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	static void Main(string[] args)
	{
		try {
			//PluginSetup();
			BuildAvaloniaApp()
				.StartWithClassicDesktopLifetime(args);
		}
		finally {
			Cleanup();
		}
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	static AppBuilder BuildAvaloniaApp()
	{
		return AppBuilder
			.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace()
			.UseReactiveUI();
	}


	static void PluginSetup()
	{
		Register = new Register();
		PluginLoader.LoadAllPlugins(Register);
	}

	public static void Cleanup()
	{
		if (Register != null) {
			Register.Dispose();
			Register = null;
		}
	}

	static Register Register;
}
