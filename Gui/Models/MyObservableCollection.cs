#if false
// System.ObjectModel, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// System.Collections.ObjectModel.ObservableCollection<T>
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace ImageFunctions.Gui.Models;


// System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e
// System.Collections.ObjectModel.Collection<T>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[Serializable]
[DebuggerDisplay("Count = {Count}")]
public class MyCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IList, ICollection, IReadOnlyList<T>, IReadOnlyCollection<T>
{
	private readonly IList<T> items;

	public int Count => items.Count;

	protected IList<T> Items => items;

	public T this[int index]
	{
		get
		{
			return items[index];
		}
		set
		{
			if (items.IsReadOnly)
			{
				throw new NotSupportedException();
				//ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
			}
			if ((uint)index >= (uint)items.Count)
			{
				//ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
				throw new ArgumentOutOfRangeException();
			}
			SetItem(index, value);
		}
	}

	bool ICollection<T>.IsReadOnly => items.IsReadOnly;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot
	{
		get
		{
			if (!(items is ICollection collection))
			{
				return this;
			}
			return collection.SyncRoot;
		}
	}

	object? IList.this[int index]
	{
		get
		{
			return items[index];
		}
		set
		{
			//ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
			T value2 = default(T);
			try
			{
				value2 = (T)value;
			}
			catch (InvalidCastException)
			{
				throw new ArgumentException();
				//ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
			}
			this[index] = value2;
		}
	}

	bool IList.IsReadOnly => items.IsReadOnly;

	bool IList.IsFixedSize
	{
		get
		{
			if (items is IList list)
			{
				return list.IsFixedSize;
			}
			return items.IsReadOnly;
		}
	}

	public MyCollection()
	{
		items = new List<T>();
	}

	public MyCollection(IList<T> list)
	{
		if (list == null)
		{
			throw new ArgumentNullException();
			//ThrowHelper.ThrowArgumentNullException(ExceptionArgument.list);
		}
		items = list;
	}

	public void Add(T item)
	{
		if (items.IsReadOnly)
		{
			throw new NotSupportedException();
			//ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		int count = items.Count;
		InsertItem(count, item);
	}

	public void Clear()
	{
		if (items.IsReadOnly)
		{
			throw new NotSupportedException();
			//ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		ClearItems();
	}

	public void CopyTo(T[] array, int index)
	{
		items.CopyTo(array, index);
	}

	public bool Contains(T item)
	{
		return items.Contains(item);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return items.GetEnumerator();
	}

	public int IndexOf(T item)
	{
		return items.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		if (items.IsReadOnly)
		{
			throw new NotSupportedException();
			//ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		if ((uint)index > (uint)items.Count)
		{
			throw new ArgumentOutOfRangeException();
			//ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException();
		}
		InsertItem(index, item);
	}

	public bool Remove(T item)
	{
		if (items.IsReadOnly)
		{
			throw new NotSupportedException();
			//ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		int num = items.IndexOf(item);
		if (num < 0)
		{
			return false;
		}
		RemoveItem(num);
		return true;
	}

	public void RemoveAt(int index)
	{
		if (items.IsReadOnly)
		{
			throw new NotSupportedException();
			//ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		if ((uint)index >= (uint)items.Count)
		{
			throw new ArgumentOutOfRangeException();
			//ThrowHelper.ThrowArgumentOutOfRange_IndexMustBeLessException();
		}
		RemoveItem(index);
	}

	protected virtual void ClearItems()
	{
		items.Clear();
	}

	protected virtual void InsertItem(int index, T item)
	{
		items.Insert(index, item);
	}

	protected virtual void RemoveItem(int index)
	{
		items.RemoveAt(index);
	}

	protected virtual void SetItem(int index, T item)
	{
		items[index] = item;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)items).GetEnumerator();
	}

	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			throw new ArgumentNullException();
			//ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
		}
		if (array.Rank != 1)
		{
			throw new ArgumentException();
			//ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
		}
		if (array.GetLowerBound(0) != 0)
		{
			throw new ArgumentException();
			//ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
		}
		if (index < 0)
		{
			throw new IndexOutOfRangeException();
			//ThrowHelper.ThrowIndexArgumentOutOfRange_NeedNonNegNumException();
		}
		if (array.Length - index < Count)
		{
			throw new ArgumentException();
			//ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
		}
		if (array is T[] array2)
		{
			items.CopyTo(array2, index);
			return;
		}
		Type elementType = array.GetType().GetElementType();
		Type typeFromHandle = typeof(T);
		if (!elementType.IsAssignableFrom(typeFromHandle) && !typeFromHandle.IsAssignableFrom(elementType))
		{
			throw new ArgumentException();
			//ThrowHelper.ThrowArgumentException_Argument_IncompatibleArrayType();
		}
		object[] array3 = array as object[];
		if (array3 == null)
		{
			throw new ArgumentException();
			//ThrowHelper.ThrowArgumentException_Argument_IncompatibleArrayType();
		}
		int count = items.Count;
		try
		{
			for (int i = 0; i < count; i++)
			{
				array3[index++] = items[i];
			}
		}
		catch (ArrayTypeMismatchException)
		{
			throw new ArgumentException();
			//ThrowHelper.ThrowArgumentException_Argument_IncompatibleArrayType();
		}
	}

	int IList.Add(object value)
	{
		if (items.IsReadOnly)
		{
			throw new NotSupportedException();
			//ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		//ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
		T item = default(T);
		try
		{
			item = (T)value;
		}
		catch (InvalidCastException)
		{
			throw new ArgumentException();
			//ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
		}
		Add(item);
		return Count - 1;
	}

	bool IList.Contains(object value)
	{
		if (IsCompatibleObject(value))
		{
			return Contains((T)value);
		}
		return false;
	}

	int IList.IndexOf(object value)
	{
		if (IsCompatibleObject(value))
		{
			return IndexOf((T)value);
		}
		return -1;
	}

	void IList.Insert(int index, object value)
	{
		if (items.IsReadOnly)
		{
			throw new NotSupportedException();
			//ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		//ThrowHelper.IfNullAndNullsAreIllegalThenThrow<T>(value, ExceptionArgument.value);
		T item = default(T);
		try
		{
			item = (T)value;
		}
		catch (InvalidCastException)
		{
			throw new ArgumentException();
			//ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(T));
		}
		Insert(index, item);
	}

	void IList.Remove(object value)
	{
		if (items.IsReadOnly)
		{
			throw new NotSupportedException();
			//ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ReadOnlyCollection);
		}
		if (IsCompatibleObject(value))
		{
			Remove((T)value);
		}
	}

	private static bool IsCompatibleObject(object value)
	{
		if (!(value is T))
		{
			if (value == null)
			{
				return default(T) == null;
			}
			return false;
		}
		return true;
	}
}



//[Serializable]
[DebuggerDisplay("Count = {Count}")]
public class MyObservableCollection<T> : MyCollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
	/*
	[Serializable]
	private sealed class SimpleMonitor : IDisposable
	{
		internal int _busyCount;

		[NonSerialized]
		internal ObservableCollection<T> _collection;

		public SimpleMonitor(ObservableCollection<T> collection)
		{
			_collection = collection;
		}

		public void Dispose()
		{
			_collection._blockReentrancyCount--;
		}
	}

	private SimpleMonitor _monitor;
	*/

	//[NonSerialized]
	//private int _blockReentrancyCount;

	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			PropertyChanged += value;
		}
		remove
		{
			PropertyChanged -= value;
		}
	}

