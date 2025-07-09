using ReactiveUI;

namespace ImageFunctions.Gui.ViewModels;

public class OverlayViewModel : ViewModelBase
{
	bool _isPopupVisible = false;
	public bool IsPopupVisible {
		get => _isPopupVisible;
		set => this.RaiseAndSetIfChanged(ref _isPopupVisible, value);
	}

	double _completionMin = 0.0;
	public double CompletionMin {
		get => _completionMin;
		set => this.RaiseAndSetIfChanged(ref _completionMin, value);
	}

	double _completionMax = 1.0;
	public double CompletionMax {
		get => _completionMax;
		set => this.RaiseAndSetIfChanged(ref _completionMax, value);
	}

	double _progressAmount = 0.0;
	public double ProgressAmount {
		get => _progressAmount;
		set => this.RaiseAndSetIfChanged(ref _progressAmount, value);
	}

	string _label = "";
	public string Label {
		get => _label;
		set => this.RaiseAndSetIfChanged(ref _label, value);
	}

	public delegate void StopJobEventHandler(object sender, EventArgs e);
	public event StopJobEventHandler OnStopJob;

	public void StopJob()
	{
		var args = new EventArgs();
		OnStopJob?.Invoke(this, args);
	}
}
