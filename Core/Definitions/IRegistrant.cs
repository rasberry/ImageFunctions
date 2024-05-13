namespace ImageFunctions.Core;

/// <summary>
/// An interface that is used for registration namespace wrappers.
///  Note: You should use AbstractRegistrant instead of this interface
/// </summary>
/// <typeparam name="T">The type of the item</typeparam>
public interface IRegistrant<T>
{
	/// <summary>
	/// Add an item to the resitration list
	/// </summary>
	/// <param name="name">The name of the item</param>
	/// <param name="item">The instance of the item</param>
	void Add(string name, T item);

	/// <summary>
	/// Retrieves an item by name
	/// </summary>
	/// <param name="name">The name of the item</param>
	/// <returns>The instance of the item</returns>
	IRegisteredItem<T> Get(string name);

	/// <summary>
	/// Tries to retrieve an item by name
	/// </summary>
	/// <param name="name">The name of the item</param>
	/// <param name="item">The instance of the item</param>
	/// <returns>Returns true if the item was retrieved</returns>
	bool Try(string name, out IRegisteredItem<T> item);

	/// <summary>
	/// Returns the names of all items registered with object
	/// </summary>
	/// <returns></returns>
	IEnumerable<string> All();
}
