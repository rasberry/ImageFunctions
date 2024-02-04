using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia.Platform.Storage;
using ImageFunctions.Core;

namespace ImageFunctions.Gui.Models;

public class ReactiveLayers : ILayers, INotifyCollectionChanged, INotifyPropertyChanged
{
	public ReactiveLayers()
	{
		Storage = new();
		Tracker = new();
		Tracker.CollectionChanged += OnCollectionChanged;
		//Tracker.PropertyChanged += PropertyChanged;
	}

	void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		Trace.WriteLine($"{nameof(OnCollectionChanged)}");
		CollectionChanged?.Invoke(sender, args);
	}

	public ICanvas this[int index] {
		get {
			Trace.WriteLine($"this get I:{index}");
			var s = Storage[index];
			var t = Tracker[StackIxToListIx(index)];
			EnsureSame(s,t);
			return s;
		}
		set {
			Trace.WriteLine($"this set I:{index}");
			Storage[index] = value;
			Tracker[StackIxToListIx(index)] = value;
		}
	}

	public int Count { get {
		var s = Storage.Count;
		var t = Tracker.Count;
		Trace.WriteLine($"Layers Count C:{s}");
		EnsureSame(s,t);
		return s;
	}}

	public bool IsReadOnly { get { return false; }}

	public void DisposeAt(int index)
	{
		Trace.WriteLine($"Layers DisposeAt I:{index}");
		Storage.DisposeAt(index);
		Tracker.RemoveAt(StackIxToListIx(index));
	}

	public IEnumerator<ICanvas> GetEnumerator()
	{
		var s = Storage.GetEnumerator();
		var t = Enumerable.Reverse(Tracker).GetEnumerator();
		while(true) {
			Trace.WriteLine($"Layers Enumerate");

			bool sm = s.MoveNext();
			bool tm = t.MoveNext();
			if (sm != tm) {
				throw new InvalidOperationException("Collections do not have the same number of items");
			}

			var si = s.Current;
			var ti = t.Current;
			EnsureSame(si,ti);
			yield return si;
		}
	}

	public int IndexOf(string name, int startIndex = 0)
	{
		// This is the only method that doesn't have a direct equivalent in Tracker
		//  I don't think it would fire any notifications since it's not modifying anything
		return Storage.IndexOf(name,startIndex);
	}

	public void Move(int fromIndex, int toIndex)
	{
		Trace.WriteLine($"Layers Move {fromIndex} {toIndex}");
		Storage.Move(fromIndex,toIndex);
		Tracker.Move(StackIxToListIx(fromIndex),StackIxToListIx(toIndex));
		EnsureSame(Storage[toIndex],Tracker[StackIxToListIx(toIndex)]);
	}

	public ICanvas PopAt(int index, out string name)
	{
		Trace.WriteLine($"Layers PopAt {index}");
		var s = Storage.PopAt(index,out name);
		var t = Tracker[StackIxToListIx(index)];
		Tracker.RemoveAt(StackIxToListIx(index));
		EnsureSame(s,t);
		return s;
	}

	public void Push(ICanvas layer, string name = null)
	{
		Trace.WriteLine($"Layers Push");
		Storage.Push(layer, name);
		Tracker.Add(layer);
	}

	public void PushAt(int index, ICanvas layer, string name = null)
	{
		Trace.WriteLine($"Layers PushAt {index}");
		Storage.PushAt(index, layer, name);
		Tracker.Insert(StackIxToListIx(index), layer);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	void EnsureSame<T>(T one, T two)
	{
		if (!EqualityComparer<T>.Default.Equals(one, two)) {
			Trace.WriteLine($"{one} {two}");
			foreach(var i in Storage) {
				Trace.WriteLine($"S: {i.Height} {i.Width}");
			}
			foreach(var i in Tracker) {
				Trace.WriteLine($"T: {i.Height} {i.Width}");
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

	public event NotifyCollectionChangedEventHandler CollectionChanged;
	public event PropertyChangedEventHandler PropertyChanged;
}