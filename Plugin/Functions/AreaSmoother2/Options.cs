using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.AreaSmoother2;

public sealed class Options : IOptions, IUsageProvider
{
	public bool HOnly = false;
	public bool VOnly = false;

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		if(p.Has("-H").IsGood()) {
			HOnly = true;
		}
		if(p.Has("-V").IsGood()) {
			VOnly = true;
		}

		return true;
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, "Blends adjacent areas of flat color together by blending horizontal and vertical gradients"),
			Parameters = [
				new UsageOne<bool>(1, "-H", "Horizontal only"),
				new UsageOne<bool>(1, "-V", "Vertical only")
			]
		};

		return u;
	}
}
