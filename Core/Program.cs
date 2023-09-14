namespace ImageFunctions.Core;

class Program
{
	static void Main(string[] args)
	{
		var register = new Register();
		RegisterInst = register;
		register.RunAllRegisterMethods();
		PluginLoader.LoadAllPlugins(RegisterInst);

		if (!Options.ParseArgs(args, RegisterInst)) {
			Options.ShowUsage(RegisterInst);
			return;
		}
	}

	static IRegister RegisterInst;
}