	public event NotifyCollectionChangedEventHandler CollectionChanged;
	protected event PropertyChangedEventHandler PropertyChanged;

	public MyObservableCollection()
	{
	}

	//public MyObservableCollection(IEnumerable<T> collection)
	//	: base((IList<T>)new List<T>(collection ?? throw new ArgumentNullException("collection")))
	//{
	//}

	//public MyObservableCollection(List<T> list)
	//	: base((IList<T>)new List<T>(list ?? throw new ArgumentNullException("list")))
	//{
	//}

	public void Move(int oldIndex, int newIndex)
	{
		MoveItem(oldIndex, newIndex);
	}

	protected override void ClearItems()
	{
		//CheckReentrancy();
		base.ClearItems();
		OnCountPropertyChanged();
		OnIndexerPropertyChanged();
		OnCollectionReset();
	}

	protected override void RemoveItem(int index)
	{
		//CheckReentrancy();
		T val = base[index];
		base.RemoveItem(index);
		OnCountPropertyChanged();
		OnIndexerPropertyChanged();
		OnCollectionChanged(NotifyCollectionChangedAction.Remove, val, index);
	}

	protected override void InsertItem(int index, T item)
	{
		//CheckReentrancy();
		base.InsertItem(index, item);
		OnCountPropertyChanged();
		OnIndexerPropertyChanged();
		OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
	}

	protected override void SetItem(int index, T item)
	{
		//CheckReentrancy();
		T val = base[index];
		base.SetItem(index, item);
		OnIndexerPropertyChanged();
		OnCollectionChanged(NotifyCollectionChangedAction.Replace, val, item, index);
	}

	protected void MoveItem(int oldIndex, int newIndex)
	{
		//CheckReentrancy();
		T val = base[oldIndex];
		base.RemoveItem(oldIndex);
		base.InsertItem(newIndex, val);
		OnIndexerPropertyChanged();
		OnCollectionChanged(NotifyCollectionChangedAction.Move, val, newIndex, oldIndex);
	}

	protected void OnPropertyChanged(PropertyChangedEventArgs e) {
		this.PropertyChanged?.Invoke(this, e);
	}

	protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
		this.CollectionChanged?.Invoke(this,e);

		//NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
		//if (collectionChanged != null)
		//{
		//	//_blockReentrancyCount++;
		//	try
		//	{
		//		collectionChanged(this, e);
		//	}
		//	finally
		//	{
		//		//_blockReentrancyCount--;
		//	}
		//}
	}

	/*
	protected IDisposable BlockReentrancy()
	{
		_blockReentrancyCount++;
		return EnsureMonitorInitialized();
	}

	protected void CheckReentrancy()
	{
		if (_blockReentrancyCount > 0)
		{
			NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
			if (collectionChanged != null && collectionChanged.GetInvocationList().Length > 1)
			{
				throw new InvalidOperationException(System.SR.ObservableCollectionReentrancyNotAllowed);
			}
		}
	}
	*/

	void OnCountPropertyChanged() {
		OnPropertyChanged(CountPropertyChanged);
	}

	void OnIndexerPropertyChanged() {
		OnPropertyChanged(IndexerPropertyChanged);
	}

	void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index) {
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
	}

	void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex) {
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
	}

	void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index) {
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
	}

	void OnCollectionReset() {
		OnCollectionChanged(ResetCollectionChanged);
	}

	/*
	private SimpleMonitor EnsureMonitorInitialized()
	{
		return _monitor ?? (_monitor = new SimpleMonitor(this));
	}

	[OnSerializing]
	private void OnSerializing(StreamingContext context)
	{
		EnsureMonitorInitialized();
		_monitor._busyCount = _blockReentrancyCount;
	}

	[OnDeserialized]
	private void OnDeserialized(StreamingContext context)
	{
		if (_monitor != null)
		{
			_blockReentrancyCount = _monitor._busyCount;
			_monitor._collection = this;
		}
	}
	*/

	internal static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");
	internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
	internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);

}
#endif