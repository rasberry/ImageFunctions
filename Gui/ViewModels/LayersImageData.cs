using System.Diagnostics;
using Avalonia.Media.Imaging;
using ImageFunctions.Core;
using ReactiveUI;

namespace ImageFunctions.Gui.ViewModels;

public class LayersImageData : ViewModelBase
{
	Bitmap _image;
	public Bitmap Image {
		get {
			Trace.WriteLine($"{nameof(LayersImageData)} get_{nameof(Image)} {_image.GetHashCode()}");
			return _image;
		}
		set {
			Trace.WriteLine($"{nameof(LayersImageData)} set_{nameof(Image)} {value.GetHashCode()}");
			this.RaiseAndSetIfChanged(ref _image, value);
		}
	}

	string _name;
	public string Name {
		get => _name;
		set => this.RaiseAndSetIfChanged(ref _name, value);
	}

	uint _id;
	public uint Id {
		get => _id;
		set => this.RaiseAndSetIfChanged(ref _id, value);
	}

	//this shouldn't change after the initial set
	public ILayers Layers { get; set; }

	public void LayerMoveDown()
	{
		int index = Layers.IndexOf(Id);
		if (index < 0) { return; } //Not found :?
		CheckUpDownEnabled();
		if (DownEnabled) {
			Trace.WriteLine($"{nameof(LayerMoveDown)} id:{Id} ix:{index} to:{index+1}");
			Layers.Move(index,index + 1);
		}
	}

	public void LayerMoveUp()
	{
		int index = Layers.IndexOf(Id);
		if (index < 0) { return; } //Not found :?
		CheckUpDownEnabled();
		if (UpEnabled) {
			Trace.WriteLine($"{nameof(LayerMoveUp)} id:{Id} ix:{index} to:{index-1}");
			Layers.Move(index,index - 1);
		}
	}

	public void CheckUpDownEnabled()
	{
		int count = Layers.Count;
		int index = Layers.IndexOf(Id);
		UpEnabled = count > 1 && index - 1 >= 0;
		DownEnabled = count > 1 && index + 1 < count;
	}

	bool _upEnabled;
	public bool UpEnabled {
		get => _upEnabled;
		set => this.RaiseAndSetIfChanged(ref _upEnabled, value);
	}

	bool _downEnabled;
	public bool DownEnabled {
		get => _downEnabled;
		set => this.RaiseAndSetIfChanged(ref _downEnabled, value);
	}
}