using ImageFunctions.Core;
using ImageFunctions.Core.Samplers;
using ImageFunctions.Core.Metrics;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.PixelRules;

public sealed class Options : IOptions
{
	public static Mode WhichMode = Mode.StairCaseDescend;
	public static int Passes = 1;
	public static int MaxIters = 100;
	public static Lazy<IMetric> Metric;
	public static Lazy<ISampler> Sampler;

	public static void Usage(StringBuilder sb)
	{
		sb.ND(1,"Average a set of pixels by following a minimaztion function");
		sb.ND(1,"-m (mode)"  ,"Which mode to use (default StairCaseDescend)");
		sb.ND(1,"-n (number)","Number of times to apply operation (default 1)");
		sb.ND(1,"-x (number)","Maximum number of iterations - in case of infinite loops (default 100)");
		sb.SamplerHelpLine();
		sb.MetricHelpLine();
		sb.WT();
		sb.ND(1,"Available Modes");
		sb.PrintEnum<Mode>(1,ModeDesc);
	}

	public static bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if (p.Default("-n",out Passes,1)
			.BeGreaterThanZero("-n",Passes).IsInvalid()) {
			return false;
		}
		if (p.Default("-m",out WhichMode,Mode.StairCaseDescend).IsInvalid()) {
			return false;
		}
		if (p.Default("-x",out MaxIters,100)
			.BeGreaterThanZero("-x",MaxIters).IsInvalid()) {
			return false;
		}
		if (p.DefaultSampler(register, out Sampler).IsInvalid()) {
			return false;
		}
		if (p.DefaultMetric(register, out Metric).IsInvalid()) {
			return false;
		}

		return true;
	}

	static string ModeDesc(Mode m)
	{
		switch(m)
		{
		case Mode.StairCaseDescend:  return "move towards smallest distance";
		case Mode.StairCaseAscend:   return "move towards largest distance";
		case Mode.StairCaseClosest:  return "move towards closest distance";
		case Mode.StairCaseFarthest: return "move towards farthest distance";
		}
		return "";
	}
}

public enum Mode {
	None = 0,
	StairCaseDescend = 1,
	StairCaseAscend = 2,
	StairCaseClosest = 3,
	StairCaseFarthest = 4
}
