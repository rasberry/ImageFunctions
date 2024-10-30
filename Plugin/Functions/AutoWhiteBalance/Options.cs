using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.AutoWhiteBalance;

public sealed class Options : IOptions, IUsageProvider
{
	public double DiscardRatio;
	public int BucketCount;
	public bool StretchAlpha;
	readonly ICoreLog Log;

	public Options(IFunctionContext context)
	{
		if(context == null) { throw Squeal.ArgumentNull(nameof(context)); }
		Log = context.Log;
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var desc = "Balances the image by stretching the color channels separately. The process"
			+ " 'trims' the color histogram removing sections of low color use at the top and bottom of the range"
			+ " then stretching the remaining colors to fill the space";

		var u = new Usage {
			Description = new UsageDescription(1, desc),
			Parameters = [
				new UsageOne<double>(1, "-p", "Threshold for discarding infrequently used colors (default 0.05%)") { Default = 0.0005, IsNumberPct = true },
				new UsageOne<int>(1, "-b", "Number of buckets to use for histogram (default 256)") { Default = 256, Min = 1, Max = 65536 },
				new UsageOne<bool>(1, "-a", "Also stretch alpha channel")
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});

		if(p.Scan("-p", 0.0005, parser)
			.WhenGoodOrMissing(r => { DiscardRatio = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.BeBetween(Log, 0.0, 1.0)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-b", 256)
			.WhenGoodOrMissing(r => { BucketCount = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.BeGreaterThan(Log, 1, true)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Has("-a").IsGood()) {
			StretchAlpha = true;
		}

		return true;
	}
}
