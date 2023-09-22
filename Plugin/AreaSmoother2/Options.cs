using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.AreaSmoother2;

public sealed class Options : IOptions
{
	public static bool HOnly = false;
	public static bool VOnly = false;

	public static bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		if (p.Has("-H").IsGood()) {
			HOnly = true;
		}
		if (p.Has("-V").IsGood()) {
			VOnly = true;
		}

		return true;
	}

	public static void Usage(StringBuilder sb)
	{
		sb.ND(1,"Blends adjacent areas of flat color together by blending horizontal and vertical gradients");
		sb.ND(1,"-H","Horizontal only");
		sb.ND(1,"-V","Vertical only");
	}
}