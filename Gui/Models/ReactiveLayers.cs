using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using ImageFunctions.Core;

namespace ImageFunctions.Gui.Models;

public class ReactiveLayers : ILayers, INotifyCollectionChanged
{
	public ReactiveLayers(Helpers.ICollectionSymbiote<SingleLayerItem> symbiote)
	{
		Storage = new();
		Tracker = new();
		Symbiote = symbiote;
		Tracker.CollectionChanged += OnCollectionChanged;
	}

	void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		Trace.WriteLine($"{nameof(ReactiveLayers)} {nameof(OnCollectionChanged)} {args.Action}");
		CollectionChanged?.Invoke(sender, args);
	}

	public SingleLayerItem this[int index] {
		get {
			Trace.WriteLine($"{nameof(ReactiveLayers)} this get I:{index}");
			var s = Storage[index];
			var t = Tracker[StackIxToListIx(index)];
			EnsureSame(s.Canvas,t);
			return s;
		}
		set {
			Trace.WriteLine($"{nameof(ReactiveLayers)} this set I:{index}");
			int ixIndex = StackIxToListIx(index);
			Storage[index] = value;
			Tracker[ixIndex] = value.Canvas;
			Symbiote.Set(ixIndex, value);
		}
	}

	public int Count { get {
		var s = Storage.Count;
		var t = Tracker.Count;
		Trace.WriteLine($"{nameof(ReactiveLayers)} Layers Count C:{s}");
		EnsureSame(s,t);
		return s;
	}}

	public bool IsReadOnly { get { return false; }}

	public void DisposeAt(int index)
	{
		Trace.WriteLine($"{nameof(ReactiveLayers)} Layers DisposeAt I:{index}");
		Storage.DisposeAt(index);
		int ixIndex = StackIxToListIx(index);
		Tracker.RemoveAt(ixIndex);
		Symbiote.RemoveAt(ixIndex);
	}

	public IEnumerator<SingleLayerItem> GetEnumerator()
	{
		var s = Storage.GetEnumerator();
		var t = Enumerable.Reverse(Tracker).GetEnumerator();
		while(true) {
			Trace.WriteLine($"{nameof(ReactiveLayers)} Layers Enumerate");

			bool sm = s.MoveNext();
			bool tm = t.MoveNext();
			if (!sm && !tm) {
				yield break;
			}
			if (sm != tm) {
				throw new InvalidOperationException("Collections do not have the same number of items");
			}

			var si = s.Current;
			var ti = t.Current;
			EnsureSame(si.Canvas,ti);
			yield return si;
		}
	}

	public int IndexOf(string name, int startIndex = 0)
	{
		// This method that doesn't have a direct equivalent in Tracker
		//  I don't think it would fire any notifications since it's not modifying anything
		return Storage.IndexOf(name,startIndex);
	}

	public int IndexOf(uint id, int startIndex = 0)
	{
		// This method that doesn't have a direct equivalent in Tracker
		//  I don't think it would fire any notifications since it's not modifying anything
		return Storage.IndexOf(id,startIndex);
	}

	public void Move(int fromIndex, int toIndex)
	{
		Trace.WriteLine($"{nameof(ReactiveLayers)} Layers Move F:{fromIndex} T:{toIndex}");
		Storage.Move(fromIndex,toIndex);
		int ixFrom = StackIxToListIx(fromIndex);
		int ixTo = StackIxToListIx(toIndex);
		Tracker.Move(ixFrom,ixTo);
		Symbiote.Move(ixFrom,ixTo);
		EnsureSame(Storage[toIndex].Canvas,Tracker[ixTo]);
	}

	public SingleLayerItem PopAt(int index)
	{
		Trace.WriteLine($"{nameof(ReactiveLayers)} Layers PopAt {index}");
		var s = Storage.PopAt(index);
		int ixIndex = StackIxToListIx(index);
		var t = Tracker[ixIndex];
		Tracker.RemoveAt(ixIndex);
		Symbiote.RemoveAt(ixIndex);
		EnsureSame(s.Canvas,t);
		return s;
	}

	public SingleLayerItem Push(ICanvas layer, string name = null)
	{
		Trace.WriteLine($"{nameof(ReactiveLayers)} Layers Push");
		var item = Storage.Push(layer, name);
		Tracker.Add(layer);
		Symbiote.Add(item);
		return item;
	}

	public SingleLayerItem PushAt(int index, ICanvas layer, string name = null)
	{
		Trace.WriteLine($"{nameof(ReactiveLayers)} Layers PushAt {index}");
		var item = Storage.PushAt(index, layer, name);
		int ixIndex = StackIxToListIx(index);
		Tracker.Insert(ixIndex, layer);
		Symbiote.Insert(ixIndex, item);
		return item;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	void EnsureSame<T>(T one, T two)
	{
		if (!EqualityComparer<T>.Default.Equals(one, two)) {
			Trace.WriteLine($"{nameof(ReactiveLayers)} {one} {two}");
			foreach(var i in Storage) {
				Trace.WriteLine($"{nameof(ReactiveLayers)} S: {i.Canvas.Height} {i.Canvas.Width}");
			}
			foreach(var i in Tracker) {
				Trace.WriteLine($"{nameof(ReactiveLayers)} T: {i.Height} {i.Width}");
			}
			throw new InvalidOperationException("Instances are not equal");
		}
	}

	int StackIxToListIx(int index)
	{
		return Tracker.Count - index - 1;
	}

	// decided to to store the ICanvas instances in both collections so I don't have to
	// re-implement either. Using EnsureSame to make sure I get the correct behavior
	// having two pointers per item I think is worth not re-implementing a bunch of code
	readonly Layers Storage;
	readonly ObservableCollection<ICanvas> Tracker;
	readonly Helpers.ICollectionSymbiote<SingleLayerItem> Symbiote;

	public event NotifyCollectionChangedEventHandler CollectionChanged;
}
