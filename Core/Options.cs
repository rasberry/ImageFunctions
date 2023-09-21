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

static class Options
{
	public static void ShowUsage(StringBuilder sb)
	{
		sb.ND(0,"Usage: "+nameof(ImageFunctions)+" [options] [function name] [-- function options]");
		sb.WT();
		sb.WT(0,"Options:");
		sb.ND(1,"-h / --help"                 ,"Show help / full help (provide a function name to show only that help instead");
		sb.ND(1,"-i / --image (file)"         ,"Load this image as a layer. Supports images with multiple layers");
		sb.ND(1,"-f / --format (name)"        ,"Save any output files as specified (registered) format");
		sb.ND(1,"-x / --max-threads (number)" ,"Restrict parallel processing to a given number of threads (defaults to # of cores)");
		sb.ND(1,"-e / --engine (name)"        ,"Select (a registered) image engine (default first available)");
		sb.ND(1,"-v / --verbose"              ,"Show additional messages");
		sb.ND(1,"-o / --output (name)"        ,"Output file name");
		sb.ND(1,"-lf / --formats"             ,"List engine supported image formats");
		sb.ND(1,"-ln / --namespace (name)"    ,"List registered items in given namespace (specify 'all' to list everything)");
		sb.ND(1,"--"                          ,"Pass all remaining options to the function");
	}

	public static bool ParseArgs(string[] args)
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

		var ofmt = p.Default(new[]{"-f","--format"},out ImageFormat);
		if (ofmt.IsInvalid()) {
			return false;
		}

		var oon = p.Default(new[]{"-o","--output"},out OutputName);
		if (oon.IsMissingArgument()) {
			Tell.MissingArgument("--output");
		}

		if (p.Has("-v","--verbose").IsGood()) {
			BeVerbose = true;
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
		p.Default(out FunctionName);
		if (Show.HasFlag(PickShow.Usage) && !String.IsNullOrWhiteSpace(FunctionName)) {
			Show |= PickShow.Function;
		}

		return true;
	}

	public static bool ProcessOptions(IRegister register)
	{
		StringBuilder sb = new StringBuilder();

		//show normal options and function options
		if (Show.HasFlag(PickShow.Usage)) {
			ShowUsage(sb);
		}

		if (Show.HasFlag(PickShow.Function)) {
			if (!ShowFunctionHelp(FunctionName, register, sb)) {
				return false;
			}
		}

		//show registered items
		if (Show.HasFlag(PickShow.Registered)) {
			bool namespaceGiven = !String.IsNullOrWhiteSpace(HelpNameSpace);
			var space = namespaceGiven ? HelpNameSpace : "all";
			ShowRegisteredItems(space, register, sb);
		}

		//need to select the engine so we can show formats
		var er = new EngineRegister(register);
		if (!String.IsNullOrWhiteSpace(EngineName)) {
			if (!er.Try(EngineName, out Engine)) {
				Tell.NotRegistered(er.Namespace,EngineName);
				return false;
			}
		}
		else {
			EngineName = EngineRegister.SixLaborsString;
			Engine = er.Get(EngineName);
		}

		//show formats
		if (Show.HasFlag(PickShow.Formats)) {
			var eng = Engine.Value;
			sb.WT();
			sb.WT(0,$"Supported Image Formats for Selected Engine - {EngineName}");
			sb.WT(0,"Legend: R = Reading, W = Writting, M = Multiple layers");
			foreach(var f in eng.Formats()) {
				string rw = $"[{(f.CanRead ? "R" : " ")}{(f.CanWrite ? "W" : " ")}{(f.MultiFrame ? "M" : " ")}]";
				sb.ND(1,f.Name,$"{rw} {f.Description}");
			}
		}

		//if there's any help to print do so now
		if (sb.Length > 0) {
			Log.Info(sb.ToString());
			// stop if we've printed any help
			return false;
		}

		if (!DetermineImageFormat()) {
			return false;
		}

		if (String.IsNullOrWhiteSpace(FunctionName)) {
			Tell.MustProvideInput("function name");
			return false;
		}

		if (String.IsNullOrWhiteSpace(OutputName)) {
			var name = nameof(ImageFunctions).ToLowerInvariant();
			var date = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss");
			OutputName = $"{name}-{date}";
		}

		return true;
	}

	static void ShowRegisteredItems(string @namespace, IRegister register, StringBuilder sb)
	{
		IEnumerable<string> keyList = null;

		bool all;
		string nameWithDot = @namespace + ".";
		keyList = (all = @namespace.EqualsIC("all"))
			? register.All()
			: register.All().Where(k => k.StartsWithIC(nameWithDot))
		;

		string suffix = all ? "" : $" for '{@namespace}'";
		sb.WT();
		sb.WT(0,$"Registered Items{suffix}:");
		foreach(var k in keyList) {
			sb.WT(1,k);
		}
	}

	static bool ShowFunctionHelp(string name, IRegister register, StringBuilder sb)
	{
		var fn = new FunctionRegister(register);
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
			list = fn.All();
		}

		foreach(string key in list) {
			var lzFunc = fn.Get(key);
			sb.WT();
			sb.WT(0,$"Function {key}:");
			lzFunc.Value.Usage(sb);
		}

		return true;
	}

	static bool DetermineImageFormat()
	{
		var eng = Engine.Value;
		bool formatGiven = !String.IsNullOrWhiteSpace(ImageFormat);
		ImageFormat? found = null;
		foreach(var f in  eng.Formats()) {
			if (formatGiven && f.Name.EqualsIC(ImageFormat)) {
				found = f;
			}
			else if (f.Name.EqualsIC("png")) {
				found = f;
			}
		}

		if (found == null) {
			Tell.NoImageFormatFound(ImageFormat);
		}

		return true;
	}

	static bool EnumerateInputImages(ParseParams p)
	{
		bool found = true;
		while(found) {
			var oim = p.Default(new[]{"-i","--image"},out string name);
			if (oim.IsMissingArgument()) {
				Tell.MissingArgument("--image");
				return false;
			}
			else if (oim.IsGood()) {
				ImageFileNames.Add(name);
			}
			else {
				found = false;
			}
		}

		return true;
	}

	//Options only helper parameters
	static string HelpNameSpace = null;
	static string EngineName = null;
	static PickShow Show = PickShow.None;

	//Global options
	public static Lazy<IImageEngine> Engine;
	public static int? MaxDegreeOfParallelism;
	public static string OutputName;
	public static string ImageFormat;
	public static string FunctionName;
	public static string[] FunctionArgs;
	public static bool BeVerbose = false;
	public static List<string> ImageFileNames = new();

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