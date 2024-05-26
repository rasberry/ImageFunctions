using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.ImgDiff;

public sealed class Options : IOptions
{
	public double? HilightOpacity;
	public bool MatchSamePixels;
	public bool OutputOriginal;
	public bool MakeThirdLayer;
	public ColorRGBA HilightColor;
	public string MetricName;
	internal Lazy<IMetric> MetricInstance;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1, "Highlights differences between two images.");
		sb.ND(1, "By default differences are highlighted based on distance ranging from highlight color to white");
		sb.ND(1, "-o (number)[%]", "Overlay highlight color at given opacity");
		sb.ND(1, "-i", "Match identical pixels instead of differences");
		sb.ND(1, "-x", "Output original pixels instead of highlighting them");
		sb.ND(1, "-c (color)", "Change highlight color (default is magenta)");
		sb.ND(1, "-m (metric)", "Use another (registered) distance metric (default Euclidean)");
		sb.ND(1, "-nl", "Create a third layer instead of replacing two with one");
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string s) => {
			return ExtraParsers.ParseNumberPercent(s);
		});

		var colorParser = new ParseParams.Parser<ColorRGBA>(PlugTools.ParseColor);

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
			.WhenInvalidTellDefault()
			.BeGreaterThanZero(true)
			.IsInvalid()
		) {
			return false;
		}
		if(p.Scan<ColorRGBA>("-c", PlugColors.Magenta, colorParser)
			.WhenGoodOrMissing(r => { HilightColor = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}
		if(p.Scan("-m", "Euclidean")
			.WhenGoodOrMissing(r => { MetricName = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		var mr = new MetricRegister(register);
		if(!mr.Try(MetricName, out var mEntry)) {
			Log.Error(Note.NotRegistered(mr.Namespace, MetricName));
			return false;
		}
		MetricInstance = mEntry.Item;

		return true;
	}
}
