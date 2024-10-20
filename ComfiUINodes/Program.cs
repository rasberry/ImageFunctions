using ImageFunctions.Core;

namespace ImageFunctions.ComfiUINodes;

internal class Program
{
	const int DefaultPort = 41414;

	static void Main(string[] args)
	{
		PluginSetup();
		RunServer();
	}

	static void RunServer()
	{
		Console.CancelKeyPress += ShutDown;
		Server = new HttpServer(DefaultPort);
		Server.AddRoute("/register", Handlers.ShowRegister);
		Server.AddRoute("/function", Handlers.RunFunction);
		Server.AddRoute("/functioninfo", Handlers.FunctionInfo);

		Core.Logging.Log.Message($"Starting server on http://localhost:{DefaultPort}");
		try {
			Server.Start();
			Core.Logging.Log.Message("Press any key to stop ...");
			Console.ReadKey(false);
		}
		finally {
			ShutDown();
		}
	}

	static void ShutDown(object sender = null, ConsoleCancelEventArgs args = null)
	{
		Core.Logging.Log.Message("Shutting down ...");
		Server?.Dispose();
		Register?.Dispose();
	}

	static void PluginSetup()
	{
		Register = new Register();
		PluginLoader.LoadAllPlugins(Register);
	}

	internal static Register Register;
	static HttpServer Server;
}
