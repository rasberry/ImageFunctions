using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.PixelateDetails;

public sealed class Options : IOptions
{
	public bool UseProportionalSplit = false;
	public double ImageSplitFactor = 2.0;
	public double DescentFactor = 0.5;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1, "Creates areas of flat color by recursively splitting high detail chunks");
		sb.ND(1, "-p", "Use proportianally sized sections (default is square sized sections)");
		sb.ND(1, "-s (number)[%]", "Multiple or percent of image dimension used for splitting (default 2.0)");
		sb.ND(1, "-r (number)[%]", "Count or percent or sections to re-split (default 50%)");
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if(p.Has("-p").IsGood()) {
			UseProportionalSplit = true;
		}

		if(p.Scan("-s", 2.0)
			.WhenGoodOrMissing(r => { ImageSplitFactor = r.Value; return r; })
			.WhenInvalidTellDefault()
			.BeGreaterThanZero()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-r", 0.5)
			.WhenGoodOrMissing(r => { DescentFactor = r.Value; return r; })
			.WhenInvalidTellDefault()
			.BeGreaterThanZero()
			.IsInvalid()
		) {
			return false;
		}

		return true;
	}
}
