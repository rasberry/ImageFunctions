using System.Net;
using System.Text;

namespace ImageFunctions.ComfiUINodes;

public static class HttpServerEx
{
	public static void WriteText(this HttpListenerResponse resp, string text)
	{
		using Stream @out = resp.OutputStream;
		byte[] ebuf = Encoding.UTF8.GetBytes(text);
		resp.ContentLength64 = ebuf.Length;
		@out.Write(ebuf, 0, ebuf.Length);
	}

	public static void End(this HttpListenerResponse resp, HttpStatusCode code)
	{
		resp.StatusCode = (int)code;
		resp.SendChunked = false;
		resp.Close();
	}

	public static bool EnsureMethodIs(this HttpListenerContext ctx, HttpMethod expectedMethod)
	{
		var req = ctx.Request;
		var resp = ctx.Response;
		var httpMethod = HttpMethod.Parse(req.HttpMethod);
		if(httpMethod != expectedMethod) {
			resp.End(HttpStatusCode.MethodNotAllowed);
			return false;
		}
		return true;
	}
}
