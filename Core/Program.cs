namespace ImageFunctions.Core;

//this is internal for testing purposes
internal class Program
{
	static int Main(string[] args)
	{
		try {
			var register = new CoreRegister();
			var options = new Options(register);
			using var layers = new CoreLayers();
			var main = new Program(register, options, layers);
			return main.Run(args);
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

	Program(IRegister register, Options options, ILayers layers)
	{
		Register = register;
		O = options;
		Layers = layers;
	}

	int Run(string[] args)
	{
		int exitCode;

		//setup stage
		if (!TrySetup(args, out exitCode)) {
			return exitCode;
		}

		//load any input images into layers
		if (!LoadImages()) {
			return ExitCode.StoppedAtLoadImages;
		}

		//function stage
		if (!TryRunFunction(out exitCode)) {
			return exitCode;
		}

		//save the layers to one or more images
		Log.Info($"Saving image {O.OutputName}");
		if (Layers.Count > 0) {
			O.Engine.Item.Value.SaveImage(Layers, O.OutputName, O.ImageFormat);
		}
		else {
			Tell.NoLayersToSave();
		}

		return ExitCode.Success;
	}

	//this is internal for testing purposes
	internal bool TrySetup(string[] args, out int exitCode)
	{
		//process args in two parts - first part parse the options
		if (!O.ParseArgs(args, null)) {
			exitCode = ExitCode.StoppedAtParseArgs;
			return false;
		}

		//load the plugins - this could be a slow operation if there are many plugins
		PluginLoader.LoadAllPlugins(Register);

		//second part - do any additional output or checks based on given options
		if (!O.ProcessOptions()) {
			exitCode = ExitCode.StoppedAtProcessOptions;
			return false;
		}

		exitCode = ExitCode.Success;
		return true;
	}

	internal bool TryRunFunction(out int exitCode)
	{
		// figure out the function to run
		var fr = new FunctionRegister(Register);
		if (!fr.Try(O.FunctionName, out var lzFunc)) {
			Tell.NotRegistered(fr.Namespace,O.FunctionName);
			exitCode = ExitCode.StoppedFunctionNotRegistered;
			return false;
		}

		//Not really sure how to best use the bool return. Going with exit code for now
		Log.Info($"Running Function {lzFunc}");
		if (!lzFunc.Item.Value.Run(Register, Layers, O, O.FunctionArgs)) {
			exitCode = ExitCode.StoppedAfterRun;
			return false;
		}

		exitCode = ExitCode.Success;
		return true;
	}

	bool LoadImages()
	{
		foreach(var i in O.ImageFileNames) {
			if (!File.Exists(i)) {
				Tell.CannotFindFile(i);
				return false;
			}
			O.Engine.Item.Value.LoadImage(Layers, i);
		}

		return true;
	}

	internal ILayers Layers;
	internal IRegister Register;
	internal Options O; //not using interface to allow access to extra methods
}