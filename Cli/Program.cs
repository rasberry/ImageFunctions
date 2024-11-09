using ImageFunctions.Core;
using ImageFunctions.Core.FileIO;
using ImageFunctions.Core.Logging;
using System.Diagnostics;

namespace ImageFunctions.Cli;

#pragma warning disable CA1031 // Do not catch general exception types - We want to handle all Exception

//this is internal for testing purposes
internal sealed class Program
{
	static int Main(string[] args)
	{
#if DEBUG
		Trace.Listeners.Add(new ConsoleTraceListener());
#endif
		var log = new LogToConsole();

		try {
			using var register = new CoreRegister(log);
			var options = new Options(register, log);
			using var layers = new Layers();
			var main = new Program(register, options, layers, log);
			return main.Run(args);
		}
		catch(Exception e) {
			//#if DEBUG
			log.Error(e.ToString());
			//#else
			//Log.Error(e.Message);
			//#endif

			return e.GetHashCode();
		}
	}

	internal Program(IRegister register, Options options, ILayers layers, ICoreLog log)
	{
		Register = register;
		Options = options;
		Layers = layers;
		Log = log;
	}

	int Run(string[] args)
	{
		//setup stage
		if(!TrySetup(args, out int exitCode)) {
			return exitCode;
		}

		//load any input images into layers
		if(!LoadImages()) {
			return ExitCode.StoppedAtLoadImages;
		}

		//function stage
		if(!TryRunFunction(out exitCode)) {
			return exitCode;
		}

		//save the layers to one or more images
		Log.Info($"Saving image {Options.OutputName}");
		using var clerk = new FileClerk(FileIO, Options.OutputName);
		if(Layers.Count > 0) {
			Options.Engine.Item.Value.SaveImage(Layers, clerk, Options.ImageFormat);
		}
		else {
			Log.Warning(Note.NoLayersToSave());
		}

		return ExitCode.Success;
	}

	//this is internal for testing purposes
	internal bool TrySetup(string[] args, out int exitCode)
	{
		//process args in two parts - first part parse the options
		if(!Options.ParseArgs(args, null)) {
			exitCode = ExitCode.StoppedAtParseArgs;
			return false;
		}

		//load the plugins - this could be a slow operation if there are many plugins
		PluginLoader.LoadAllPlugins(Register, Log);

		//second part - do any additional output or checks based on given options
		if(!Options.ProcessOptions()) {
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
		if(!fr.Try(Options.FunctionName, out var funcItem)) {
			Log.Error(Note.NotRegistered(fr.Namespace, Options.FunctionName));
			exitCode = ExitCode.StoppedFunctionNotRegistered;
			return false;
		}

		var context = new FunctionContext {
			Register = Register,
			Layers = Layers,
			Options = Options,
			Log = Log
		};

		//Not really sure how to best use the bool return. Going with exit code for now
		Log.Info($"Running Function {funcItem}");
		var func = funcItem.Item.Invoke(context);
		if(!func.Run(Options.FunctionArgs)) {
			exitCode = ExitCode.StoppedAfterRun;
			return false;
		}

		exitCode = ExitCode.Success;
		return true;
	}

	bool LoadImages()
	{
		//we're reversing the images since we're using a stack
		// so the first image specified should stay on top
		// and the last one on the bottom.
		foreach(var file in Options.ImageFileNames.Reverse()) {
			using var clerk = new FileClerk(FileIO, file);
			if(!File.Exists(file)) {
				Log.Error(Note.CannotFindInputImage(file));
				return false;
			}
			Options.Engine.Item.Value.LoadImage(Layers, clerk);
		}

		return true;
	}

	internal ILayers Layers;
	internal IRegister Register;
	internal Options Options; //not using ICoreOptions interface to allow access to extra methods
	internal ICoreLog Log;
	readonly SimpleFileIO FileIO = new();
}
