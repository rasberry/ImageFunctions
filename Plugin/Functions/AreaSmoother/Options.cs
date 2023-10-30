using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.AreaSmoother;

public sealed class Options : IOptions
{
	public int TotalTries;
	public bool DrawRatio;
	public Lazy<ISampler> Sampler;
	public Lazy<IMetric> Measurer;

	public void Usage(StringBuilder sb)
	{
		sb.ND(1,"Blends adjacent areas of flat color together by sampling the nearest two colors to the area");
		sb.ND(1,"-t (number)","Number of times to run fit function (default 7)");
		sb.ND(1,"-r"         ,"Draw the gradient ratio as a grayscale image instead of modifying the original colors");
		sb.SamplerHelpLine();
		sb.MetricHelpLine();
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		if (p.Default("-t",out TotalTries,7).IsInvalid()) {
			Tell.CouldNotParse("-t");
			return false;
		}

		if (p.Has("-r").IsGood()) {
			DrawRatio = true;
		}

		if (p.DefaultSampler(register, out Sampler).IsInvalid()) {
			return false;
		}
		if (p.DefaultMetric(register, out Measurer).IsInvalid()) {
			return false;
		}

		if (TotalTries < 1) {
			Tell.MustBeGreaterThanZero("-t");
			return false;
		}

		return true;
	}

}
