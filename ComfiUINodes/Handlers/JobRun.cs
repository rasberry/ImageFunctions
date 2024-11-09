using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.FileIO;
using ImageFunctions.Core.Logging;
using Rasberry.Cli;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace ImageFunctions.ComfiUINodes;

internal static partial class Handlers
{
	[HttpRoute("/run")]
	public static void JobRun(HttpListenerContext ctx)
	{
		if(!ctx.EnsureMethodIs(HttpMethod.Post)) { return; }
		using var resp = ctx.Response;
		var req = ctx.Request;
		
		//check for valid content-type
		bool vaildContentType = false;
		if (req.ContentType != null) {
			var contentType = new ContentType(req.ContentType);
			vaildContentType = contentType.MediaType.EqualsIC("multipart/form-data");
		}
		if (!vaildContentType) {
			resp.StatusCode = (int)HttpStatusCode.UnprocessableContent;
			resp.WritePlainText($"422 - Only Content-Type: multipart/form-data is supported");
			return;
		}

		var log = new LoggerForJob();
		var layers = new Layers();

		//bucket incoming data into args and binaries
		List<string> argsList = new();
		List<NamedMemory> binList = new();
		if (!BucketRequestData(req, resp, argsList, binList, log)) {
			ErrorResponse(resp, HttpStatusCode.BadRequest, log);
			return;
		}

		var options = new ComfiCoreOptions(log);
		if(!options.ParseArgs(argsList.ToArray(), Program.Register)) {
			ErrorResponse(resp, HttpStatusCode.BadRequest, log);
			return;
		}
		if(!options.ProcessOptions(Program.Register)) {
			ErrorResponse(resp, HttpStatusCode.BadRequest, log);
			return;
		}
		if(!TryHydrateImages(layers, options.Engine.Item.Value, binList, log)) {
			ErrorResponse(resp, HttpStatusCode.BadRequest, log);
			return;
		}

		var context = new FunctionContext {
			Register = Program.Register,
			Layers = layers,
			Options = options,
			Log = log,
			Progress = new ProgressForJob()
		};
		var job = new Job { Context = context };
		int num = Interlocked.Increment(ref JobCounter);
		if(!JobHoard.TryAdd(num, job)) {
			log.Error(Note.UnableToQueueJob($"{options.FunctionName} [#{num}]"));
			ErrorResponse(resp, HttpStatusCode.InternalServerError, log);
			return;
		}

		var task = Task.Run(() => {
			job.Run(argsList.ToArray());
		});

		resp.StatusCode = (int)HttpStatusCode.OK;
		var json = new JobStatusData {
			job = num,
			status = job.Status,
			progress = job.Progress.Amount
		};
		resp.WriteJson(json);
	}

	static void ErrorResponse(HttpListenerResponse resp, HttpStatusCode status, LoggerForJob log)
	{
		resp.StatusCode = (int)status;
		resp.WriteJson(log.LogMessages);
	}

	static bool BucketRequestData(HttpListenerRequest req, HttpListenerResponse resp, List<string> argsList, List<NamedMemory> binList, LoggerForJob log)
	{
		// https://stackoverflow.com/questions/20968492/reading-multipart-content-from-raw-http-request
		using var readStream = req.InputStream;
		var content = new StreamContent(readStream);
		content.Headers.ContentType = MediaTypeHeaderValue.Parse(req.ContentType);
		MultipartMemoryStreamProvider outerMultipart = null;
		try {
			outerMultipart = content.ReadAsMultipartAsync().GetAwaiter().GetResult();
		}
		catch(Exception e) {
			log.Error($"Could not process request",e);
			return false;
		}

		foreach(var outerPart in outerMultipart.Contents) {
			string name = outerPart.Headers.ContentDisposition.Name;
			name = name?.Trim('"'); //not sure why the names all have quotes
			var memoryData = new MemoryStream();
			outerPart.CopyTo(memoryData, req.TransportContext, CancellationToken.None);
			memoryData.Seek(0, SeekOrigin.Begin); //reset the pointer for reading

			if(TryGetUtf8String(memoryData, out string stringData)) {
				argsList.Add(name);
				argsList.Add(stringData);
			}
			else {
				//assume content is binary
				binList.Add(new() {
					Memory = memoryData,
					Name = name
				});
			}
		}

		return true;
	}

	static bool TryHydrateImages(ILayers layers, IImageEngine engine, List<NamedMemory> binList, LoggerForJob log)
	{
		foreach(var nm in binList) {
			var clerk = new RelayClerk(nm.Name);
			clerk.AqureRead += (o, e) => { e.Source = nm.Memory; };
			try {
				engine.LoadImage(layers, clerk, nm.Name);
			}
			catch(Exception e) {
				log.Error(Note.CouldNotLoadImage(nm.Name), e);
				return false;
			}
		}
		return true;
	}

