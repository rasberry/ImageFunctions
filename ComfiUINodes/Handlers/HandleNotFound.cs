using System.Net;

namespace ImageFunctions.ComfiUINodes;

internal static partial class Handlers
{
	public static void HandleNotFound(HttpListenerContext ctx)
	{
		using HttpListenerResponse resp = ctx.Response;
		resp.ContentType = "text/plain";
		resp.StatusCode = (int)HttpStatusCode.NotFound;

		string err = $"404 - Not Found '{ctx.Request.Url?.LocalPath}'";
		resp.WriteText(err);
	}
}
