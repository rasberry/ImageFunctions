namespace ImageFunctions.Core;

class RegisterStore
{
	public bool Add<T>(string name, T instance)
	{
		EnsureNameIsNotNull(name);
		if (instance == null) {
			throw new ArgumentNullException("instance");
		}

		Tell.Registering(name);

		return Store.TryAdd(name,instance);
	}

	public T Get<T>(string name)
	{
		EnsureNameIsNotNull(name);
		return (T)Store[name];
	}

	public bool TryGet<T>(string name, out T instance)
	{
		EnsureNameIsNotNull(name);
		bool wasFound = Store.TryGetValue(name,out object o);

		//must be found and be of the correct type
		if (wasFound && o is T inst) {
			instance = inst;
			return true;
		}

		instance = default;
		return false;
	}

	public bool Has(string name)
	{
		EnsureNameIsNotNull(name);
		return Store.ContainsKey(name);
	}

	/*
	public IEnumerable<string> GetAllOfType<T>()
	{
		foreach(var kvp in Store) {
			if (kvp.Value is T) {
				yield return kvp.Key;
			}
		}
	}
	*/

	public IEnumerable<string> GetAll()
	{
		return Store.Keys;
	}

	void EnsureNameIsNotNull(string name)
	{
		if (string.IsNullOrWhiteSpace(name)) {
			throw new ArgumentException("must not be null or empty","name");
		}
	}

	Dictionary<string,object> Store = new(StringComparer.CurrentCultureIgnoreCase);
}