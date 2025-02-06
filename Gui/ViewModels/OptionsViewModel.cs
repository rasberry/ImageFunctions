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

	readonly SettingsManager Manager = new();
	readonly System.Timers.Timer DelayTimer;
	const int TimerDelay = 1000; //milliseconds
}