	const int MaxNonBinarySize = 1024;
	static bool TryGetUtf8String(MemoryStream stream, out string data)
	{
		data = null;
		if(stream.Length > MaxNonBinarySize) { return false; }
		var streamArr = stream.ToArray();
		var chars = new char[MaxNonBinarySize];
		if(!Encoding.UTF8.TryGetChars(streamArr, chars, out int count)) { return false; }
		data = new string(chars, 0, count);
		return true;
	}

	class Job
	{
		public IFunctionContext Context;
		public readonly ProgressForJob Progress = new();
		public JobStatusKind Status { get; private set; } = JobStatusKind.NotStarted;

		ComfiCoreOptions Options { get { return (ComfiCoreOptions)Context.Options; } }

		FunctionSpawner GetFunction(string name)
		{
			var reg = new FunctionRegister(Program.Register);
			if(!reg.Try(name, out var regFunction)) {
				return null;
			}
			return regFunction?.Item;
		}

		public void Run(string[] args)
		{
			try {
				Status = JobStatusKind.Started;
				var spawner = GetFunction(Options.FunctionName);
				var func = spawner.Invoke(Context);
				var ret = func.Run(args);
				Status = ret ? JobStatusKind.Finished : JobStatusKind.Failed;
			}
			catch(Exception e) {
				Context.Log.Error(Note.FunctionFailed(Options.FunctionName), e);
				Status = JobStatusKind.Failed;
			}
		}
	}

	struct NamedMemory
	{
		public MemoryStream Memory;
		public string Name;
	}

	enum JobStatusKind
	{
		NotStarted = 0,
		Started = 1,
		Finished = 2,
		Failed = 3
	}

	class ComfiCoreOptions : ICoreOptions
	{
		public ComfiCoreOptions(ICoreLog log)
		{
			Log = log;
		}

		public int? MaxDegreeOfParallelism { get; internal set; }
		public IRegisteredItem<Lazy<IImageEngine>> Engine { get; internal set; }
		public int? DefaultWidth { get; internal set; }
		public int? DefaultHeight { get; internal set; }
		public string ImageFormat;
		public string EngineName;
		public string FunctionName;
		readonly ICoreLog Log;

		//only some of the options from cli are supported
		// sb.ND(1, "-# / --size (width) (height)", "Set the default size in pixels when no images are loaded");
		// sb.ND(1, "-f / --format (name)", "Save any output files as specified (engine supported) format");
		// sb.ND(1, "-x / --max-threads (number)", "Restrict parallel processing to a given number of threads (defaults to # of cores)");
		// sb.ND(1, "-e / --engine (name)", "Select (a registered) image engine (default first available)");
		public bool ParseArgs(string[] args, IRegister register)
		{
			ParseParams p = new(args);

			if(p.Scan<int?, int?>(new[] { "--size", "-#" })
				.WhenGood(r => { (DefaultWidth, DefaultHeight) = r.Value; return r; })
				.WhenInvalidTellDefault(Log)
				.IsInvalid()
			) {
				return false;
			}

			if(p.Scan<string>(new[] { "--format", "-f" })
				.WhenGood(r => { ImageFormat = r.Value; return r; })
				.WhenInvalidTellDefault(Log)
				.IsInvalid()
			) {
				return false;
			}

			if(p.Scan<int>(new[] { "--max-threads", "-x" })
				.WhenInvalidTellDefault(Log)
				.WhenGood(r => {
					if(r.Value < 1) {
						Log.Error(Note.MustBeGreaterThan(r.Name, 0));
						return r with { Result = ParseParams.Result.UnParsable };
					}
					MaxDegreeOfParallelism = r.Value; return r;
				})
				.IsInvalid()
			) {
				return false;
			}

			if(p.Scan<string>(new[] { "--engine", "-e" })
				.WhenGood(r => { EngineName = r.Value; return r; })
				.WhenInvalidTellDefault(Log)
				.IsInvalid()
			) {
				return false;
			}

			if(p.Scan<string>(new[] { "--name", "-n" })
				.WhenGood(r => { FunctionName = r.Value; return r; })
				.WhenInvalidTellDefault(Log)
				.IsInvalid()
			) {
				return false;
			}

			if(!ProcessOptions(register)) {
				return false;
			}

			return true;
		}

		internal bool ProcessOptions(IRegister register)
		{
			if(!register.TrySelectEngine(EngineName, Log, out var engineEntry)) {
				return false;
			}

			if(!engineEntry.Item.Value.TryDetermineImageFormat(ImageFormat, Log, out _)) {
				return false;
			}

			Engine = engineEntry;

			if(String.IsNullOrWhiteSpace(FunctionName)) {
				Log.Error(Note.MustProvideInput("function name"));
				return false;
			}
			return true;
		}

		public void Usage(StringBuilder sb, IRegister register)
		{
			//TODO
		}
	}
}
