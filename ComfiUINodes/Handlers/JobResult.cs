using ImageFunctions.Core.FileIO;
using System.Net;
using System.Net.Http.Headers;

namespace ImageFunctions.ComfiUINodes;

internal static partial class Handlers
{
	[HttpRoute("/result")]
	public static void JobResult(HttpListenerContext ctx)
	{
		if(!ctx.EnsureMethodIs(HttpMethod.Get)) { return; }
		using var resp = ctx.Response;
		var req = ctx.Request;

		var sjob = req.QueryString.Get("job");
		if(!TryFindJob(req, resp, sjob, out var job, out var jobId)) {
			return;
		}

		if(job.Status == JobStatusKind.Failed) {
			ErrorResponse(resp, HttpStatusCode.OK, (LoggerForJob)job.Context.Log);
			JobHoard.Remove(jobId, out _);
			return;
		}
		else if(job.Status != JobStatusKind.Finished) {
			resp.WritePlainText($"102 - Job '{jobId}' continues to be processed");
			return;
		}

		//job.Context.Options.

		//extract the binary images from layers
		var clerk = new RelayClerk("result");
		List<NamedMemory> binList = new();
		clerk.AqureWrite += (o, e) => {
			var nm = new NamedMemory {
				Memory = new MemoryStream(),
				Name = clerk.GetLabel(e.Name, e.Extension, e.Tag)
			};
			binList.Add(nm);
			e.Source = nm.Memory;
		};
		var eng = job.Context.Options.Engine.Item.Value;
		eng.SaveImage(job.Context.Layers, clerk);

		var data = new MultipartContent();
		foreach(var nm in binList) {
			var content = new ByteArrayContent(nm.Memory.ToArray());
			var contentDisp = new ContentDispositionHeaderValue("form-data") { Name = nm.Name };
			content.Headers.ContentDisposition = contentDisp;
			data.Add(content);
		}

		//write bins back to to http stream
		resp.ContentType = "multipart/form-data";
		var canSource = new CancellationTokenSource(TimeSpan.FromMinutes(60));
		data.CopyTo(resp.OutputStream, null, canSource.Token);
	}
}
