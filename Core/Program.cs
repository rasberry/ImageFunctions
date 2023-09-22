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

			return e.GetHashCode();
		}
	}

	static int MainInner(string[] args)
	{
		//process args in two parts - first part parse the options
		if (!Options.ParseArgs(args)) {
			return ExitCode.StoppedAtParseArgs;
		}

		var register = new Register();
		RegisterInst = register;
		PluginLoader.LoadAllPlugins(RegisterInst);

		//second part do any additional output or checks
		// based on given options
		if (!Options.ProcessOptions(RegisterInst)) {
			return ExitCode.StoppedAtProcessOptions;
		}

		//load any input images into layers
		var layers = new Layers();
		if (!LoadImages(layers)) {
			return ExitCode.StoppedAtLoadImages;
		}

		// figure out the function to run
		var fr = new FunctionRegister(register);
		if (!fr.Try(Options.FunctionName, out var lzFunc)) {
			Tell.NotRegistered(fr.Namespace,Options.FunctionName);
			return ExitCode.StoppedFunctionNotRegistered;
		}

		//Not really sure how to best use the bool return. Going with exit code for now
		Log.Info($"Running Function {Options.FunctionName}");
		if (!lzFunc.Value.Run(register,layers,Options.FunctionArgs)) {
			return ExitCode.StoppedAfterRun;
		}

		Log.Info($"Saving image {Options.OutputName}");
		if (layers.Count > 0) {
			Tools.Engine.SaveImage(layers, Options.OutputName);
		}
		else {
			Tell.NoLayersToSave();
		}

		return ExitCode.Success;
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