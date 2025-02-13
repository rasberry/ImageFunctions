namespace ImageFunctions.Core;

/// <summary>
/// Extend this abstract to create wrappers for your registration namespaces
/// </summary>
/// <typeparam name="T">Type of the item</typeparam>
public abstract class AbstractRegistrant<T> : IRegistrant<T>
{
	/// <summary>
	/// Creates an instance of the object.
	/// </summary>
	/// <param name="register">IRegister that is used to register the items</param>
	protected AbstractRegistrant(IRegister register)
	{
		Reg = register;
	}

	public void Add(string name, T item)
	{
		Reg.Add(Namespace, name, item);
	}

	public IEnumerable<string> All()
	{
		return Reg.All(Namespace).Select(n => n.Name);
	}

	public IRegisteredItem<T> Get(string name)
	{
		return Reg.Get<T>(Namespace, name);
	}

	public bool Try(string name, out IRegisteredItem<T> item)
	{
		return Reg.Try(Namespace, name, out item);
	}

	public string Default(string @name = null)
	{
		return Reg.Default(Namespace, @name);
	}

	/// <summary>
	/// Override to specify the namespace for this class of items
	/// </summary>
	public abstract string Namespace { get; }
	readonly IRegister Reg;
}
