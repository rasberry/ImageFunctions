using System;
using System.Collections;
using System.Collections.Generic;

namespace ImageFunctions.Maze
{
	//Trying to make removal of elements in the middle faster by using a
	// dictionary (heap) O(1) instead of a regular list O(n)

	public class HeapList<T> : IList<T>
	{
		public HeapList()
		{
			Storage = new Dictionary<int, T>();
		}

		public T this[int index] {
			get {
				if (index < 0 || index >= Storage.Count) {
					throw new IndexOutOfRangeException();
				}
				return Storage[index];
			}
			set {
				if (index < 0 || index >= Storage.Count) {
					throw new IndexOutOfRangeException();
				}
				Storage[index] = value;
			}
		}

		public int Count { get {
			return Storage.Count;
		}}

		public bool IsReadOnly { get { return false; }}

		public void Add(T item)
		{
			Storage[Storage.Count] = item;
		}

		public void Clear()
		{
			Storage.Clear();
		}

		public bool Contains(T item)
		{
			return Storage.ContainsValue(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			for(int i=0; i<Storage.Count; i++) {
				array[i + arrayIndex] = Storage[i];
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			for(int i=0; i<Storage.Count; i++) {
				yield return Storage[i];
			}
		}

		public int IndexOf(T item)
		{
			foreach(var kvp in Storage) {
				if (item.Equals(kvp.Value)) {
					return kvp.Key;
				}
			}
			return -1;
		}

		public void Insert(int index, T item)
		{
			//allowing one past the end of the list
			if (index < 0 || index > Storage.Count) {
				throw new IndexOutOfRangeException();
			}

			for(int i = Storage.Count - 1; i >= index; i--) {
				T val = Storage[i];
				Storage[i+1] = val;
			}
			Storage[index] = item;
		}

		public bool Remove(T item)
		{
			int i = IndexOf(item);
			if (i == -1) { return false; }
			RemoveAt(i);
			return true;
		}

		public void RemoveAt(int index)
		{
			if (index < 0 || index >= Storage.Count) {
				throw new IndexOutOfRangeException();
			}
			Storage.Remove(index);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		Dictionary<int,T> Storage;
	}
}


/*
O(1) append (to end)
	= array
	= stack
	= heap (i think)
	= hash table
O(1) remove from anywhere
	= linked list
	= heap
O(1) lookup
	= array
	= hash table



*/