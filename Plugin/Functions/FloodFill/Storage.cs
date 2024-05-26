namespace ImageFunctions.Plugin.Functions.FloodFill;

public interface IStowTakeStore<T>
{
	void Stow(T item);
	T Take();
	int Count { get; }
}

public class StackWrapper<T> : IStowTakeStore<T>
{
	public void Stow(T item)
	{
		Data.Push(item);
	}

	public T Take()
	{
		return Data.Pop();
	}

	public int Count { get { return Data.Count; } }

	Stack<T> Data = new();
}

public class QueueWrapper<T> : IStowTakeStore<T>
{
	public void Stow(T item)
	{
		Data.Enqueue(item);
	}

	public T Take()
	{
		return Data.Dequeue();
	}

	public int Count { get { return Data.Count; } }

	Queue<T> Data = new();
}
