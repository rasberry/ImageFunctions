using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.ImgDiff;

public sealed class Options : IOptions
{
	public double? HilightOpacity;
	public bool MatchSamePixels;
	public bool OutputOriginal;
	public ColorRGBA HilightColor;
	public string MetricName;
	internal Lazy<IMetric> MetricInstance;

	public void Usage(StringBuilder sb)
	{
		sb.ND(1,"Highlights differences between two images.");
		sb.ND(1,"By default differences are highlighted based on distance ranging from highlight color to white");
		sb.ND(1,"-o (number)[%]","Overlay highlight color at given opacity");
		sb.ND(1,"-i"            ,"Match identical pixels instead of differences");
		sb.ND(1,"-x"            ,"Output original pixels instead of highlighting them");
		sb.ND(1,"-c (color)"    ,"Change highlight color (default is magenta)");
		sb.ND(1,"-m (metric)"   ,"Use another (registered) distance metric (default Euclidean)");
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double?>((string s, out double? p) => {
			return ExtraParsers.TryParseNumberPercent(s,out p);
		});

		if (p.Has("-i").IsGood()) {
			MatchSamePixels = true;
		}
		if (p.Has("-x").IsGood()) {
			OutputOriginal = true;
		}

		if(p.Default("-o",out HilightOpacity, par: parser)
			.BeGreaterThanZero("-o",HilightOpacity,true).IsInvalid()) {
			return false;
		}

		if (p.Default("-c",out HilightColor, PlugColors.Magenta).IsInvalid()) {
			return false;
		}

		if (p.Default("-m", out MetricName, "Euclidean").IsInvalid()) {
			return false;
		}

		var mr = new MetricRegister(register);
		if (!mr.Try(MetricName, out var mEntry)) {
			Tell.NotRegistered(mr.Namespace,MetricName);
			return false;
		}
		MetricInstance = mEntry.Item;

		return true;
	}
}