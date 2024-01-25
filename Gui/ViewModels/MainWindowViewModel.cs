using Avalonia;
using Avalonia.Styling;

namespace ImageFunctions.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	#pragma warning disable CA1822 // Mark members as static
	public string Greeting => "Welcome to Avalonia!";
	#pragma warning restore CA1822 // Mark members as static

	public bool ToggleThemeClick()
	{
		Console.WriteLine("test");
		var app = Application.Current;
		if (app is not null) {
			var theme = app.ActualThemeVariant;
			app.RequestedThemeVariant = theme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
		}
		return true;
	}
}
