namespace ImageFunctions.Gui.Helpers;

public interface ICollectionSymbiote<T>
{
	void Set(int index, T item);
	void Add(T item);
	void Insert(int index, T item);
	bool Remove(T item);
	void RemoveAt(int index);
	void Move(int fromIndex, int toIndex);
}