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
	public AbstractRegistrant(IRegister register) {
		Reg = register;
	}

	public void Add(string name, T item) {
		Reg.Add(Namespace,name,item);
	}

	public IEnumerable<string> All() {
		foreach(var i in Reg.All()) {
			if (i.StartsWith(Namespace)) {
				yield return StripPrefix(i,Namespace);
			}
		}
	}

	public T Get(string name) {
		return Reg.Get<T>(Namespace,name);
	}

	public bool Try(string name, out T item) {
		return Reg.Try(Namespace,name,out item);
	}

	static string StripPrefix(string text, string prefix) {
		//the + 1 is to remove the extra '.'
		return text.StartsWith(prefix) ? text.Substring(prefix.Length + 1) : text;
	}

	/// <summary>
	/// Override to specify the namespace for this class of items
	/// </summary>
	public abstract string Namespace { get; }
	IRegister Reg;
}
