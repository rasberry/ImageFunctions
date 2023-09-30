using System.Reflection;
using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;


class Register : IRegister
{
	// DRY methods
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

		return (IRegisteredItem<T>)Store[full];
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
		if (wasFound && o is IRegisteredItem<T> inst) {
			item = inst;
			return true;
		}

		item = default;
		return false;
	}

	public IEnumerable<INameSpaceName> All() {
		return Store.Keys;
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

	Dictionary<INameSpaceName,object> Store = new();
}

internal readonly struct NameSpaceName : INameSpaceName, IEquatable<NameSpaceName>
{
	public string Name { get; init; }
	public string NameSpace { get; init; }

	public bool Equals(NameSpaceName other)
	{
		var comp = StringComparison.CurrentCultureIgnoreCase;
		return NameSpace.Equals(other.NameSpace, comp)
			&& Name.Equals(other.Name, comp)
		;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(NameSpace, Name);
	}
}

class NameWithItem<T> : IRegisteredItem<T>
{
	public string Name { get { return Id.Name; }}
	public string NameSpace { get { return Id.NameSpace; }}
	public T Item { get; init; }
	internal NameSpaceName Id { get; init; }
}
