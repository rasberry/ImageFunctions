using System.Collections.Specialized;
using System.ComponentModel;
using ImageFunctions.Core;

namespace ImageFunctions.Gui.Models;

public class ObservableStackList<T> : StackList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
	public override T this[int index] {
		get {
			return base[index];
		}
		set {
			T orig = base[index];
			base[index] = value;
			PropertyChanged.Invoke(this,IndexerPropertyChanged);
			CollectionChanged.Invoke(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, orig, value, index));
		}
	}

	protected override void Clear()
	{
		var copy = this.ToList();
		base.Clear();
		PropertyChanged.Invoke(this,CountPropertyChanged);
		PropertyChanged.Invoke(this,IndexerPropertyChanged);
		if (DisableRangedNotifications) {
			CollectionChanged.Invoke(this,ResetCollectionChanged);
		}
		else {
			CollectionChanged.Invoke(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, copy, 0));
		}
	}

	public override void Push(T item)
	{
		base.Push(item);
		PropertyChanged.Invoke(this,CountPropertyChanged);
		PropertyChanged.Invoke(this,IndexerPropertyChanged);
		CollectionChanged.Invoke(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, base.Count - 1));
	}

	public override void PushAt(int index, T item)
	{
		base.PushAt(index, item);
		PropertyChanged.Invoke(this,CountPropertyChanged);
		PropertyChanged.Invoke(this,IndexerPropertyChanged);
		CollectionChanged.Invoke(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
	}

	public override void Move(int fromIndex, int toIndex)
	{
		T item = base[fromIndex];
		base.Move(fromIndex, toIndex);
		PropertyChanged.Invoke(this,IndexerPropertyChanged);
		CollectionChanged.Invoke(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move,item,fromIndex,toIndex));
	}

	public override void AddRange(IEnumerable<T> items)
	{
		var copy = items.ToList();
		if (copy.Count < 1) { return; }

		int startIx = base.Count - 1;
		base.AddRange(copy);
		PropertyChanged.Invoke(this,CountPropertyChanged);
		PropertyChanged.Invoke(this,IndexerPropertyChanged);
		if (DisableRangedNotifications) {
			CollectionChanged.Invoke(this,ResetCollectionChanged);
		}
		else {
			CollectionChanged.Invoke(this,new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, copy, startIx));
		}
	}

	public override T PopAt(int index)
	{
		T item = base.PopAt(index);
		PropertyChanged.Invoke(this,CountPropertyChanged);
		PropertyChanged.Invoke(this,IndexerPropertyChanged);
		CollectionChanged.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,item,index));
		return item;
	}

	protected override bool Remove(T item)
	{
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
	static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
	static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");
}