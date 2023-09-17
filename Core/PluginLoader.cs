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
		Log.Debug($"Plugin Path is {pluginsPath}");
		var rawList = Directory.EnumerateFiles(pluginsPath);

		foreach(string f in rawList) {
			//TODO does '.dll' work on linux ?
			if (!f.EndsWithIC(".dll")) { continue; }

			//we don't want to re-load the core dll as a plugin
			var selfAssembly = typeof(IPlugin).Assembly;
			if (Path.GetFullPath(f) == selfAssembly.Location) {
				RegisterPlugin(selfAssembly, register);
				continue;
			}

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
		//var iPluginType = typeof(IPlugin);

		/*
		var l1 = AppDomain.CurrentDomain.GetAssemblies();
		var l2 = plugin.GetReferencedAssemblies();
		foreach(var a in l1) {
			Log.Debug(a.CodeBase);
		}
		foreach(var a in l2) {
			Log.Debug(a.CodeBase);
		}
		return;
		*/

		foreach(Type t in plugTypes) {
			//var ints = string.Join<Type>(" ",t.GetInterfaces());
			//Log.Debug($"{t.Module.FullyQualifiedName} {t.FullName} {String.Join<Type>(",",t.GetInterfaces())}");
			//Log.Debug($"{t.FullName} IsPlugin={IsIPlugin(t)} IAF={iPluginType.IsAssignableFrom(t)}");
			//continue;

			if (! IsIPlugin(t)) { continue; }
			Tell.PluginFound(plugin.Location, t.FullName);

			IPlugin pluginInst = null;
			try {
				pluginInst = (IPlugin)Activator.CreateInstance(t);
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

	static bool IsIPlugin(Type t)
	{
		Type iPluginType = typeof(IPlugin);

		// we don't want to instantiate the IPlugin interface itself
		if (t == iPluginType) {
			return false;
		}

		// Note: this is the only mechanism that works since
		//  the plugin must be loaded with the same instance of Core
		if (iPluginType.IsAssignableFrom(t)) {
			return true;
		}

		return false;
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