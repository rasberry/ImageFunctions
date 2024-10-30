#if false
using Rasberry.Cli;

namespace ImageFunctions.Core.Functions.SimpleAlter;

public sealed class Options : IOptions, IUsageProvider
{
	//TODO maybe these should be broken out into their own functions unless the same inputs can be used
	public enum OperationKind
	{
		None = 0,
		Rotate = 1, //rotate center, angle
		Scale = 2, //scale center, amount
		// Skew = 3, //skew direction, amount (updown, leftright, both
		// Crop = 4, //crop rectangle
		// Resize = 5, //resize rectangle
		// Flip = 6, //flip direction (horiz, vert)
		// Matrix = 7 //generic transformation matrix..
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, ""),
			Parameters = [
				//new UsageOne<int>(1, "-t", "Number of times to run fit function (default 7)") { Default = 7, Min = 1, Max = 99 },
				//new UsageOne<bool>(1, "-r", "Draw the gradient ratio as a grayscale image instead of modifying the original colors"),
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		//use ParseNumberPercent for parsing numbers like 0.5 or 50%
		//var parser = new ParseParams.Parser<double>((string n) => {
		//	return ExtraParsers.ParseNumberPercent(n);
		//});

		// if (p.Scan<string>("-myopt", "default")
		// 	.WhenGoodOrMissing(r => { SomeOption = r.Value; return r; })
		// 	.WhenInvalidTellDefault()
		// 	.IsInvalid()
		// ) {
		// 	return false;
		// }

		//TODO parse any other options and maybe do checks

		return true;
	}
}
#endif
