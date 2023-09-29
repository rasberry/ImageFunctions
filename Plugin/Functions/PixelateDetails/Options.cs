using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.PixelateDetails;

public sealed class Options : IOptions
{
	public static bool UseProportionalSplit = false;
	public static double ImageSplitFactor = 2.0;
	public static double DescentFactor = 0.5;

	public static void Usage(StringBuilder sb)
	{
		sb.ND(1,"Creates areas of flat color by recursively splitting high detail chunks");
		sb.ND(1,"-p"            ,"Use proportianally sized sections (default is square sized sections)");
		sb.ND(1,"-s (number)[%]","Multiple or percent of image dimension used for splitting (default 2.0)");
		sb.ND(1,"-r (number)[%]","Count or percent or sections to re-split (default 50%)");
	}

	public static bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if (p.Has("-p").IsGood()) {
			UseProportionalSplit = true;
		}
		if (p.Default("-s",out ImageSplitFactor,2.0)
			.BeGreaterThanZero("-s",ImageSplitFactor).IsInvalid()) {
			return false;
		}
		if(p.Default("-r",out DescentFactor,0.5)
			.BeGreaterThanZero("-r",DescentFactor).IsInvalid()) {
			return false;
		}

		return true;
	}
}