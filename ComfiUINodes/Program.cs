using ImageFunctions.Core;
using ImageFunctions.Core.Logging;

namespace ImageFunctions.ComfiUINodes;

internal class Program
{
	const int DefaultPort = 41414;

	static void Main(string[] args)
	{
		Log = new LogToConsole();
		PluginSetup();
		RunServer();
	}

	static void RunServer()
	{
		Console.CancelKeyPress += ShutDown;
		Server = new HttpServer(DefaultPort);
		Server.NotFoundRoute = Handlers.HandleNotFound;
		Server.AddRoute("/register", Handlers.ShowRegister);
		Server.AddRoute("/function", Handlers.RunFunction);
		Server.AddRoute("/functioninfo", Handlers.FunctionInfo);

		Log.Message($"Starting server on http://localhost:{DefaultPort}");
		try {
			Server.Start();
			Log.Message("Press any key to stop ...");
			Console.ReadKey(false);
		}
		finally {
			ShutDown();
		}
	}

	static void ShutDown(object sender = null, ConsoleCancelEventArgs args = null)
	{
		Log.Message("Shutting down ...");
		Server?.Dispose();
		Register?.Dispose();
	}

	static void PluginSetup()
	{
		Register = new CoreRegister(Log);
		PluginLoader.LoadAllPlugins(Register, Log);
	}

	internal static CoreRegister Register;
	internal static ICoreLog Log;
	static HttpServer Server;
}
