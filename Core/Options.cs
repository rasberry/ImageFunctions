using System.Text;
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

internal class Options : ICoreOptions
{
	public Options(IRegister register)
	{
		Register = register;
	}

	public void Usage(StringBuilder sb)
	{
		sb.ND(0,"Usage: "+nameof(ImageFunctions)+" [options] [function name] [-- function options]");
		sb.WT();
		sb.WT(0,"Options:");
		sb.ND(1,"-h / --help"                 ,"Show help / full help (provide a function name to show only that help instead");
		sb.ND(1,"-i / --image (file)"         ,"Load this image as a layer. Supports images with multiple layers");
		sb.ND(1,"-# / --size (width) (height)","Set the default size in pixels when no images are loaded");
		sb.ND(1,"-f / --format (name)"        ,"Save any output files as specified (engine supported) format");
		sb.ND(1,"-x / --max-threads (number)" ,"Restrict parallel processing to a given number of threads (defaults to # of cores)");
		sb.ND(1,"-e / --engine (name)"        ,"Select (a registered) image engine (default first available)");
		sb.ND(1,"-v / --verbose"              ,"Show additional messages");
		sb.ND(1,"-o / --output (name)"        ,"Output file name");
		sb.ND(1,"-lf / --formats"             ,"List engine supported image formats");
		sb.ND(1,"-ln / --namespace (name)"    ,"List registered items in given namespace (specify 'all' to list everything)");
		sb.ND(1,"--"                          ,"Pass all remaining options to the function");
	}

	public bool ParseArgs(string[] args, IRegister _)
	{
		if (args.Length < 1) {
			Show |= PickShow.Usage;
			return true; //there's no arguments so nothing else to do
		}

		//split the args into two lists at the "--"
		var regularArgs = new List<string>();
		var scriptArgs = new List<string>();
		bool seperatorFound = false;
		foreach(var a in args) {
			if (a == "--") {
				seperatorFound = true;
			}
			else if (seperatorFound) {
				scriptArgs.Add(a);
			}
			else {
				regularArgs.Add(a);
			}
		}
		FunctionArgs = scriptArgs.ToArray();

		var p = new ParseParams(regularArgs.ToArray());
		if (p.Has("-h").IsGood()) {
			Show |= PickShow.Usage;
		}
		if (p.Has("--help").IsGood()) {
			Show |= PickShow.All;
		}

		var ons = p.Default(new[]{"-ln","--namespace"},out HelpNameSpace);
		if (ons.IsInvalid()) {
			Tell.MissingArgument("--namespace");
			return false;
		}
		else if (ons.IsGood()) {
			Show |= PickShow.Registered;
		}

		//engine selection needs to happen before other engine specific options
		var oeng = p.Default(new[]{"-e","--engine"}, out EngineName);
		if (oeng.IsInvalid()) {
			Tell.MissingArgument("--engine");
			return false;
		}

		var omtr = p.Default(new[]{"-x","--max-threads"},out int mdop, 0);
		if (omtr.IsInvalid()) {
			return false;
		}
		else if(omtr.IsGood()) {
			if (mdop < 1) {
				Tell.MustBeGreaterThanZero("--max-threads");
				return false;
			}
			MaxDegreeOfParallelism = mdop;
		}

		var ofmt = p.Default(new[]{"-f","--format"},out _imageFormat);
		if (ofmt.IsInvalid()) {
			return false;
		}

		var oon = p.Default(new[]{"-o","--output"}, out _outputName);
		if (oon.IsMissingArgument()) {
			Tell.MissingArgument("--output");
			return false;
		}

		var osz = p.Default(new[] {"-#","--size"}, out _defaultWidth, out _defaultHeight);
		if (osz.IsInvalid()) {
			Tell.CouldNotParse("--size");
			return false;
		}

		if (p.Has("-v","--verbose").IsGood()) {
			Log.BeVerbose = true;
		}

		if (p.Has("-lf","--formats").IsGood()) {
			Show |= PickShow.Formats;
		}

		//grab all of the inputs images
		if (!EnumerateInputImages(p)) {
			return false;
		}

		//take the first remaining option as the script name
		// all other options must be accounted for at this point
		p.Default(out _functionName);
		if (Show.HasFlag(PickShow.Usage) && !String.IsNullOrWhiteSpace(_functionName)) {
			Show |= PickShow.Function;
		}

		return true;
	}

