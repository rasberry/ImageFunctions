#if false
using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Media.Imaging;
using ImageFunctions.Core;
using ImageFunctions.Gui.Helpers;
using ImageFunctions.Gui.ViewModels;

namespace ImageFunctions.Gui.Models;

public class LayersStorage : ObservableCollection<LayersImageData>, ICollectionSymbiote<ISingleLayerItem>
{
	public LayersStorage(Func<ICanvas,Bitmap> converter)
	{
		Converter = converter;
	}
	readonly Func<ICanvas,Bitmap> Converter;
	public ILayers Parent { get; set; }

	public void Add(ISingleLayerItem item)
	{
		//Trace.WriteLine($"{nameof(LayersStorage)} {nameof(Add)} oid:{item?.Id}");
		this.Add(Make(item));
	}

	public void Insert(int index, ISingleLayerItem item)
	{
		//Trace.WriteLine($"{nameof(LayersStorage)} {nameof(Insert)} i:{index} oid:{item?.Id}");
		this.InsertItem(index, Make(item));
	}

	public bool Remove(ISingleLayerItem item)
	{
		//Trace.WriteLine($"{nameof(LayersStorage)} {nameof(Remove)} oid:{item?.Id}");
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

	public void Set(int index, ISingleLayerItem item)
	{
		//Trace.WriteLine($"{nameof(LayersStorage)} {nameof(Set)} i:{index} oid:{item?.Id}");
		var entry = this[index];
		this[index] = Make(item);
		entry.Image?.Dispose();
	}

	LayersImageData Make(ISingleLayerItem item)
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
#endif