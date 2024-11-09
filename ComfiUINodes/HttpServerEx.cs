using System.Net;
using System.Text;
using System.Text.Json;

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

	public static void WriteJson(this HttpListenerResponse resp, object model, JsonSerializerOptions options = null)
	{
		//aparently you can't re-use a JsonSerializerOptions object, so making a copy
		options = options == null
			? new JsonSerializerOptions()
			: new JsonSerializerOptions(options)
		;
		options.IncludeFields = true;
		var json = JsonSerializer.Serialize(model, model.GetType(), options);
		resp.ContentType = "application/json";
		WriteText(resp, json);
	}

	public static void WritePlainText(this HttpListenerResponse resp, string text)
	{
		resp.ContentType = "text/plain";
		WriteText(resp, text);
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
