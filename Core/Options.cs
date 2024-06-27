using Rasberry.Cli;

namespace ImageFunctions.Core;

/*
sr - show registered
su - show usage == -h | !-lf & !-ln & !gf
sf - show formats == -lf
sh - show function help == -ln
gf - given function name == -h && gf

-h -lf -ln gf | sf su sr sh
 0   0   0  0 |  0  1  0  0
 0   0   0  1 |  0  0  0  0
 0   0   1  0 |  0  0  1  0
 0   0   1  1 |  0  0  1  0
 0   1   0  0 |  1  0  0  0
 0   1   0  1 |  1  0  0  0
 0   1   1  0 |  1  0  1  0
 0   1   1  1 |  1  0  1  0
 1   0   0  0 |  0  1  0  0
 1   0   0  1 |  0  1  0  1
 1   0   1  0 |  0  1  1  0
 1   0   1  1 |  0  1  1  1
 1   1   0  0 |  1  1  0  0
 1   1   0  1 |  1  1  0  1
 1   1   1  0 |  1  1  1  0
 1   1   1  1 |  1  1  1  1
*/
#pragma warning disable CA1861 //Avoid constant arrays as arguments - There's little to no performance gain for doing this here
internal class Options : ICoreOptions
{
	public Options(IRegister register)
	{
		Register = register;
	}

	public void Usage(StringBuilder sb, IRegister _)
	{
		sb.ND(0, "Usage: " + nameof(ImageFunctions) + " [options] [function name] [-- function options]");
		sb.WT();
		sb.WT(0, "Options:");
		sb.ND(1, "-h / --help", "Show help / full help (provide a function name to show only that help instead");
		sb.ND(1, "-i / --image (file)", "Load this image as a layer. Supports images with multiple layers");
		sb.ND(1, "-# / --size (width) (height)", "Set the default size in pixels when no images are loaded");
		sb.ND(1, "-f / --format (name)", "Save any output files as specified (engine supported) format");
		sb.ND(1, "-x / --max-threads (number)", "Restrict parallel processing to a given number of threads (defaults to # of cores)");
		sb.ND(1, "-e / --engine (name)", "Select (a registered) image engine (default first available)");
		sb.ND(1, "-v / --verbose", "Show additional messages");
		sb.ND(1, "-o / --output (name)", "Output file name");
		sb.ND(1, "-lf / --formats", "List engine supported image formats");
		sb.ND(1, "-ln / --namespace (name)", "List registered items in given namespace (specify 'all' to list everything)");
		sb.ND(1, "--", "Pass all remaining options to the function");
	}

