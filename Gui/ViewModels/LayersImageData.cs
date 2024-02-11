using System.Diagnostics;
using Avalonia.Media.Imaging;
using ImageFunctions.Core;

namespace ImageFunctions.Gui.ViewModels;

public class LayersImageData : ViewModelBase
{
	public Bitmap Image { get; set; }
	public string Name { get; set; }
	public uint Id { get; set; }
	public ILayers Layers { get; set; }

	public void LayerMoveUp()
	{
		int index = Layers.IndexOf(Id);
		if (index < 0) {
			Trace.WriteLine($"{nameof(LayerMoveUp)} could not find id:{Id}");
			return;
		}
		if (index - 1 >= 0) {
			Trace.WriteLine($"{nameof(LayerMoveUp)} id:{Id} ix:{index}");
			Layers.Move(index,index - 1);
		}
	}

	public void LayerMoveDown()
	{
		int index = Layers.IndexOf(Id);
		if (index < 0) {
			Trace.WriteLine($"{nameof(LayerMoveUp)} could not find id:{Id}");
			return;
		}

		if (index + 1 < Layers.Count) {
			Trace.WriteLine($"{nameof(LayerMoveDown)} id:{Id} ix:{index}");
			Layers.Move(index,index + 1);
		}
	}
}