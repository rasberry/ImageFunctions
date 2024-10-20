using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.PixelateDetails;

public sealed class Options : IOptions, IUsageProvider
{
	public bool UseProportionalSplit = false;
	public double ImageSplitFactor = 2.0;
	public double DescentFactor = 0.5;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, "Creates areas of flat color by recursively splitting high detail chunks"),
			Parameters = [
				new UsageOne<bool>(1, "-p", "Use proportianally sized sections (default is square sized sections)"),
				new UsageOne<double>(1, "-s", "Multiple or percent of image dimension used for splitting (default 2.0)") { IsNumberPct = true, Max = 200.0 },
				new UsageOne<double>(1, "-r", "Count or percent of sections to re-split (default 50%)") { IsNumberPct = true, Max = 200.0 },
			],
		};

		return u;
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