	public bool ProcessOptions()
	{
		StringBuilder sb = new StringBuilder();

		//show normal options and function options
		if (Show.HasFlag(PickShow.Usage)) {
			Usage(sb);
		}

		if (Show.HasFlag(PickShow.Function)) {
			if (!ShowFunctionHelp(_functionName, sb)) {
				return false;
			}
		}

		//show registered items
		if (Show.HasFlag(PickShow.Registered)) {
			bool namespaceGiven = !String.IsNullOrWhiteSpace(HelpNameSpace);
			var space = namespaceGiven ? HelpNameSpace : "all";
			ShowRegisteredItems(space, sb);
		}

		//need to select the engine so we can show formats
		var er = new EngineRegister(Register);
		if (!String.IsNullOrWhiteSpace(EngineName)) {
			if (!er.Try(EngineName, out var engineEntry)) {
				Tell.NotRegistered(engineEntry.NameSpace, engineEntry.Name);
				return false;
			}
			Engine = engineEntry;
		}
		else {
			EngineName = EngineRegister.SixLaborsString;
			Engine = er.Get(EngineName);
		}

		//show formats
		if (Show.HasFlag(PickShow.Formats)) {
			var eng = Engine.Item.Value;
			sb.WT();
			sb.WT(0,$"Supported Image Formats for Selected Engine - {Engine}");
			sb.WT(0,"Legend: R = Reading, W = Writting, M = Multiple layers");
			foreach(var f in eng.Formats()) {
				string rw = $"[{(f.CanRead ? "R" : " ")}{(f.CanWrite ? "W" : " ")}{(f.MultiFrame ? "M" : " ")}]";
				sb.ND(1,f.Name,$"{rw} {f.Description}");
			}
		}

		//if there's any help to print do so now
		if (sb.Length > 0) {
			Log.Message(sb.ToString());
			// stop if we've printed any help
			return false;
		}

		if (!DetermineImageFormat()) {
			return false;
		}

		if (String.IsNullOrWhiteSpace(_functionName)) {
			Tell.MustProvideInput("function name");
			return false;
		}

		if (String.IsNullOrWhiteSpace(_outputName)) {
			var name = nameof(ImageFunctions).ToLowerInvariant();
			var date = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss");
			_outputName = $"{name}-{date}";
		}

		return true;
	}

	void ShowRegisteredItems(string @namespace, StringBuilder sb)
	{
		IEnumerable<string> keyList = null;

		bool all;
		keyList = ((all = @namespace.EqualsIC("all"))
			? Register.All()
			: Register.All().Where(k => k.NameSpace.StartsWithIC(@namespace))
		)
		.Select(n => $"{n.NameSpace}.{n.Name}").Order();

		string suffix = all ? "" : $" for '{@namespace}'";
		sb.WT();
		sb.WT(0,$"Registered Items{suffix}:");
		foreach(var k in keyList) {
			sb.WT(1,k);
		}
	}

	bool ShowFunctionHelp(string name, StringBuilder sb)
	{
		var fn = new FunctionRegister(Register);
		IEnumerable<string> list = null;
		if (!String.IsNullOrWhiteSpace(name)) {
			//show specific help for given function
			if (!fn.Try(name,out _)) {
				Tell.NotRegistered(fn.Namespace,name);
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
			sb.WT(0,$"Function {key}:");
			var inst = funcItem.Item.Invoke(Register, null, this);
			inst.Usage(sb);
		}

		return true;
	}

	bool DetermineImageFormat()
	{
		var eng = Engine.Item.Value;
		bool formatGiven = !String.IsNullOrWhiteSpace(_imageFormat);
		ImageFormat? found = null;
		foreach(var f in  eng.Formats()) {
			if (formatGiven && f.Name.EqualsIC(_imageFormat)) {
				found = f;
			}
			else if (f.Name.EqualsIC("png")) {
				found = f;
			}
		}

		if (found == null) {
			Tell.NoImageFormatFound(_imageFormat);
		}

		return true;
	}

	bool EnumerateInputImages(ParseParams p)
	{
		bool found = true;
		while(found) {
			var oim = p.Default(new[]{"-i","--image"},out string name);
			if (oim.IsMissingArgument()) {
				Tell.MissingArgument("--image");
				return false;
			}
			else if (oim.IsGood()) {
				_imageFileNames.Add(name);
			}
			else {
				found = false;
			}
		}

		return true;
	}

	//Options only helper parameters
	string HelpNameSpace = null;
	string EngineName = null;
	PickShow Show = PickShow.None;
	readonly List<string> _imageFileNames = new();
	string _functionName;
	string _imageFormat;
	string _outputName;
	readonly IRegister Register;
	int? _defaultWidth;
	int? _defaultHeight;

	//Global options
	public IRegisteredItem<Lazy<IImageEngine>> Engine { get; internal set; }
	public int? MaxDegreeOfParallelism  { get; internal set; }
	public string OutputName { get { return _outputName; }}
	public string ImageFormat { get { return _imageFormat; }}
	public string FunctionName { get { return _functionName; }}
	public string[] FunctionArgs { get; internal set; }
	public IReadOnlyList<string> ImageFileNames { get { return _imageFileNames.AsReadOnly(); }}
	public int? DefaultWidth { get { return _defaultWidth; }}
	public int? DefaultHeight { get { return _defaultHeight; }}

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