using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace ImageFunctions.Core;
#pragma warning disable CA1031 // Do not catch general exception types - We want other plugins to load if possible

/// <summary>
/// Handles loading plugins
/// </summary>
public static class PluginLoader
{
	/// <summary>
	/// Loads any plugins and adds any registrations to the given IRestier instance
	/// </summary>
	/// <param name="register">an instance of IRegister</param>
	public static void LoadAllPlugins(IRegister register, ICoreLog log)
	{
		if (log == null) { throw Squeal.ArgumentNull(nameof(log)); }
		var list = GetFilesWithPlugins(log);

		foreach(string f in list) {
			log.Debug($"Loading pluginfile {f}");
			//we don't want to re-load the core dll as a plugin
			var selfAssembly = typeof(IPlugin).Assembly;
			if(Path.GetFullPath(f) == selfAssembly.Location) {
				RegisterPlugin(log, selfAssembly, register);
				continue;
			}

			Assembly plugin = null;
			try {
				log.Info($"Looking for plugins in assembly '{f}'");
				var context = new PluginLoadContext(f);
				plugin = context.LoadFromAssemblyName(AssemblyName.GetAssemblyName(f));
			}
			catch(Exception e) {
				log.Warning(Note.PluginFileWarnLoading(f, e));
				continue;
			}

			if(plugin != null) {
				bool found = RegisterPlugin(log, plugin, register);
				if(!found) {
					log.Error(Note.PluginNotFound(plugin.Location, f));
				}
			}
		}
	}

	static string GetPluginsFolder()
	{
		//just keeping this simple for now - assume all plugins are in the same folder
		// as the main program
		var root = typeof(PluginLoader).Assembly.Location;
		return Path.GetDirectoryName(root);
	}

	internal static bool RegisterPlugin(ICoreLog log, Assembly plugin, IRegister register)
	{
		if (plugin == null) {
			throw Squeal.ArgumentNull(nameof(plugin));
		}

		var plugTypes = plugin.GetTypes();
		bool pluginFound = false;

		foreach(Type t in plugTypes) {
			if(!IsIPlugin(t)) { continue; }
			log.Info(Note.PluginFound(plugin.Location, t.FullName));
			pluginFound = true;

			IPlugin pluginInst = null;
			try {
				pluginInst = (IPlugin)Activator.CreateInstance(t);
			}
			catch(Exception e) {
				log.Warning(Note.PluginTypeWarnLoading(t, e));
				continue;
			}

			log.Info(Note.InitializingPlugin(t));
			//plugins register things themselves
			try {
				pluginInst.Init(register);
			}
			catch(Exception e) {
				log.Warning(Note.PluginInitFailed(t, e));
			}
		}
		return pluginFound;
	}

	static IEnumerable<string> GetFilesWithPlugins(ICoreLog log)
	{
		var pluginsPath = GetPluginsFolder();
		log.Info($"Plugin Path is {pluginsPath}");
		var plugList = Directory.EnumerateFiles(pluginsPath, "*.dll");
		var coreList = Directory.GetFiles(RuntimeEnvironment.GetRuntimeDirectory(), "*.dll");
		var dllList = coreList.Concat(plugList);

		// Log.Debug(String.Join('\n',dllList));
		var resolver = new PathAssemblyResolver(dllList);
		using var metadataContext = new MetadataLoadContext(resolver);

		foreach(string dllFile in plugList) {
			log.Info($"Looking for plugins in assembly '{dllFile}'");
			Assembly assembly = null;
			try {
				assembly = metadataContext.LoadFromAssemblyPath(dllFile);
			}
			catch(BadImageFormatException e) {
				log.Debug($"Skipping {dllFile} E:{e.Message}");
			}

			if(assembly == null) { continue; }
			foreach(var type in assembly.GetTypes()) {
				var isPlugin = IsIPlugin(type);
				//Log.Debug($"Checking type {type.FullName} [{(isPlugin?"Y":"n")}]");
				if(IsIPlugin(type)) {
					//Log.Debug($"Found plugin {type.FullName}");
					yield return dllFile;
				}
			}
		}
	}

	static readonly Dictionary<string, bool> PluginTypeCache = new(StringComparer.CurrentCultureIgnoreCase);
	static readonly Type iPluginType = typeof(IPlugin);
	static bool IsIPlugin(Type subj)
	{
		//check memoized value first
		if(PluginTypeCache.TryGetValue(subj.FullName, out var memoized)) {
			return memoized;
		}

		bool isPlugin = CheckIsIPlugin(subj);
		PluginTypeCache[subj.FullName] = isPlugin;
		return isPlugin;
	}

	static bool CheckIsIPlugin(Type subj)
	{
		// we don't want to instantiate the IPlugin interface itself
		if(subj == iPluginType) {
			return false;
		}
		var comparer = StringComparison.InvariantCultureIgnoreCase;
		if(string.Equals(subj.FullName, iPluginType.FullName, comparer)) {
			return false;
		}

		//Do recursive name check because we can't rely on Type.Equals since we're
		// loading types in different contexts
		var stack = new List<Type>();
		stack.AddRange(subj.GetInterfaces()); //push

		while(stack.Count > 0) {
			var curr = stack[stack.Count - 1];
			stack.RemoveAt(stack.Count - 1); //pop

			//Log.Debug($"Comp {curr.FullName} {stack.Count}");
			if(string.Equals(curr.FullName, iPluginType.FullName, comparer)) {
				return true;
			}
			var faces = curr.GetInterfaces();
			if(faces.Length > 0) {
				stack.AddRange(faces); //push
			}
		}

		//fallback to the normal check
		if(iPluginType.IsAssignableFrom(subj)) {
			return true;
		}

		return false;
	}
}

// https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
class PluginLoadContext : AssemblyLoadContext
{
	readonly AssemblyDependencyResolver _resolver;
	readonly string OriginalPath;

	public PluginLoadContext(string pluginPath)
	{
		OriginalPath = pluginPath;
		_resolver = new AssemblyDependencyResolver(pluginPath);
	}

	protected override Assembly Load(AssemblyName assemblyName)
	{
		//Log.Message($"PluginLoadContext [{OriginalPath}] Trying to load {assemblyName}");
		//Trace.WriteLine($"PluginLoadContext Trying to load {assemblyName}");
		string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
		if(assemblyPath != null) {
			//Log.Message($"PluginLoadContext [{OriginalPath}] Loading {assemblyPath}");
			//Trace.WriteLine($"PluginLoadContext Loading {assemblyPath}");
			return LoadFromAssemblyPath(assemblyPath);
		}

		return null;
	}

	protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
	{
		string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
		if(libraryPath != null) {
			return LoadUnmanagedDllFromPath(libraryPath);
		}

		return IntPtr.Zero;
	}
}
