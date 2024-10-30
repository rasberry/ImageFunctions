using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Logging;
using Rasberry.Cli;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace ImageFunctions.ComfiUINodes;

internal static partial class Handlers
{
	public static void RunFunction(HttpListenerContext ctx)
	{
		if(!ctx.EnsureMethodIs(HttpMethod.Post)) { return; }
		using var resp = ctx.Response;
		var req = ctx.Request;
		var contentType = new ContentType(req.ContentType);

		if(!contentType.MediaType.EqualsIC("multipart/form-data")) {
			resp.StatusCode = (int)HttpStatusCode.UnprocessableContent;
			resp.WriteText($"422 - Only Content-Type: multipart/form-data is supported");
			return;
		}

		List<string> argsList = new();
		List<MemoryStream> binList = new();

		// https://stackoverflow.com/questions/20968492/reading-multipart-content-from-raw-http-request
		using var readStream = req.InputStream;
		var content = new StreamContent(readStream);
		content.Headers.ContentType = MediaTypeHeaderValue.Parse(req.ContentType);
		var outerMultipart = content.ReadAsMultipartAsync().GetAwaiter().GetResult();

		foreach(var outerPart in outerMultipart.Contents) {
			string name = outerPart.Headers.ContentDisposition.Name;
			var memoryData = new MemoryStream();
			outerPart.CopyTo(memoryData, ctx.Request.TransportContext, CancellationToken.None);
			if(TryGetUtf8String(memoryData, out string stringData)) {
				argsList.Add(name);
				argsList.Add(stringData);
			}
			else {
				//assume content is binary
				binList.Add(memoryData);
			}
		}

		var options = new ComfiCoreOptions();

		var job = new Job {
			Register = Program.Register,
			Layers = new Layers(),
		};



		//System.Net.Http.httpcontentm.
		//var content = new MultipartContent("form-data", contentType.Boundary);
		//var multiData = new MultipartFormDataContent(contentType.Boundary);
		//multiData.
		// TODO create job
		// ughh.. logging is not going to work as is.. 
		// maybe change IFunction to have an IFunctionContext object with all of the bits attached
		// (basically the Job object) and include logging
		// fs.Write(sep,0,sep.Length);
		// var help = $"{contentType.Name} {contentType.MediaType} {contentType.Boundary}";
		// var ct = Encoding.UTF8.GetBytes(help + "\r\n");
		// fs.Write(ct,0,ct.Length);
		// readStream.CopyTo(fs);

		//return job id
		resp.StatusCode = (int)HttpStatusCode.OK;
		resp.WriteText("ok");
	}

	const int MaxNonBinarySize = 1024;
	static bool TryGetUtf8String(MemoryStream stream, out string data)
	{
		data = null;
		if(stream.Length > MaxNonBinarySize) { return false; }
		var streamArr = stream.ToArray();
		var chars = new char[1024];
		if(!Encoding.UTF8.TryGetChars(streamArr, chars, out int _)) { return false; }
		data = new string(chars);
		return true;
	}

	class Job
	{
		public IRegister Register;
		public ICoreOptions CoreOptions;
		public ILayers Layers;
		public readonly ProgressBar Progress = new();

		public FunctionSpawner GetFunction(string name)
		{
			var reg = new FunctionRegister(Program.Register);
			if(!reg.Try(name, out var regFunction)) {
				return null;
			}
			return regFunction?.Item;
		}

		public bool Run(FunctionSpawner spawner, string[] args, out LoggerForJob log)
		{
			log = new LoggerForJob();
			var ctx = new FunctionContext {
				Register = Register,
				Layers = Layers,
				Options = CoreOptions,
				Log = log
			};
			var func = spawner.Invoke(ctx);
			return func.Run(args);
		}
	}

	class ComfiCoreOptions : ICoreOptions
	{
		public int? MaxDegreeOfParallelism { get; internal set; }
		public IRegisteredItem<Lazy<IImageEngine>> Engine { get; internal set; }
		public int? DefaultWidth { get; internal set; }
		public int? DefaultHeight { get; internal set; }
		public string ImageFormat;
		public string EngineName;
		public string FunctionName;
		internal ICoreLog Log;


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

			//take the first remaining option as the script name
			// all other options must be accounted for at this point
			p.Value<string>()
				.WhenGood(r => { FunctionName = r.Value; return r; })
			;

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
