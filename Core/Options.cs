using System.Text;
using Rasberry.Cli;

namespace ImageFunctions.Core;

static class Options
{
	public static void ShowUsage(IRegister register)
	{
		StringBuilder sb = new StringBuilder();
		sb.ND(0,"Usage "+nameof(ImageFunctions)+" [options] [function name] [-- function options]");
		sb.ND(1,"-h / --help"                 ,"Show full help (provide a function name to show only that help instead");
		sb.ND(1,"-f / --format (name)"        ,"Save any output files as specified format");
		sb.ND(1,"-x / --max-threads (number)" ,"Restrict parallel processing to a given number of threads (defaults to # of cores)");
		sb.ND(1,"-e / --engine (name)"        ,"Select image engine (default first available)");
		sb.ND(1,"-q / --quiet"                ,"Suppress informational messages");
		sb.ND(1,"-le / --engines"             ,"List registered engines");
		sb.ND(1,"-lf / --functions"           ,"List registered functions");
		sb.ND(1,"-lc / --colors"              ,"List available colors");
		sb.ND(1,"-lm / --formats"             ,"List available output formats");
		sb.ND(1,"--"                          ,"Pass all remaining options to the script");

		var show = WhichShow;
		if (show.HasFlag(PickShow.FullHelp))
		{
			foreach(var fname in register.GetAllFunctions()) {
				var func = register.GetFunction(fname);
				sb.WT();
				sb.WT(0,$"Function '{fname}'");
				func.Usage(sb);
			}
		}
	}

	public static bool ParseArgs(string[] args, IRegister register)
	{
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
		if (p.Has("-h","--help").IsGood()) {
			WhichShow |= PickShow.FullHelp;
			return false;
		}

		//engine selection needs to happen before other engine specific options
		var oeng = p.Default(new[]{"-e","--engine"}, out string eng);
		if (oeng.IsInvalid()) {
			Tell.MissingArgument("--engine");
			return false;
		}
		else if (oeng.IsGood()) {
			if (!register.TryGetEngine(eng, out Engine)) {
				Tell.NotRegistered("Engine",eng);
				return false;
			}
		}

		if (p.Has("-lc","--colors").IsGood()) {
			WhichShow |= PickShow.ColorList;
		}
		if (p.Has("-lm","--formats").IsGood()) {
			WhichShow |= PickShow.Formats;
		}
		if (p.Has("-le","--engines").IsGood()) {
			WhichShow |= PickShow.Engines;
		}
		if (p.Has("-lf","--functions").IsGood()) {
			WhichShow |= PickShow.Functions;
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

		var ofmt = p.Default(new[]{"-f","--format"},out string fmt);
		if (ofmt.IsInvalid()) {
			return false;
		}
		else if (ofmt.IsGood()) {
			ImageFormat = fmt;
		}

		if (p.Has("-q","--quiet").IsGood()) {
			SuppressInfo = true;
		}

		//take the first remaining option as the script name
		// all other options must be accounted for at this point
		p.Default(out string FunctionName).IsGood();

		//return false if any of the show parameters were included so that
		// usage will get triggered
		return WhichShow == PickShow.None;
	}

	//static void ShowFunctionHelp(

	public static IImageEngine Engine;
	public static int? MaxDegreeOfParallelism;
	public static string ImageFormat;
	public static string FunctionName;
	public static string[] FunctionArgs;
	public static bool SuppressInfo = false;

	static PickShow WhichShow = PickShow.None;

	[Flags]
	public enum PickShow
	{
		None      = 0,
		FullHelp  = 1,
		ColorList = 2,
		Formats   = 4,
		Engines   = 8,
		Functions = 16
	}
}