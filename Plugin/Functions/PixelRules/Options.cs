using ImageFunctions.Core;
using ImageFunctions.Core.Samplers;
using ImageFunctions.Core.Metrics;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.PixelRules;

public sealed class Options : IOptions
{
	public Mode WhichMode = Mode.StairCaseDescend;
	public int Passes = 1;
	public int MaxIters = 100;
	public Lazy<IMetric> Metric;
	public Lazy<ISampler> Sampler;

	public void Usage(StringBuilder sb)
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

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if (p.Scan("-n", 1)
			.WhenGoodOrMissing(r => { Passes = r.Value; return r; })
			.WhenInvalidTellDefault()
			.BeGreaterThanZero()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan("-m", Mode.StairCaseDescend)
			.WhenGoodOrMissing(r => { WhichMode = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan("-x", 100)
			.WhenGoodOrMissing(r => { MaxIters = r.Value; return r; })
			.WhenInvalidTellDefault()
			.BeGreaterThanZero()
			.IsInvalid()
		) {
			return false;
		}

		if (p.DefaultSampler(register)
			.WhenGood(r => { Sampler = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		if (p.DefaultMetric(register)
			.WhenGood(r => { Metric = r.Value; return r; })
			.IsInvalid()
		) {
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
