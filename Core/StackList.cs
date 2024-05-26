using System.Collections;

namespace ImageFunctions.Core;

/// <summary>
/// Represents a stack of items with additional list-like functions
/// Note: All methods assume stack indexing where index zero is the top of the stack
/// </summary>
public interface IStackList<T> : IEnumerable<T>
{
	/// <summary>
	/// Get or Set an individual item
	/// </summary>
	/// <param name="index">The index of the item</param>
	T this[int index] { get; set; }

	/// <summary>
	/// Pushes a new item on top of the stack
	/// </summary>
	/// <param name="item">The item to add</param>
	/// <param name="name">The name of the layer to add</param>
	void Push(T item);

	/// <summary>
	/// Insert an item at a specific index and shifts other items
	/// </summary>
	/// <param name="index">The index to use for the insert</param>
	/// <param name="item">The item to insert</param>
	void PushAt(int index, T item);

	/// <summary>
	/// Removes the top layer and returns it
	/// </summary>
	/// <returns>The removed item</returns>
	T Pop();

	/// <summary>
	/// Removes the layer at the given index and returns it
	/// </summary>
	/// <param name="index">The index of the item to remove</param>
	/// <returns>The removed item</returns>
	T PopAt(int index);

	/// <summary>
	/// The number of items in the stack
	/// </summary>
	int Count { get; }

	/// <summary>
	/// Moves an item from the one index to another
	/// </summary>
	/// <param name="fromIndex">The index of the item to be moved</param>
	/// <param name="toIndex">The destination index</param>
	void Move(int fromIndex, int toIndex);

	/// <summary>
	/// Adds a range of items to the stack
	/// </summary>
	/// <param name="items">items to store</param>
	void AddRange(IEnumerable<T> items);
}

/// <summary>
/// An Implementation of the IStackList<T> interface
/// </summary>
/// <typeparam name="T">Type to store</typeparam>
public class StackList<T> : IStackList<T>, IList<T>, IList, IReadOnlyCollection<T>, IReadOnlyList<T>
{
	public virtual T this[int index] {
		get {
			int ix = StackIxToListIx(index);
			return Storage[ix];
		}
		set {
			int ix = StackIxToListIx(index);
			Storage[ix] = value;
		}
	}

	public virtual void Move(int fromIndex, int toIndex)
	{
		if(fromIndex == toIndex) { return; }

		int fix = StackIxToListIx(fromIndex);
		int tix = StackIxToListIx(toIndex);

		var item = Storage[fix];
		Storage.RemoveAt(fix);
		Storage.Insert(tix, item);
	}

	public virtual T PopAt(int index)
	{
		int ix = StackIxToListIx(index);
		var img = Storage[ix];
		Storage.RemoveAt(ix);
		return img;
	}

	public virtual void PushAt(int index, T item)
	{
		int ix = StackIxToListIx(index - 1);
		Storage.Insert(ix, item);
	}

	public virtual IEnumerator<T> GetEnumerator()
	{
		//enumerations are backwards (stack ordering)
		int count = Storage.Count;
		for(int i = count - 1; i >= 0; i--) {
			yield return Storage[i];
		}
	}

	/// <summary>
	/// Add several items in stack order
	/// </summary>
	/// <param name="items">IEnumerable of items to add</param>
	public virtual void AddRange(IEnumerable<T> items)
	{
		if(items == null) {
			throw Squeal.ArgumentNull(nameof(items));
		}
		foreach(var i in items) { Push(i); }
	}

	public T Pop() => PopAt(0);
	public void Push(T item) => PushAt(0, item);
	public virtual int Count => Storage.Count;

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	int StackIxToListIx(int index) => Storage.Count - index - 1;

	readonly List<T> Storage = new();

	#region IList<T> Implementation ============================================
	// Here's the rest of the IList<T> implementation; hidden so we can expose these
	// via the interface only definitions instead of publicly

	protected virtual int IndexOf(T item)
	{
		int ix = Storage.IndexOf(item);
		return ix != -1 ? StackIxToListIx(ix) : -1;
	}

	protected virtual void Clear() => Storage.Clear();
	protected virtual bool Contains(T item) => Storage.Contains(item);
	protected virtual bool Remove(T item) => Storage.Remove(item);
	protected virtual bool IsReadOnly => false;

#pragma warning disable CA1033 // Interface methods should be callable by child types - only want stack methods public
	void ICollection<T>.CopyTo(T[] array, int startIndex) => CopyTo(array, startIndex);
	int IList<T>.IndexOf(T item) => IndexOf(item);
	void IList<T>.Insert(int index, T item) => PushAt(index, item);
	void IList<T>.RemoveAt(int index) => PopAt(index);
	void ICollection<T>.Add(T item) => Push(item);
	void ICollection<T>.Clear() => Clear();
	bool ICollection<T>.Contains(T item) => Contains(item);
	bool ICollection<T>.Remove(T item) => Remove(item);
	bool ICollection<T>.IsReadOnly => IsReadOnly;
#pragma warning restore CA1033
	#endregion IList<T> Implementation =========================================

	#region IList Implementation ===============================================
	//It seems that Avalonia really wants to use the IList (non generic)
	// interface but doesn't complain when not implemented !? 🤷

	protected virtual void CopyTo(Array array, int startIndex)
	{
		if(array == null) {
			throw Squeal.ArgumentNull(nameof(array));
		}
		int count = Storage.Count;
		var mimic = (object[])array;

		for(int a = 0, i = count - 1 - startIndex; i >= 0; a++, i--) {
			mimic[a] = Storage[i];
		}
	}

#pragma warning disable CA1033 // Interface methods should be callable by child types - only want stack methods public
	int IList.Add(object item)
	{
		EnsureIsValid(item);
		int ix = Count;
		Push((T)item);
		return ix;
	}

	bool IList.IsFixedSize {
		get {
			if(Storage is IList list) {
				return list.IsFixedSize;
			}
			return IsReadOnly;
		}
	}

	object IList.this[int index] {
		get => this[index];
		set => this[index] = EnsureIsValid(value);
	}

	bool ICollection.IsSynchronized {
		get {
			if(Storage is ICollection col) {
				return col.IsSynchronized;
			}
			return false;
		}
	}

	object ICollection.SyncRoot {
		get {
			if(Storage is ICollection col) {
				return col.SyncRoot;
			}
			return this;
		}
	}

	void ICollection.CopyTo(Array array, int index) => CopyTo(array, index);
	void IList.Clear() => Clear();
	bool IList.Contains(object item) => Contains(EnsureIsValid(item));
	int IList.IndexOf(object item) => IndexOf(EnsureIsValid(item));
	void IList.Insert(int index, object item) => PushAt(index, EnsureIsValid(item));
	void IList.Remove(object item) => Remove(EnsureIsValid(item));
	void IList.RemoveAt(int index) => PopAt(index);
	bool IList.IsReadOnly => IsReadOnly;
#pragma warning restore CA1033

	static T EnsureIsValid(object value)
	{
		if(!IsValidType(value)) {
			throw new ArrayTypeMismatchException($"Incompatible type {value?.GetType().FullName}");
		}
		return (T)value;
	}

	static bool IsValidType(object value)
	{
		if(value is not T) {
			if(value == null) {
				return default(T) == null;
			}
			return false;
		}
		return true;
	}

	#endregion IList Implementation ============================================
}
