using System.Net;

namespace ImageFunctions.ComfiUINodes;

public sealed class HttpServer : IDisposable
{
	public HttpServer(int port)
	{
		Listener = new HttpListener();
		Listener.Prefixes.Add($"http://localhost:{port}/");
	}

	readonly HttpListener Listener;
	bool KeepRunning = true;
	Dictionary<string, Action<HttpListenerContext>> Routes = new(StringComparer.OrdinalIgnoreCase);

	public void AddRoute(string path, Action<HttpListenerContext> handler)
	{
		Routes.Add(path,handler);
	}

	public void Start()
	{
		Listener.Start();
		while(KeepRunning) {
			HttpListenerContext ctx = Listener.GetContext(); //blocks
			HttpListenerRequest req = ctx.Request;

			string path = ctx.Request.Url?.LocalPath;
			if (string.IsNullOrWhiteSpace(path)) { continue; }

			if (Routes.TryGetValue(path, out var handler)) {
				handler(ctx);
			}
			else {
				HandleNotFound(ctx);
			}
		}
	}

	void HandleNotFound(HttpListenerContext ctx)
	{
		using HttpListenerResponse resp = ctx.Response;
		resp.Headers.Set("Content-Type", "text/plain");
		resp.StatusCode = (int)HttpStatusCode.NotFound;

		string err = $"404 - Not Found '{ctx.Request.Url?.LocalPath}'";
		resp.WriteText(err);
	}

	public void Dispose()
	{
		KeepRunning = false;
		Listener.Stop();
		((IDisposable)Listener).Dispose();
	}
}