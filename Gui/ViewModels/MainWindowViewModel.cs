﻿using Avalonia;
using Avalonia.Styling;
using ReactiveUI;

namespace ImageFunctions.Gui.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
	string StatusTextValue = $"Welcome to {nameof(ImageFunctions)}";
	public string StatusText {
		get =>  StatusTextValue;
		set => this.RaiseAndSetIfChanged(ref StatusTextValue, value);
	}

	string CommandTextValue = "";
	public string CommandText {
		get => CommandTextValue;
		set => this.RaiseAndSetIfChanged(ref CommandTextValue, value);
	}

	string UsageTextValue = "";
	public string UsageText {
		get => UsageTextValue;
		set => this.RaiseAndSetIfChanged(ref UsageTextValue, value);
	}

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

	// The behavior is to show the text as long as the control is still under the pointer
	// but wait some time before hiding the text after the pointer leaves
	public void UpdateStatusText(string text = "", bool startTimer = false)
	{
		//Trace.WriteLine($"UpdateStatusText T:'{text}' E:{(expired?"Y":"N")}");
		if (StatusTextTimer == null) {
			StatusTextTimer = new() {
				AutoReset = false,
				Interval = StatusTextLifetimeMs
			};
			//this clears the status after some time
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
}
