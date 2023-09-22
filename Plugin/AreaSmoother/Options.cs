using System.Reflection;
using System.Text;
using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.AreaSmoother;

public sealed class Options : IOptions
{
	public static int TotalTries = 7;
	public static Lazy<ISampler> Sampler;
	public static Lazy<IMetric> Measurer;

	public static void Usage(StringBuilder sb)
	{
		sb.ND(1,"Blends adjacent areas of flat color together by sampling the nearest two colors to the area");
		sb.ND(1,"-t (number)","Number of times to run fit function (default 7)");
		sb.SamplerHelpLine();
		sb.MetricHelpLine();
	}

	public static bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		if (p.Default("-t",out TotalTries,7)
			.BeGreaterThanZero("-t",TotalTries).IsInvalid()) {
			return false;
		}

		if (p.DefaultSampler(register, out Sampler).IsInvalid()) {
			return false;
		}
		if (p.DefaultMetric(register, out Measurer).IsInvalid()) {
			return false;
		}

		return true;
	}

}
