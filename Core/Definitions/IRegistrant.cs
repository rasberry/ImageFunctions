namespace ImageFunctions.Core;

public interface IRegistrant<T>
{
	void Add(string name, T item);
	T Get(string name);
	bool Try(string name, out T item);
	IEnumerable<string> All();
}




