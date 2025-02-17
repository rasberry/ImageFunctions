using Avalonia.Styling;
using ImageFunctions.Gui.Models;
using ReactiveUI;

namespace ImageFunctions.Gui.ViewModels;

public class OptionsViewModel : ViewModelBase
{
	public OptionsViewModel()
	{
		Load();

		DelayTimer = new System.Timers.Timer(TimerDelay);
		DelayTimer.Elapsed += (s, e) => {
			//Trace.WriteLine("Options Timer Save");
			DelayTimer.Stop();
			Manager.Save();
		};
		DelayTimer.AutoReset = false;
		DelayTimer.Enabled = false;

		PropertyChanged += (s, e) => {
			//Trace.WriteLine($"Options Trigger Property {e.PropertyName}");
			DelayTimer.Interval = TimerDelay;
			DelayTimer.Start();
		};
	}

	void Load()
	{
		Manager.Load();

		//populate and/or set defaults
		if(!Manager.TryGetAs(nameof(InitialLayerWidth), out _initialLayerWidth)) {
			_initialLayerWidth = 512;
		}
		if(!Manager.TryGetAs(nameof(InitialLayerHeight), out _initialLayerHeight)) {
			_initialLayerHeight = 512;
		}
		if(!Manager.TryGetAs(nameof(MaxNumberThreads), out _maxNumberThreads)) {
			_maxNumberThreads = null;
		}
		if(!Manager.TryGetAs(nameof(WhichTheme), out _whichTheme)) {
			_whichTheme = ThemeKind.System;
		}

		ToggleTheme(_whichTheme);
	}

	int _initialLayerWidth;
	public int InitialLayerWidth {
		get { return _initialLayerWidth; }
		set {
			Manager.Set(nameof(InitialLayerWidth), value);
			this.RaiseAndSetIfChanged(ref _initialLayerWidth, value);
		}
	}

	int _initialLayerHeight;
	public int InitialLayerHeight {
		get { return _initialLayerHeight; }
		set {
			Manager.Set(nameof(InitialLayerHeight), value);
			this.RaiseAndSetIfChanged(ref _initialLayerHeight, value);
		}
	}

	int? _maxNumberThreads;
	public int? MaxNumberThreads {
		get { return _maxNumberThreads; }
		set {
			Manager.Set(nameof(MaxNumberThreads), value);
			this.RaiseAndSetIfChanged(ref _maxNumberThreads, value);
		}
	}

	ThemeKind _whichTheme;
	public ThemeKind WhichTheme {
		get { return _whichTheme; }
		set {
			Manager.Set(nameof(WhichTheme), (int)value);
			ToggleTheme(value);
			this.RaiseAndSetIfChanged(ref _whichTheme, value);
		}
	}

	readonly SettingsManager Manager = new();
	readonly System.Timers.Timer DelayTimer;
	const int TimerDelay = 1000; //milliseconds

	void ToggleTheme(ThemeKind selected)
	{
		var app = Avalonia.Application.Current;
		if(app != null) {
			var current = app.ActualThemeVariant;
			var request = selected switch {
				ThemeKind.Dark => ThemeVariant.Dark,
				ThemeKind.Light => ThemeVariant.Light,
				_ => ThemeVariant.Default
			};

			// Trace.WriteLine($"ToggleTheme c={current} r={request}");
			if(current != request) {
				app.RequestedThemeVariant = request;
			}
		}
	}
}

public enum ThemeKind
{
	System = 0,
	Light = 1,
	Dark = 2
}
