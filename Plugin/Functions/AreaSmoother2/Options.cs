using ImageFunctions.Core;
using ImageFunctions.Core.Docs;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.AreaSmoother2;

public sealed class Options : IOptions
{
	public bool HOnly = false;
	public bool VOnly = false;

	public bool ParseArgs(string[] args, IRegister register)
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

	public void Usage(StringBuilder sb)
	{
		sb.ND(1,"Blends adjacent areas of flat color together by blending horizontal and vertical gradients");
		sb.ND(1,"-H","Horizontal only");
		sb.ND(1,"-V","Vertical only");
	}
}