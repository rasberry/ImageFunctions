using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using ImageFunctions.Core;
using NCCAction = System.Collections.Specialized.NotifyCollectionChangedAction;
using NCCArgs = System.Collections.Specialized.NotifyCollectionChangedEventArgs;

namespace ImageFunctions.Gui.Models;

public class ObservableStackList<T> : StackList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
	public override T this[int index] {
		get {
			Trace.WriteLine($"{nameof(ObservableStackList<T>)} get[{index}]");
			return base[index];
		}
		set {
			Trace.WriteLine($"{nameof(ObservableStackList<T>)} set[{index}]");
			T orig = base[index];
			base[index] = value;
			OnIndexerPropertyChanged();
			OnCollectionReplace(orig, value, index);
		}
	}

	protected override void Clear()
	{
		Trace.WriteLine($"{nameof(ObservableStackList<T>)} {nameof(Clear)}");
		var copy = this.ToList();
		base.Clear();
		OnCountPropertyChanged();
		OnIndexerPropertyChanged();
		OnCollectionRange(NCCAction.Remove, copy, 0);
	}

	public override void PushAt(int index, T item)
	{
		Trace.WriteLine($"{nameof(ObservableStackList<T>)} {nameof(PushAt)} I:{index}");
		base.PushAt(index, item);
		OnCountPropertyChanged();
		OnIndexerPropertyChanged();
		OnCollectionSingle(NCCAction.Add, item, index);
	}

	public override void Move(int fromIndex, int toIndex)
	{
		Trace.WriteLine($"{nameof(ObservableStackList<T>)} {nameof(Move)} F:{fromIndex} T:{toIndex}");
		T item = base[fromIndex];
		base.Move(fromIndex, toIndex);
		OnIndexerPropertyChanged();
		OnCollectionMove(item, fromIndex, toIndex);
	}

	public override void AddRange(IEnumerable<T> items)
	{
		Trace.WriteLine($"{nameof(ObservableStackList<T>)} {nameof(AddRange)}");
		var copy = items.ToList();
		if (copy.Count < 1) { return; }

		int startIx = base.Count;
		base.AddRange(copy);
		OnCountPropertyChanged();
		OnIndexerPropertyChanged();
		OnCollectionRange(NCCAction.Add, copy, startIx);
	}

	public override T PopAt(int index)
	{
		Trace.WriteLine($"{nameof(ObservableStackList<T>)} {nameof(PopAt)} {index}");
		T item = base.PopAt(index);
		OnCountPropertyChanged();
		OnIndexerPropertyChanged();
		OnCollectionSingle(NCCAction.Remove, item, index);
		return item;
	}

	protected override bool Remove(T item)
	{
		Trace.WriteLine($"{nameof(ObservableStackList<T>)} {nameof(Remove)}");
		int ix = IndexOf(item);
		if (ix < 0) { return false; }
		PopAt(ix);
		return true;
	}

	public event NotifyCollectionChangedEventHandler CollectionChanged;
	public event PropertyChangedEventHandler PropertyChanged;

	/// <summary>Disable using ranged notification for operations that affect multiple items</summary>
	public bool DisableRangedNotifications { get; set; } = false;

	static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
	static readonly NCCArgs ResetCollectionChanged = new(NCCAction.Reset);
	static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");

	void OnIndexerPropertyChanged() {
		PropertyChanged?.Invoke(this, IndexerPropertyChanged);
	}

	void OnCountPropertyChanged() {
		PropertyChanged?.Invoke(this, CountPropertyChanged);
	}

	void OnCollectionReplace(T orig, T value, int index) {
		var n = new NCCArgs(NCCAction.Replace, orig, value, index);
		CollectionChanged?.Invoke(this, n);
	}

	void OnCollectionMove(T item, int fromIndex, int toIndex)
	{
		var n = new NCCArgs(NCCAction.Move,item,fromIndex,toIndex);
		CollectionChanged?.Invoke(this,n);
	}

	void OnCollectionSingle(NCCAction action, T item, int index)
	{
		var n = new NCCArgs(action, item, index);
		CollectionChanged?.Invoke(this, n);
	}

	void OnCollectionRange(NCCAction action, List<T> copy, int startIx)
	{
		if (DisableRangedNotifications) {
			CollectionChanged?.Invoke(this, ResetCollectionChanged);
		}
		else {
			var n = new NCCArgs(action, copy, startIx);
			CollectionChanged?.Invoke(this,n);
		}
	}
}