using System.Reflection;

namespace ImageFunctions.Core;

class Program
{
	static void Main(string[] args)
	{
		RegisterInst = new Register();
		DefaultColors.RegisterColors(RegisterInst);
		LoadAllPlugins();

		if (!Options.ParseArgs(args, RegisterInst)) {
			Options.ShowUsage(RegisterInst);
			return;
		}
	}

	static void LoadAllPlugins()
	{
		var root = GetConfigLocation();
		var pluginsPath = Path.Combine(root,"plugins");

		var rawList = Directory.GetFiles(pluginsPath);
		foreach(string f in rawList) {
			if (!f.EndsWith(".dll",StringComparison.InvariantCultureIgnoreCase)) { continue; }

			Assembly plug = null;
			try {
				plug = Assembly.LoadFile(f);
			}
			catch(Exception e) {
				Tell.PluginFileWarnLoading(f,e);
				continue;
			}

			var plugTypes = plug.GetTypes();
			foreach(Type t in plugTypes) {
				if (!(t is IPlugin)) { continue; }

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
				pluginInst.Init(RegisterInst);
			}
		}
	}

	static string GetConfigLocation()
	{
		return Assembly.GetEntryAssembly().Location;
	}

	static IRegister RegisterInst;
}