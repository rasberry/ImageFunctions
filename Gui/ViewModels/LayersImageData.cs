using Avalonia.Media.Imaging;
using ImageFunctions.Core;
using ReactiveUI;

namespace ImageFunctions.Gui.ViewModels;

public class LayersImageData : ViewModelBase
{
	public LayersImageData(ILayers layers)
	{
		LayersInst = layers;
	}

	Bitmap _image;
	public Bitmap Image {
		get => _image;
		set => this.RaiseAndSetIfChanged(ref _image, value);
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

	readonly ILayers LayersInst;

	public void LayerMoveDown()
	{
		int index = LayersInst.IndexOf(Id);
		if(index < 0) { return; } //Not found :?
		CheckUpDownEnabled();
		if(DownEnabled) {
			//Trace.WriteLine($"{nameof(LayerMoveDown)} id:{Id} ix:{index} to:{index+1} h:{GetHashCode()}");
			LayersInst.Move(index, index + 1);
		}
	}

	public void LayerMoveUp()
	{
		int index = LayersInst.IndexOf(Id);
		if(index < 0) { return; } //Not found :?
		CheckUpDownEnabled();
		if(UpEnabled) {
			//Trace.WriteLine($"{nameof(LayerMoveUp)} id:{Id} ix:{index} to:{index-1} h:{GetHashCode()}");
			LayersInst.Move(index, index - 1);
		}
	}

	public void LayerDelete()
	{
		int index = LayersInst.IndexOf(Id);
		if(index < 0) { return; } //Not found :?
		LayersInst.PopAt(index);
	}

	public void CheckUpDownEnabled()
	{
		int count = LayersInst.Count;
		int index = LayersInst.IndexOf(Id);
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
