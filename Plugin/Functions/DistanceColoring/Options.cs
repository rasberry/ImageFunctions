using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.DistanceColoring;

public sealed class Options : IOptions
{
	public string SomeOption;
	readonly ICoreLog Log;

	public Options(IFunctionContext context)
	{
		if(context == null) { throw Squeal.ArgumentNull(nameof(context)); }
		Log = context.Log;
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1, "Does something interesting");
		sb.ND(1, "-myopt (number)", "describe myopt here");
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		//use ParseNumberPercent for parsing numbers like 0.5 or 50%
		//var parser = new ParseParams.Parser<double>((string n) => {
		//	return ExtraParsers.ParseNumberPercent(n);
		//});

		if(p.Scan<string>("-myopt", "default")
			.WhenGoodOrMissing(r => { SomeOption = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		//TODO parse any other options and maybe do checks

		return true;
	}
}
