using System.Reflection;
using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;

internal class CoreRegister : IRegister
{
	public void Add<T>(string @namespace, string name, T item) {
		EnsureNameIsNotNull(@namespace, name);
		if (item == null) {
			throw Squeal.ArgumentNull(nameof(item));
		}
		Tell.Registering(@namespace,name);
		var full = new NameSpaceName {
			NameSpace = @namespace,
			Name = name
		};

		//this is loud to avoid override or re-register bugs
		if (!Store.TryAdd(full, item)) {
			throw Squeal.AlreadyRegistered(@namespace,name);
		}
	}

	public IRegisteredItem<T> Get<T>(string @namespace, string name) {
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

	public bool Try<T>(string @namespace, string name, out IRegisteredItem<T> item) {
		//return TryGet($"{@namespace}.{name}", out item);
		EnsureNameIsNotNull(@namespace, name);
		var full = new NameSpaceName {
			NameSpace = @namespace,
			Name = name
		};

		bool wasFound = Store.TryGetValue(full,out object o);

		//must be found and be of the correct type
		if (wasFound && o is T inst) {
			item = new NameWithItem<T> {
				Id = full,
				Item = inst
			};
			return true;
		}

		item = default;
		return false;
	}

	public IEnumerable<INameSpaceName> All() {
		return Store.Keys.Cast<INameSpaceName>();
	}

	void EnsureNameIsNotNull(string @namespace, string name)
	{
		if (string.IsNullOrWhiteSpace(@namespace)) {
			throw Squeal.ArgumentNullOrEmpty(nameof(@namespace));
		}

		if (string.IsNullOrWhiteSpace(name)) {
			throw Squeal.ArgumentNullOrEmpty(nameof(name));
		}
	}

	//can't use the INameSpaceName as the key because the overridden
	// GetHashCode doesn't get called
	Dictionary<NameSpaceName,object> Store = new();
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
		var ns = NameSpace.ToLower();
		var n = Name.ToLower();
		return HashCode.Combine(ns, n);
	}
}

//the dictionary is saving both pieces
// - NameSpaceName in the key and T in the value
// Use this to combine the two again
class NameWithItem<T> : IRegisteredItem<T>
{
	public string Name { get { return Id.Name; }}
	public string NameSpace { get { return Id.NameSpace; }}
	public T Item { get; init; }
	internal NameSpaceName Id { get; init; }
}
