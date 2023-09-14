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

	public IEnumerable<string> All<T>(string @namespace) {
		return Store.GetAllOfType<T>()
			.Select((t) => StripPrefix(t,@namespace));
	}

	static string StripPrefix(string text, string prefix)
	{
		return text.StartsWith(prefix) ? text.Substring(prefix.Length) : text;
	}

	RegisterStore Store = new RegisterStore();

	public void RunAllRegisterMethods()
	{
		var assembly = GetType().Assembly;
		var flags = BindingFlags.Static | BindingFlags.NonPublic;
		var methods = assembly.GetTypes()
			.SelectMany(t => t.GetMethods(flags))
			.Where(m => m.GetCustomAttributes(typeof(InternalRegisterAttribute), false).Length > 0)
		;
		foreach(var m in methods) {
			//Log.Debug($"register method: {m.Name} {m.DeclaringType.Name}");
			m.Invoke(null, new object[] { this });
		}
	}
}