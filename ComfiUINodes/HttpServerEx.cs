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
}