using ImageFunctions.Core.Aides;

namespace ImageFunctions.Core;

internal class Register : IRegister, IDisposable
{
	public void Add<T>(string @namespace, string name, T item)
	{
		EnsureNameIsNotNull(@namespace, name);
		if(item == null) {
			throw Squeal.ArgumentNull(nameof(item));
		}
		Log.Info(Note.Registering(@namespace, name));
		var full = new NameSpaceName {
			NameSpace = @namespace,
			Name = name
		};

		//this is loud to avoid override or re-register bugs
		if(!Store.TryAdd(full, item)) {
			throw Squeal.AlreadyRegistered(@namespace, name);
		}
	}

	public IRegisteredItem<T> Get<T>(string @namespace, string name)
	{
		EnsureNameIsNotNull(@namespace, name);
		var full = new NameSpaceName {
			NameSpace = @namespace,
			Name = name
		};

		var item = new NameWithItem<T> {
			Id = full,
			Item = (T)Store[full]
		};
		return item;
	}

	public bool Try<T>(string @namespace, string name, out IRegisteredItem<T> item)
	{
		//return TryGet($"{@namespace}.{name}", out item);
		EnsureNameIsNotNull(@namespace, name);
		var full = new NameSpaceName {
			NameSpace = @namespace,
			Name = name
		};

		bool wasFound = Store.TryGetValue(full, out object o);

		//must be found and be of the correct type
		if(wasFound && o is T inst) {
			item = new NameWithItem<T> {
				Id = full,
				Item = inst
			};
			return true;
		}

		item = default;
		return false;
	}

	public IEnumerable<INameSpaceName> All(string @namespace = null)
	{
		var all = Store.Keys.Cast<INameSpaceName>();
		if (@namespace != null) {
			return all.Where(n => n.NameSpace.EqualsIC(@namespace));
		}
		else {
			return all;
		}
	}

	public IEnumerable<string> Spaces()
	{
		return All().Select(k => k.NameSpace).Distinct().Order();
	}

	public string Default(string @namespace, string name = null)
	{
		if (@name != null) {
			Defaults[@namespace] = name;
		}

		if (Defaults.TryGetValue(@namespace, out var def)) {
			return def;
		}

		return null;
	}

	void EnsureNameIsNotNull(string @namespace, string name)
	{
		if(string.IsNullOrWhiteSpace(@namespace)) {
			throw Squeal.ArgumentNullOrEmpty(nameof(@namespace));
		}

		if(string.IsNullOrWhiteSpace(name)) {
			throw Squeal.ArgumentNullOrEmpty(nameof(name));
		}
	}

	public void Dispose()
	{
		foreach(var kvp in Store) {
			if(kvp.Value is IDisposable disposable) {
				disposable.Dispose();
			}
		}
		Store.Clear();
	}

	//can't use the INameSpaceName as the key because the overridden
	// GetHashCode doesn't get called
	readonly Dictionary<NameSpaceName, object> Store = new();
	readonly Dictionary<string, string> Defaults = new();
}

readonly struct NameSpaceName : INameSpaceName, IEquatable<NameSpaceName>
{
	public string Name { get; init; }
	public string NameSpace { get; init; }

	public bool Equals(NameSpaceName other)
	{
		return NameSpace.EqualsIC(other.NameSpace)
			&& Name.EqualsIC(other.Name)
		;
	}

	public override int GetHashCode()
	{
		var c = System.Globalization.CultureInfo.InvariantCulture;
		var ns = NameSpace.ToLower(c);
		var n = Name.ToLower(c);
		return HashCode.Combine(ns, n);
	}

	public override bool Equals(object obj)
	{
		return obj is NameSpaceName name && Equals(name);
	}
}

//the dictionary is saving both pieces
// - NameSpaceName in the key and T in the value
// Use this to combine the two again
class NameWithItem<T> : IRegisteredItem<T>
{
	public string Name { get { return Id.Name; } }
	public string NameSpace { get { return Id.NameSpace; } }
	public T Item { get; init; }
	internal NameSpaceName Id { get; init; }
}
