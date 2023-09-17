namespace ImageFunctions.Core;

public interface IRegister
{
	void Add<T>(string @namespace, string name, T item);
	T Get<T>(string @namespace, string name);
	bool Try<T>(string @namespace, string name, out T item);
	// IEnumerable<string> All<T>(string @namespace, bool stripPrefix = false);
	IEnumerable<string> All();
}