using System;
using System.Reflection;
using System.Runtime.Loader;
using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;

internal static class PluginLoader
{
	public static void LoadAllPlugins(IRegister register)
	{
		var pluginsPath = GetPluginsFolder();
		Log.Debug($"pluginsPath = {pluginsPath}");
		var rawList = Directory.EnumerateFiles(pluginsPath);
		
		foreach(string f in rawList) {
			if (!f.EndsWith(".dll",StringComparison.InvariantCultureIgnoreCase)) { continue; }

			Assembly plugin = null;
			try {
				var context = new PluginLoadContext(f);
				plugin = context.LoadFromAssemblyName(AssemblyName.GetAssemblyName(f));
			}
			catch(Exception e) {
				Tell.PluginFileWarnLoading(f,e);
				continue;
			}

			if (plugin != null) {
				RegisterPlugin(plugin, register);
			}
		}
	}

	static string GetPluginsFolder()
	{
		var root = typeof(Program).Assembly.Location;
		return Path.GetDirectoryName(root);
		//var pluginsPath = Path.Combine(root,"plugins");
		//return pluginsPath;
		//return root;
	}

	static void RegisterPlugin(Assembly plugin, IRegister register)
	{
		var plugTypes = plugin.GetTypes();
		var iPluginType = typeof(IPlugin);


		foreach(Type t in plugTypes) {
			var ints = string.Join<Type>(" ",t.GetInterfaces());

			if (! iPluginType.IsAssignableFrom(t)) { continue; }
			Tell.PluginFound(plugin.Location, t.FullName);

			IPlugin pluginInst = null;
			try {
				pluginInst = Activator.CreateInstance(t) as IPlugin;
			}
			catch (Exception e) {
				Tell.PluginTypeWarnLoading(t,e);
				continue;
			}

			Tell.InitingPlugin(t);
			//plugins register things themselves
			try {
				pluginInst.Init(register);
			}
			catch(Exception e) {
				Tell.PluginInitFailed(t,e);
			}
		}
	}
}

// https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
class PluginLoadContext : AssemblyLoadContext
{
	private AssemblyDependencyResolver _resolver;

	public PluginLoadContext(string pluginPath)
	{
		_resolver = new AssemblyDependencyResolver(pluginPath);
	}

	protected override Assembly Load(AssemblyName assemblyName)
	{
		string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
		if (assemblyPath != null)
		{
			return LoadFromAssemblyPath(assemblyPath);
		}

		return null;
	}

	protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
	{
		string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
		if (libraryPath != null)
		{
			return LoadUnmanagedDllFromPath(libraryPath);
		}

		return IntPtr.Zero;
	}
}