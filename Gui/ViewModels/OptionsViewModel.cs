using ReactiveUI;

namespace ImageFunctions.Gui.ViewModels;

public class OptionsViewModel : ViewModelBase
{
	int _initialLayerWidth;
	public int InitialLayerWidth {
		get => _initialLayerWidth;
		set => this.RaiseAndSetIfChanged(ref _initialLayerWidth, value);
	}

	int _initialLayerHeight;
	public int InitialLayerHeight {
		get => _initialLayerHeight;
		set => this.RaiseAndSetIfChanged(ref _initialLayerHeight, value);
	}

	int _maxNumberThreads;
	public int MaxNumberThreads {
		get => _maxNumberThreads;
		set => this.RaiseAndSetIfChanged(ref _maxNumberThreads, value);
	}

}