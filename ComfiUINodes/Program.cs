using System.Net;

namespace ImageFunctions.ComfiUINodes;

internal class Program
{
	const int DefaultPort = 41414;

	static void Main(string[] args)
	{
		Console.CancelKeyPress += ShutDown;
		Server = new HttpServer(DefaultPort);
		Server.AddRoute("/test", TestHandler);

		var task = new Task(Server.Start, TaskCreationOptions.LongRunning);
		Core.Logging.Log.Message($"Starting server on http://localhost:{DefaultPort}");
		try {
			task.Start();

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
	}

	static HttpServer Server;

	static void TestHandler(HttpListenerContext ctx)
	{
		ctx.Response.StatusCode = (int)HttpStatusCode.OK;
		Core.Logging.Log.Message($"TestHandler {ctx.Request.Url?.LocalPath}");
		Thread.Sleep(10000);
		using var resp = ctx.Response;
		resp.WriteText("this is a test");
	}
}
