using System.Collections;
using System.Diagnostics;
using Avalonia.Media.Imaging;
using ImageFunctions.Core;
using ImageFunctions.Gui.ViewModels;

namespace ImageFunctions.Gui.Models;

public class ImageStorage
{
	public ILayers Layers { get; }
	public ObservableStackList<LayersImageData> Bitmaps { get; }
	//public System.Collections.ObjectModel.ObservableCollection<LayersImageData> Bitmaps { get; }
	//public MyObservableCollection<LayersImageData> Bitmaps { get; }

	public ImageStorage(Func<ICanvas,Bitmap> converter)
	{
		var storage = new StackList<Poco>();
		var layers = new LayersInside();
		Bitmaps = new();

		layers.Maps = Bitmaps;
		layers.Stack = storage;
		layers.Converter = converter;
		Layers = layers;
	}

	internal class LayersInside : ILayers
	{
		public ObservableStackList<LayersImageData> Maps;
		public StackList<Poco> Stack;
		public Func<ICanvas,Bitmap> Converter;

		public ISingleLayerItem this[int index] {
			get { return Getter(index); }
			set { Setter(index,value); }
		}

		Poco Getter(int index)
		{
			var item = Stack[index];
			return item;
		}

		void Setter(int index, ISingleLayerItem item)
		{
			var orig = Stack[index];
			if (item.Id == orig.Id) { return; }

			var origMap = Maps[index];
			var poco = Make(item);

			Stack[index] = poco;
			Maps[index] = Make(poco, this);

			orig.Canvas?.Dispose();
			origMap.Image?.Dispose();
		}

		public int Count { get {
			return Stack.Count;
		}}

		public void DisposeAt(int index)
		{
			var item = Stack.PopAt(index);
			var map = Maps.PopAt(index);

			item.Canvas?.Dispose();
			map.Image?.Dispose();
		}

		public IEnumerator<ISingleLayerItem> GetEnumerator()
		{
			return Stack.GetEnumerator();
		}

		public int IndexOf(string name, int startIndex = 0)
		{
			int count = Stack.Count;
			for(int i = startIndex; i < count; i++) {
				if (Stack[i].Name == name) {
					return i;
				}
			}
			return -1;
		}

		public int IndexOf(uint id, int startIndex = 0)
		{
			int count = Stack.Count;
			for(int i = startIndex; i < count; i++) {
				if (Stack[i].Id == id) {
					return i;
				}
			}
			return -1;
		}

		public void Move(int fromIndex, int toIndex)
		{
			Stack.Move(fromIndex,toIndex);
			Maps.Move(fromIndex,toIndex);

			//Trace.WriteLine($"Stack: {string.Join(",",Stack.Select(s => s.Id))} {string.Join(",",Stack.Select(s => s.Preview.GetHashCode()))}");
			//Trace.WriteLine($"Maps : {string.Join(",", Maps.Select(s => s.Id))} {string.Join(",",Maps.Select(s => s.Image.GetHashCode()))}");
		}

		public ISingleLayerItem Pop() => PopAt(0);
		public ISingleLayerItem PopAt(int index)
		{
			var item = Stack.PopAt(index);
			var map = Maps.PopAt(index);
			map.Image?.Dispose();
			//not disposing of item since we're returning it
			return item;
		}

		public ISingleLayerItem Push(ICanvas layer, string name = null) => PushAt(0,layer,name);
		public ISingleLayerItem PushAt(int index, ICanvas layer, string name = null)
		{
			var poco = Make(layer,name);
			Stack.PushAt(index, poco);
			Maps.PushAt(index,Make(poco,this));
			return poco;
		}

		public void AddRange(IEnumerable<ISingleLayerItem> items)
		{
			var multi = ForEach(items, i => {
				var poco = Make(i);
				var loco = Make(poco,this);
				return (Poco: poco, Loco: loco);
			});

			var stackFlip = multi.Select(m => m.Poco);
			var mapFlip = multi.Select(m => m.Loco);

			Stack.AddRange(stackFlip);
			Maps.AddRange(mapFlip);
		}

		public void RefreshAll()
		{
			//Trace.WriteLine($"{nameof(RefreshAll)}");
			int count = Stack.Count;
			for(int i = 0; i < count; i++) {
				var item = Stack[i];
				if (item.Canvas is CanvasWrapper wrap && wrap.IsDirty) {
					//Trace.WriteLine($"{nameof(RefreshAll)} Dirty:{i}");
					var m = Maps[i];
					var orig = m.Image;
					m.Image = item.Preview = Converter(item.Canvas);
					wrap.DeclareClean();
					orig?.Dispose();
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Stack.GetEnumerator();
		}

		void IStackList<ISingleLayerItem>.PushAt(int index, ISingleLayerItem item)
		{
			var poco = Make(item);
			Stack.PushAt(index,poco);
			Maps.PushAt(index,Make(poco,this));
		}

		void IStackList<ISingleLayerItem>.Push(ISingleLayerItem item)
		{
			var poco = Make(item);
			Stack.Push(poco);
			Maps.Push(Make(poco,this));
		}

		Poco Make(ICanvas canvas, string name = null, uint? id = null, Bitmap preview = null)
		{
			var pre = preview ?? Converter(canvas);
			return new Poco(canvas,name,id,pre);
		}

		Poco Make(ISingleLayerItem item)
		{
			var pre = Converter(item.Canvas);
			var poco = new Poco(item.Canvas,item.Name,item.Id,pre);
			return poco;
		}

		static LayersImageData Make(Poco poco, ILayers layers)
		{
			//TODO does the assignment order matter here ?
			return new LayersImageData {
				Layers = layers,
				Image = poco.Preview,
				Name = poco.Name,
				Id = poco.Id
			};
		}

		static IEnumerable<Y> ForEach<X,Y>(IEnumerable<X> seq, Func<X,Y> action)
		{
			foreach (X item in seq) {
				yield return action(item);
			}
		}
	}

	internal class Poco : ISingleLayerItem
	{
		public Poco(ICanvas canvas, string name = null, uint? id = null, Bitmap preview = null)
		{
			if (id.HasValue) {
				Id = id.Value;
			}
			else {
				Id = Interlocked.Increment(ref Counter);
			}
			Name = name ?? $"Layer-{Id}";
			Canvas = canvas;
			Preview = preview;
		}

		public ICanvas Canvas { get; internal set; }
		public string Name    { get; internal set; }
		public uint Id        { get; internal set; }
		public Bitmap Preview { get; internal set; }

		static uint Counter = 0;
	}
}