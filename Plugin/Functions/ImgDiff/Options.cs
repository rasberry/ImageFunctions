using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.ImgDiff;

public sealed class Options : IOptions
{
	public static double? HilightOpacity;
	public static bool MatchSamePixels;
	public static bool OutputOriginal;
	public static ColorRGBA HilightColor;
	public static string MetricName;
	internal static Lazy<IMetric> MetricInstance;

	public static void Usage(StringBuilder sb)
	{
		sb.ND(1,"Highlights differences between two images.");
		sb.ND(1,"By default differences are highlighted based on distance ranging from highlight color to white");
		sb.ND(1,"-o (number)[%]","Overlay highlight color at given opacity");
		sb.ND(1,"-i"            ,"Match identical pixels instead of differences");
		sb.ND(1,"-x"            ,"Output original pixels instead of highlighting them");
		sb.ND(1,"-c (color)"    ,"Change highlight color (default is magenta)");
		sb.ND(1,"-m (metric)"   ,"Use another (registered) distance metric (default Euclidean)");
	}

	public static bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if (p.Has("-i").IsGood()) {
			MatchSamePixels = true;
		}
		if (p.Has("-x").IsGood()) {
			OutputOriginal = true;
		}

		if(p.Default("-o",out HilightOpacity, par: PlugTools.ParseNumberPercent)
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
		if (!mr.Try(MetricName, out MetricInstance)) {
			Tell.NotRegistered(mr.Namespace,MetricName);
			return false;
		}

		return true;
	}
}