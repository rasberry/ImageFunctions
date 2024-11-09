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
	Dictionary<string, Action<HttpListenerContext>> Routes = new(StringComparer.OrdinalIgnoreCase);
	bool IsStarted = false;

	public void AddRoute(string path, Action<HttpListenerContext> handler)
	{
		Routes.Add(path, handler);
	}

	public IEnumerable<string> RoutesList()
	{
		return Routes.Keys;
	}

	public Action<HttpListenerContext> NotFoundRoute { get; set; }

	public void Start()
	{
		if(IsStarted) { return; }
		IsStarted = true;
		Listener.Start();
		var res = Listener.BeginGetContext(RequestCallback, Listener);
	}

	void RequestCallback(IAsyncResult res)
	{
		if(Listener == null) { return; }
		HttpListenerContext ctx = null;

		//capture the incoming request
		try {
			ctx = Listener.EndGetContext(res);
		}
		catch(HttpListenerException) {
			//if we're shutting the server down EndGetContext throws an exception so just exit
			return;
		}

		//immediately setup a new context for the next request
		Listener.BeginGetContext(RequestCallback, Listener);

		//handle the request as usuall
		HttpListenerRequest req = ctx.Request;
		string path = ctx.Request.Url?.LocalPath;
		if(string.IsNullOrWhiteSpace(path)) { return; }

		if(Routes.TryGetValue(path, out var handler)) {
			handler(ctx);
		}
		else {
			var notfound = NotFoundRoute ?? HandleNotFound;
			notfound(ctx);
		}
	}

	static void HandleNotFound(HttpListenerContext ctx)
	{
		using HttpListenerResponse resp = ctx.Response;
		resp.End(HttpStatusCode.NotFound);
	}

	public void Dispose()
	{
		IsStarted = false;
		if(Listener != null) {
			Listener.Stop();
			((IDisposable)Listener).Dispose();
		}
	}
}
