namespace ImageFunctions.Core;

class Program
{
	static void Main(string[] args)
	{
		//process args in two parts - first part parse the options
		if (!Options.ParseArgs(args)) {
			return;
		}

		var register = new Register();
		RegisterInst = register;
		PluginLoader.LoadAllPlugins(RegisterInst);

		//second part do any additional output or checks
		// based on given options
		if (!Options.ProcessOptions(RegisterInst)) {
			return;
		}

		//TODO create a layers object
		//TODO run specified function
	}

	static IRegister RegisterInst;
}