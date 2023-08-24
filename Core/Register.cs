namespace ImageFunctions.Core;

public interface IRegister
{
	/// <summary>
	/// Register a RGBA color by name
	/// </summary>
	/// <param name="name">Name of the color</param>
	/// <param name="color">Color value</param>
	void AddColor(string name, ColorRGBA color);

	/// <summary>
	/// Get a registered color by name
	/// </summary>
	/// <param name="name">Name of the color</param>
	/// <returns>The color value</returns>
	ColorRGBA GetColor(string name);

	/// <summary>
	/// Get a registered color by name
	/// </summary>
	/// <param name="name">Name of the color</param>
	/// <param name="color">The color value</param>
	/// <returns>true if the color is registered otherwise false</returns>
	bool TryGetColor(string name, out ColorRGBA color);

	/// <summary>
	/// Gets all registered color names.
	/// </summary>
	/// <returns>IEnumerable of registered names</returns>
	IEnumerable<string> GetAllColors();

	/// <summary>
	/// Register an engine by name
	/// </summary>
	/// <param name="name">Name of the engine</param>
	/// <param name="engine">A method that constructs the engine</param>
	void AddEngine(string name, Func<IImageEngine> engine);

	/// <summary>
	/// Get a registered engine by name
	/// </summary>
	/// <param name="name">Name of the engine</param>
	/// <returns>A new instance of the engine</returns>
	IImageEngine GetEngine(string name);

	/// <summary>
	/// Get a registered engine by name
	/// </summary>
	/// <param name="name">Name of the engine</param>
	/// <param name="engine">A new instance of the engine</param>
	/// <returns>true if the engine is registered otherwise false</returns>
	bool TryGetEngine(string name, out IImageEngine engine);

	/// <summary>
	/// Gets all registered engine names.
	/// </summary>
	/// <returns>IEnumerable of registered names</returns>
	IEnumerable<string> GetAllEngines();

	/// <summary>
	/// Register a function by name
	/// </summary>
	/// <param name="name">Name of the function</param>
	/// <param name="function">A method that constructs the function</param>
	void AddFunction(string name, Func<IFunction> function);

	/// <summary>
	/// Get a registered function by name
	/// </summary>
	/// <param name="name">Name of the function</param>
	/// <returns>A new instance of the function</returns>
	IFunction GetFunction(string name);

	/// <summary>
	/// Get a registered function by name
	/// </summary>
	/// <param name="name">Name of the function</param>
	/// <param name="function">A new instance of the function</param>
	/// <returns>true if the function is registered otherwise false</returns>
	bool TryGetFunction(string name, out IFunction function);

	/// <summary>
	/// Gets all registered function names.
	/// </summary>
	/// <returns>IEnumerable of registered names</returns>
	IEnumerable<string> GetAllFunctions();

	/// <summary>
	/// Scans an assembly for IImageEngine and IFunction types and registers
	///  them. The name used is the full type name
	/// </summary>
	/// <param name="assembly">The assembly to scan</param>
	void RegisterAll(System.Reflection.Assembly assembly);
}

class Register : IRegister
{
	RegisterStore Store = new RegisterStore();

	const string ColorPrefix = "Color.";
	public void AddColor(string name, ColorRGBA color) {
		Add(ColorPrefix, name, color);
	}
	public ColorRGBA GetColor(string name) {
		return Get<ColorRGBA>(ColorPrefix,name);
	}
	public bool TryGetColor(string name, out ColorRGBA color) {
		return Try(ColorPrefix, name, out color);
	}
	public IEnumerable<string> GetAllColors() {
		return All<ColorRGBA>(ColorPrefix);
	}

	const string EnginePrefix = "Engine.";
	public void AddEngine(string name, Func<IImageEngine> engine) {
		Add(EnginePrefix, name, engine);
	}
	public IImageEngine GetEngine(string name) {
		return GetFun<IImageEngine>(EnginePrefix,name);
	}
	public bool TryGetEngine(string name, out IImageEngine engine) {
		return TryFun(EnginePrefix, name, out engine);
	}
	public IEnumerable<string> GetAllEngines() {
		return All<Func<IImageEngine>>(EnginePrefix);
	}

	const string FunctionPrefix = "Function.";
	public void AddFunction(string name, Func<IFunction> function) {
		Add(FunctionPrefix, name, function);
	}
	public IFunction GetFunction(string name) {
		return GetFun<IFunction>(FunctionPrefix, name);
	}
	public bool TryGetFunction(string name, out IFunction function) {
		return TryFun<IFunction>(FunctionPrefix, name, out function);
	}
	public IEnumerable<string> GetAllFunctions() {
		return All<Func<IFunction>>(FunctionPrefix);
	}

	public void RegisterAll(System.Reflection.Assembly assembly)
	{
		var allTypes = assembly.GetTypes();
		foreach(var t in allTypes) {
			if (t is IImageEngine) {
				AddEngine(t.FullName, () => (IImageEngine)Activator.CreateInstance(t));
			}
			else if (t is IFunction) {
				AddFunction(t.FullName, () => (IFunction)Activator.CreateInstance(t));
			}
		}
	}

	// DRY methods
	void Add<T>(string prefix, string name, T item) {
		Store.Add($"{prefix}.{name}", item);
	}
	T Get<T>(string prefix, string name) {
		return Store.Get<T>($"{prefix}.{name}");
	}
	bool Try<T>(string prefix, string name, out T item) {
		return Store.TryGet($"{prefix}.{name}", out item);
	}
	IEnumerable<string> All<T>(string prefix) {
		return Store.GetAllOfType<T>()
			.Select((t) => StripPrefix(t,prefix));
	}
	T GetFun<T>(string prefix, string name) {
		var activator = Store.Get<Func<T>>($"{prefix}.{name}");
		return activator();
	}
	bool TryFun<T>(string prefix, string name, out T item) {
		bool w = Store.TryGet($"{prefix}.{name}", out Func<T> activator);
		if (w) {
			item = activator();
			return true;
		}
		item = default;
		return false;
	}


	static string StripPrefix(string text, string prefix)
	{
		return text.StartsWith(prefix) ? text.Substring(prefix.Length) : text;
	}
}