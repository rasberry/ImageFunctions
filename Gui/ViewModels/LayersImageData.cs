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
			Trace.WriteLine($"{nameof(LayersImageData)} get_{nameof(Image)}");
			return _image;
		}
		set {
			Trace.WriteLine($"{nameof(LayersImageData)} set_{nameof(Image)}");
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

	public void LayerMoveUp()
	{
		int index = Layers.IndexOf(Id);
		if (index < 0) { return; } //Not found :?
		if (index + 1 < Layers.Count) {
			Trace.WriteLine($"{nameof(LayerMoveUp)} id:{Id} ix:{index}");
			Layers.Move(index,index + 1);
		}
	}

	public void LayerMoveDown()
	{
		int index = Layers.IndexOf(Id);
		if (index < 0) { return; } //Not found :?
		if (index - 1 >= 0) {
			Trace.WriteLine($"{nameof(LayerMoveDown)} id:{Id} ix:{index}");
			Layers.Move(index,index - 1);
		}
	}
}