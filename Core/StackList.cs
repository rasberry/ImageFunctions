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
	/// Insert an item at a specific index and shifts other items
	/// </summary>
	/// <param name="index">The index to use for the insert</param>
	/// <param name="item">The item to insert</param>
	void PushAt(int index, T item);

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
	/// Pushes a new item on top of the stack
	/// </summary>
	/// <param name="item">The item to add</param>
	/// <param name="name">The name of the layer to add</param>
	void Push(T item);

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
public class StackList<T> : IStackList<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>
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

	public virtual int Count { get {
		return Storage.Count;
	}}

	public virtual void AddRange(IEnumerable<T> items)
	{
		Storage.AddRange(items);
	}

	public virtual void Move(int fromIndex, int toIndex)
	{
		if (fromIndex == toIndex) { return; }

		int fix = StackIxToListIx(fromIndex);
		int tix = StackIxToListIx(toIndex);

		var item = Storage[fix];
		Storage.RemoveAt(fix);
		Storage.Insert(tix,item);
	}

	public virtual T PopAt(int index)
	{
		int ix = StackIxToListIx(index);
		var img = Storage[index];
		Storage.RemoveAt(ix);
		return img;
	}

	public virtual void Push(T item)
	{
		Storage.Add(item);
	}

	public virtual void PushAt(int index, T item)
	{
		int ix = StackIxToListIx(index - 1);
		Storage.Insert(ix,item);
	}

	public virtual IEnumerator<T> GetEnumerator()
	{
		//enumerations are backwards (stack ordering)
		int count = Storage.Count;
		for(int i = count - 1; i >= 0; i--) {
			yield return Storage[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/* Here's the rest of the IList implementation; hidden so we can expose these
	 * via the interface only definitions instead of publicly
	 */

	protected virtual int IndexOf(T item)
	{
		int ix = Storage.IndexOf(item);
		return ix != -1 ? StackIxToListIx(ix) : -1;
	}

	protected virtual void CopyTo(T[] array, int startIndex)
	{
		int count = Storage.Count;
		for(int a = 0, i = count - 1 - startIndex; i >= 0; a++, i--) {
			array[a] = Storage[i];
		}
	}

	protected virtual void Clear()
	{
		Storage.Clear();
	}

	protected virtual bool Contains(T item)
	{
		return Storage.Contains(item);
	}

	protected virtual bool Remove(T item)
	{
		return Storage.Remove(item);
	}

	protected virtual bool IsReadOnly { get {
		return false;
	}}

	void ICollection<T>.CopyTo(T[] array, int startIndex) => CopyTo(array,startIndex);
	int IList<T>.IndexOf(T item) => IndexOf(item);
	void IList<T>.Insert(int index, T item) => PushAt(index,item);
	void IList<T>.RemoveAt(int index) => PopAt(index);
	void ICollection<T>.Add(T item) => Push(item);
	void ICollection<T>.Clear() => Clear();
	bool ICollection<T>.Contains(T item) => Contains(item);
	bool ICollection<T>.Remove(T item) => Remove(item);
	bool ICollection<T>.IsReadOnly => IsReadOnly;

	int StackIxToListIx(int index)
	{
		return Storage.Count - index - 1;
	}

	readonly List<T> Storage = new();
}