using ImageFunctions.Core;
using ImageFunctions.Core.Logging;
using System.Reflection;

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
		FindRoutes();
		//Server.AddRoute("/register", Handlers.ShowRegister);
		//Server.AddRoute("/function", Handlers.JobRun);
		//Server.AddRoute("/functioninfo", Handlers.FunctionInfo);

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

	static void FindRoutes()
	{
		var handlers = typeof(Handlers);
		var methods = handlers.GetMethods(BindingFlags.Static | BindingFlags.Public);
		foreach(var m in methods) {
			var routes = m.GetCustomAttributes(typeof(HttpRouteAttribute));
			foreach(var r in routes.Cast<HttpRouteAttribute>()) {
				if (String.IsNullOrWhiteSpace(r.Route)) { continue; }
				var d = m.CreateDelegate<Action<System.Net.HttpListenerContext>>();
				Server.AddRoute(r.Route, d);
			}
		}
	}

	internal static CoreRegister Register;
	internal static ICoreLog Log;
	internal static HttpServer Server;
}
