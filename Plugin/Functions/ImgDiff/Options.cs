using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;
using PlugColors = ImageFunctions.Plugin.Aides.ColorAide;

namespace ImageFunctions.Plugin.Functions.ImgDiff;

public sealed class Options : IOptions, IUsageProvider
{
	public double? HilightOpacity;
	public bool MatchSamePixels;
	public bool OutputOriginal;
	public bool MakeThirdLayer;
	public ColorRGBA HilightColor;
	public string MetricName;
	internal Lazy<IMetric> MetricInstance;
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
		var u = new Usage {
			Description = new UsageDescription(1,
				"Highlights differences between two images.",
				"By default differences are highlighted based on distance ranging from highlight color to white"
			),
			Parameters = [
				new UsageOne<double>(1, "-o", "Overlay highlight color at given opacity") { IsNumberPct = true },
				new UsageOne<bool>(1, "-i", "Match identical pixels instead of differences"),
				new UsageOne<bool>(1, "-x", "Output original pixels instead of highlighting them"),
				new UsageOne<ColorRGBA>(1, "-c", "Change highlight color (default is magenta)") { Default = PlugColors.Magenta },
				new UsageOne<bool>(1, "-nl", "Create a third layer instead of replacing two with one"),
				MetricHelpers.MetricUsageParameter()
			],
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string s) => {
			return ExtraParsers.ParseNumberPercent(s);
		});

		var colorParser = new ParseParams.Parser<ColorRGBA>(Core.Aides.OptionsAide.ParseColor);

		if(p.Has("-i").IsGood()) {
			MatchSamePixels = true;
		}
		if(p.Has("-x").IsGood()) {
			OutputOriginal = true;
		}
		if(p.Has("-nl").IsGood()) {
			MakeThirdLayer = true;
		}

		if(p.Scan<double>("-o", par: parser)
			.WhenGood(r => { HilightOpacity = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.BeGreaterThanZero(Log, true)
			.IsInvalid()
		) {
			return false;
		}
		if(p.Scan<ColorRGBA>("-c", PlugColors.Magenta, colorParser)
			.WhenGoodOrMissing(r => { HilightColor = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.ScanMetric(Log, register)
			.WhenGood(r => { MetricInstance = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		return true;
	}
}
