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

	/// <summary>
	/// Finds the requested item. Should throw an exception if not found.
	/// </summary>
	/// <typeparam name="T">The expected item type</typeparam>
	/// <param name="namespace">The namespace of the item</param>
	/// <param name="name">The name of the item</param>
	/// <returns>The item</returns>
	IRegisteredItem<T> Get<T>(string @namespace, string name);

	/// <summary>
	/// Attempts to find the requested item
	/// </summary>
	/// <typeparam name="T">The expected item type</typeparam>
	/// <param name="namespace">The namespace of the item</param>
	/// <param name="name">The name of the item</param>
	/// <param name="item">The resulting item if found</param>
	/// <returns>true if found otherwise false</returns>
	bool Try<T>(string @namespace, string name, out IRegisteredItem<T> item);

	/// <summary>
	/// Returns all registered items.
	/// </summary>
	/// <param name="namespace">optional namespace to filter by</param>
	/// <returns></returns>
	IEnumerable<INameSpaceName> All(string @namespace = null);

	/// <summary>
	/// Returns a list of registered namespaces
	/// </summary>
	IEnumerable<string> Spaces();

	/// <summary>
	/// Get or set a namespace default item
	/// </summary>
	/// <param name="namespace">The namespace of the item/param>
	/// <param name="name">The name of the item to make the default (set)</param>
	/// <returns>The name of the default item (get)</returns>
	public string Default(string @namespace, string name = null);
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
