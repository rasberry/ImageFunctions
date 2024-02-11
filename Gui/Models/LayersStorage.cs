using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using ImageFunctions.Core;
using ImageFunctions.Gui.Helpers;
using ImageFunctions.Gui.ViewModels;

namespace ImageFunctions.Gui.Models;

public class LayersStorage : ObservableCollection<LayersImageData>, ICollectionSymbiote<SingleLayerItem>
{
	public LayersStorage(Func<ICanvas,Bitmap> converter)
	{
		Converter = converter;
	}
	readonly Func<ICanvas,Bitmap> Converter;
	public ILayers Parent { get; set; }

	public void Add(SingleLayerItem item)
	{
		this.Add(Make(item));
	}

	public void Insert(int index, SingleLayerItem item)
	{
		this.InsertItem(index, Make(item));
	}

	public bool Remove(SingleLayerItem item)
	{
		for(int i = 0; i < this.Count; i++) {
			var entry = this[i];
			if (entry.Id == item.Id) {
				this.RemoveItem(i);
				entry.Image?.Dispose();
				return true;
			}
		}
		return false;
	}

	public void Set(int index, SingleLayerItem item)
	{
		var entry = this[index];
		this[index] = Make(item);
		entry.Image?.Dispose();
	}

	LayersImageData Make(SingleLayerItem item)
	{
		var data = new LayersImageData {
			Image = Converter(item.Canvas),
			Name = item.Name,
			Id = item.Id,
			Layers = Parent
		};
		return data;
	}
}