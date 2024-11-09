using ImageFunctions.Core.Aides;
using System.Net;

namespace ImageFunctions.ComfiUINodes;

internal static partial class Handlers
{
	[HttpRoute("/register")]
	public static void ShowRegister(HttpListenerContext ctx)
	{
		if(!ctx.EnsureMethodIs(HttpMethod.Get)) { return; }

		var reg = Program.Register;
		var keyList = reg.All().OrderBy(n => $"{n.NameSpace}.{n.Name}");

		using var resp = ctx.Response;
		resp.StatusCode = (int)HttpStatusCode.OK;

		Dictionary<string, List<object>> output = new();
		string currentSpace = "";
		foreach(var k in keyList) {
			if(k.NameSpace != currentSpace) {
				currentSpace = k.NameSpace;
			}
			var desc = reg.GetNameSpaceItemHelp(k);
			if(!output.TryGetValue(currentSpace, out var list)) {
				list = new List<object>();
				output.Add(currentSpace, list);
			}
			list.Add(new {
				Name = k.Name,
				Info = desc
			});
		}

		resp.WriteJson(output);
	}
}