	public bool ParseArgs(string[] args, IRegister _)
	{
		if(args.Length < 1) {
			Show |= PickShow.Usage;
			return true; //there's no arguments so nothing else to do
		}

		//split the args into two lists at the "--"
		var regularArgs = new List<string>();
		var scriptArgs = new List<string>();
		bool seperatorFound = false;
		foreach(var a in args) {
			if(a == "--") {
				seperatorFound = true;
			}
			else if(seperatorFound) {
				scriptArgs.Add(a);
			}
			else {
				regularArgs.Add(a);
			}
		}
		FunctionArgs = scriptArgs.ToArray();

		var p = new ParseParams(regularArgs.ToArray());
		if(p.Has("-h").IsGood()) {
			Show |= PickShow.Usage;
		}
		if(p.Has("--help").IsGood()) {
			Show |= PickShow.All;
		}

		if(p.Scan<string>(new[] { "--namespace", "-ln" })
			.WhenGood(r => {
				HelpNameSpace = r.Value;
				Show |= PickShow.Registered;
				return r;
			})
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<string>(new[] { "--engine", "-e" })
			.WhenGood(r => { EngineName = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<int>(new[] { "--max-threads", "-x" })
			.WhenInvalidTellDefault()
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

		if(p.Scan<string>(new[] { "--format", "-f" })
			.WhenGood(r => { _imageFormat = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<string>(new[] { "--output", "-o" })
			.WhenGood(r => { _outputName = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<int?, int?>(new[] { "--size", "-#" })
			.WhenGood(r => { (_defaultWidth, _defaultHeight) = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Has("-v", "--verbose").IsGood()) {
			Log.BeVerbose = true;
		}

		if(p.Has("-lf", "--formats").IsGood()) {
			Show |= PickShow.Formats;
		}

		//grab all of the inputs images
		if(!EnumerateInputImages(p)) {
			return false;
		}

		//take the first remaining option as the script name
		// all other options must be accounted for at this point
		p.Value<string>()
			.WhenGood(r => { _functionName = r.Value; return r; })
		;

		if(Show.HasFlag(PickShow.Usage) && !String.IsNullOrWhiteSpace(_functionName)) {
			Show |= PickShow.Function;
		}

		return true;
	}

	public bool ProcessOptions()
	{
		StringBuilder sb = new StringBuilder();

		//show normal options and function options
		if(Show.HasFlag(PickShow.Usage)) {
			Usage(sb, Register);
		}

		if(Show.HasFlag(PickShow.Function)) {
			if(!ShowFunctionHelp(_functionName, sb)) {
				return false;
			}
		}

		//show registered items
		if(Show.HasFlag(PickShow.Registered) || NameSpaceList != null) {
			ShowRegisteredItems(sb, Show.HasFlag(PickShow.Registered));
		}

		//need to select the engine so we can show formats
		var er = new EngineRegister(Register);
		if(!String.IsNullOrWhiteSpace(EngineName)) {
			if(!er.Try(EngineName, out var engineEntry)) {
				Log.Error(Note.NotRegistered(engineEntry.NameSpace, engineEntry.Name));
				return false;
			}
			Engine = engineEntry;
		}
		else {
			EngineName = EngineRegister.SixLaborsString;
			Engine = er.Get(EngineName);
		}

		//show formats
		if(Show.HasFlag(PickShow.Formats)) {
			ShowFormats(sb);
		}

		//if there's any help to print do so now
		if(sb.Length > 0) {
			Log.Message(sb.ToString());
			// stop if we've printed any help
			return false;
		}

		if(!DetermineImageFormat()) {
			return false;
		}

		if(String.IsNullOrWhiteSpace(_functionName)) {
			Log.Error(Note.MustProvideInput("function name"));
			return false;
		}

		if(String.IsNullOrWhiteSpace(_outputName)) {
			var name = nameof(ImageFunctions).ToLowerInvariant();
			var date = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.CurrentCulture);
			_outputName = $"{name}-{date}";
		}

		return true;
	}

	//a = nothing or all
	//n = namespace given
	//x = aux flags
	//a n x
	//0 0 0 not possible
	//0 0 1 print aux
	//0 1 0 print n
	//0 1 1 print n | aux
	//1 0 0 print a
	//1 0 1 print a
	//1 1 0 not possible
	//1 1 1 not possible
	void ShowRegisteredItems(StringBuilder sb, bool hasPickShow)
	{
		//Log.Debug($"UsageFlags={UsageFlags}");
		var ns = hasPickShow && String.IsNullOrWhiteSpace(HelpNameSpace) ? "all" : HelpNameSpace;

		bool joinFilters(INameSpaceName k) {
			bool show = (hasPickShow && k.NameSpace.StartsWithIC(ns)) || IsNameSpaceFlagged(k);
			//Log.Debug($"{k.NameSpace} {show}");
			return show;
		}

		bool all;
		var keyList = ((all = ns.EqualsIC("all"))
			? Register.All()
			: Register.All().Where(joinFilters)
		)
		.OrderBy(n => $"{n.NameSpace}.{n.Name}");

		string currentSpace = "";
		sb.WT();
		sb.WT(0, $"Registered Items:");
		foreach(var k in keyList) {
			if (k.NameSpace != currentSpace) {
				sb.WT();
				sb.WT(0,$"{k.NameSpace}:");
				currentSpace = k.NameSpace;
			}
			if (!Register.TryPrintCustomHelp(sb,k)) {
				sb.WT(1, $"{k.Name}");
			}
		}
	}

	bool IsNameSpaceFlagged(INameSpaceName n)
	{
		if (NameSpaceList == null) {
			return false;
		}

		return NameSpaceList.Contains(n.NameSpace);
	}

	bool ShowFunctionHelp(string name, StringBuilder sb)
	{
		var fn = new FunctionRegister(Register);
		IEnumerable<string> list;
		if(!String.IsNullOrWhiteSpace(name)) {
			//show specific help for given function
			if(!fn.Try(name, out _)) {
				Log.Error(Note.NotRegistered(fn.Namespace, name));
				return false;
			}
			list = new[] { name };
		}
		else {
			//we want everything so show all function help
			list = fn.All().Order();
		}

		foreach(string key in list) {
			var funcItem = fn.Get(key);
			sb.WT();
			sb.WT(0, $"Function {key}:");
			var inst = funcItem.Item.Invoke(Register, null, this);
			var opts = inst.Options;
			if (opts is IUsageProvider uip) {
				NameSpaceList = GetFlagsFromUsageInfo(uip.GetUsageInfo());
			}
			inst.Options.Usage(sb, Register);
		}

		return true;
	}

	List<string> GetFlagsFromUsageInfo(Usage info)
	{
		List<string> nameSpaceList = new();
		foreach(var p in info.Parameters) {
			if (p is UsageRegistered ur) {
				nameSpaceList.Add(ur.NameSpace);
			}
		}
		return nameSpaceList;
	}

	void ShowFormats(StringBuilder sb)
	{
		var eng = Engine.Item.Value;
		sb.WT();
		sb.WT(0, $"Supported Image Formats for Selected Engine - {Engine.Name}");
		sb.WT(0, "Legend: R = Reading, W = Writting, M = Multiple layers");
		foreach(var f in eng.Formats()) {
			string rw = $"[{(f.CanRead ? "R" : " ")}{(f.CanWrite ? "W" : " ")}{(f.MultiFrame ? "M" : " ")}]";
			sb.ND(1, f.Name, $"{rw} {f.Description}");
		}
	}

	bool DetermineImageFormat()
	{
		var eng = Engine.Item.Value;
		bool formatGiven = !String.IsNullOrWhiteSpace(_imageFormat);
		ImageFormat? found = null;
		foreach(var f in eng.Formats()) {
			if(formatGiven && f.Name.EqualsIC(_imageFormat)) {
				found = f;
			}
			else if(f.Name.EqualsIC("png")) {
				found = f;
			}
		}

		if(found == null) {
			Log.Error(Note.NoImageFormatFound(_imageFormat));
			return false;
		}

		return true;
	}

	bool EnumerateInputImages(ParseParams p)
	{
		bool found = true;
		while(found) {
			var oim = p.Scan<string>(new[] { "-i", "--image" });
			if(oim.IsMissingArgument()) {
				Log.Error(Note.MissingArgument("--image"));
				return false;
			}
			else if(oim.IsGood()) {
				_imageFileNames.Add(oim.Value);
			}
			else {
				found = false;
			}
		}

		return true;
	}

	//Options only helper parameters
	string HelpNameSpace;
	string EngineName;
	PickShow Show = PickShow.None;
	readonly List<string> _imageFileNames = new();
	string _functionName;
	string _imageFormat;
	string _outputName;
	readonly IRegister Register;
	int? _defaultWidth;
	int? _defaultHeight;
	List<string> NameSpaceList;

	//Global options
	public IRegisteredItem<Lazy<IImageEngine>> Engine { get; internal set; }
	public int? MaxDegreeOfParallelism { get; internal set; }
	public string OutputName { get { return _outputName; } }
	public string ImageFormat { get { return _imageFormat; } }
	public string FunctionName { get { return _functionName; } }
	public string[] FunctionArgs { get; internal set; }
	public IReadOnlyList<string> ImageFileNames { get { return _imageFileNames.AsReadOnly(); } }
	public int? DefaultWidth { get { return _defaultWidth; } }
	public int? DefaultHeight { get { return _defaultHeight; } }

	[Flags]
	enum PickShow
	{
		None = 0,
		Usage = 1,
		Formats = 2,
		Registered = 4,
		Function = 8,
		All = 8 + 4 + 2 + 1
	}
}
#pragma warning restore CA1861
