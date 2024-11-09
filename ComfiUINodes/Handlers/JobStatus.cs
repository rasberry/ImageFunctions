using ImageFunctions.Core.Logging;
using System.Collections.Concurrent;
using System.Net;

namespace ImageFunctions.ComfiUINodes;

internal static partial class Handlers
{
	[HttpRoute("/status")]
	public static void JobStatus(HttpListenerContext ctx)
	{
		if(!ctx.EnsureMethodIs(HttpMethod.Get)) { return; }

		var req = ctx.Request;
		using var resp = ctx.Response;

		var sjob = req.QueryString.Get("job");
		if(!TryFindJob(req, resp, sjob, out var job, out int jobId)) {
			return;
		}

		var respJson = new JobStatusData {
			job = jobId,
			progress = job.Progress.Amount,
			status = job.Status
		};
		resp.StatusCode = (int)HttpStatusCode.OK;
		resp.WriteJson(respJson);
	}

	static bool TryFindJob(HttpListenerRequest req, HttpListenerResponse resp, string sjob, out Job job, out int jobId)
	{
		job = null;
		if(!int.TryParse(sjob, out jobId)) {
			resp.StatusCode = (int)HttpStatusCode.BadRequest;
			var json = new JobStatusData { error = Note.InvalidJobId(sjob) };
			resp.WriteJson(json);
			return false;
		}

		if(!JobHoard.TryGetValue(jobId, out job)) {
			resp.StatusCode = (int)HttpStatusCode.BadRequest;
			var json = new JobStatusData { error = Note.JobIdNotFound(sjob) };
			resp.WriteJson(json);
			return false;
		}

		return true;
	}

	static readonly ConcurrentDictionary<int, Job> JobHoard = new();
	static int JobCounter = 1;

	class JobStatusData
	{
		public int job;
		public double progress;
		public JobStatusKind status;
		public string error;
	}
}
