using System.Diagnostics;
using Avalonia;
using Avalonia.Input;
using Avalonia.Styling;
using ReactiveUI;

namespace ImageFunctions.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	public bool ToggleThemeClick()
	{
		var app = Application.Current;
		if (app is not null) {
			var theme = app.ActualThemeVariant;
			app.RequestedThemeVariant = theme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
		}
		return true;
	}

	System.Timers.Timer StatusTextTimer = null;
	const int StatusTextLifetimeMs = 2000;

	public void UpdateStatusText(string text = "", bool startTimer = false)
	{
		//Trace.WriteLine($"UpdateStatusText T:'{text}' E:{(expired?"Y":"N")}");
		if (StatusTextTimer == null) {
			StatusTextTimer = new() {
				AutoReset = false,
				Interval = StatusTextLifetimeMs
			};
			StatusTextTimer.Elapsed += (s,e) => UpdateStatusText("",false);
		}

		StatusText = text;

		if (startTimer) {
			StatusTextTimer.Start();
		}
		else {
			StatusTextTimer.Stop();
		}
	}

	string StatusTextValue = $"Welcome to {nameof(ImageFunctions)}";
	public string StatusText {
		get {
			return StatusTextValue;
		}
		set {
			this.RaiseAndSetIfChanged(ref StatusTextValue, value);
		}
	}
}
