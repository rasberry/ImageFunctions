using System.Net;

namespace ImageFunctions.ComfiUINodes;

internal static partial class Handlers
{
	[HttpRoute("/help")]
	public static void Help(HttpListenerContext ctx)
	{
		if(!ctx.EnsureMethodIs(HttpMethod.Get)) { return; }

		var list = Program.Server.RoutesList();
		ctx.Response.WriteJson(new { routes = list });
	}
}
