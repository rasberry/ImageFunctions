namespace ImageFunctions.Core;

class Program
{
	static int Main(string[] args)
	{
		try {
			return MainInner(args);
		}
		catch(Exception e) {
			#if DEBUG
			Log.Error(e.ToString());
			#else
			Log.Error(e.Message);
			#endif

			return 1;
		}
	}

	static int MainInner(string[] args)
	{
		//process args in two parts - first part parse the options
		if (!Options.ParseArgs(args)) {
			return 2;
		}

		var register = new Register();
		RegisterInst = register;
		PluginLoader.LoadAllPlugins(RegisterInst);

		//second part do any additional output or checks
		// based on given options
		if (!Options.ProcessOptions(RegisterInst)) {
			return 3;
		}

		//load any input images into layers
		var layers = new Layers();
		if (!LoadImages(layers)) {
			return 4;
		}

		// figure out the function to run
		var fr = new FunctionRegister(register);
		if (!fr.Try(Options.FunctionName, out var lzFunc)) {
			Tell.NotRegistered(fr.Namespace,Options.FunctionName);
			return 5;
		}

		//Not really sure how to best use the bool return. Going with exit code for now
		if (!lzFunc.Value.Run(register,layers,Options.FunctionArgs)) {
			return 6;
		}

		//Tools.Engine.SaveImage(layers, //TODO figure out output name(s)

		return 0;
	}

	static IRegister RegisterInst;

	static bool LoadImages(Layers layers)
	{
		foreach(var i in Options.ImageFileNames) {
			if (!File.Exists(i)) {
				Tell.CannotFindFile(i);
				return false;
			}
			Tools.Engine.LoadImage(layers, i);
		}

		return true;
	}
}