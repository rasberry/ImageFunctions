namespace ImageFunctions.Core;

/// <summary>
/// Interface to an object that is used to register items
/// </summary>
public interface IRegister
{
	/// <summary>
	/// Add an item to the register
	/// </summary>
	/// <typeparam name="T">Type of the item to add. Note: Consider using Lazy<> if the item instantiation is expensive</typeparam>
	/// <param name="namespace">The namespace for the item</param>
	/// <param name="name">The name of the item</param>
	/// <param name="item">The instace of the item to store</param>
	void Add<T>(string @namespace, string name, T item);

	//TODO docs
	IRegisteredItem<T> Get<T>(string @namespace, string name);

	//TODO docs
	bool Try<T>(string @namespace, string name, out IRegisteredItem<T> item);

	//TODO docs
	IEnumerable<INameSpaceName> All();
}

//TODO docs
public interface INameSpaceName
{
	string Name { get; }
	string NameSpace { get; }
}

//TODO docs
public interface IRegisteredItem<T> : INameSpaceName
{
	T Item { get; }
}
