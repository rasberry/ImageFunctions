using System.Reflection;
using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;


class Register : IRegister
{
	// DRY methods
	public void Add<T>(string @namespace, string name, T item) {
		Store.Add($"{@namespace}.{name}", item);
	}

	public T Get<T>(string @namespace, string name) {
		return Store.Get<T>($"{@namespace}.{name}");
	}

	public bool Try<T>(string @namespace, string name, out T item) {
		return Store.TryGet($"{@namespace}.{name}", out item);
	}

	public IEnumerable<string> All() {
		return Store.GetAll();
	}

	/*
	public IEnumerable<string> All<T>(string @namespace, bool stripPrefix = false) {
		if (stripPrefix) {
			return Store.GetAllOfType<T>()
				.Select((t) => StripPrefix(t,@namespace));
		}
		else {
			return Store.GetAllOfType<T>();
		}
	}

	static string StripPrefix(string text, string prefix)
	{
		return text.StartsWith(prefix) ? text.Substring(prefix.Length) : text;
	}
	*/

	RegisterStore Store = new RegisterStore();
}