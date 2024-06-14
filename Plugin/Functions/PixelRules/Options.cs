using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.PixelRules;

public sealed class Options : IOptions, IUsageProvider
{
	public Mode WhichMode = Mode.StairCaseDescend;
	public int Passes = 1;
	public int MaxIters = 100;
	public Lazy<IMetric> Metric;
	public Lazy<ISampler> Sampler;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1,"Average a set of pixels by following a minimaztion function"),
			Parameters = [
				new UsageOne<Mode>(1, "-m (mode)", "Which mode to use (default StairCaseDescend)"),
				new UsageOne<int>(1, "-n (number)", "Number of times to apply operation (default 1)"),
				new UsageOne<int>(1, "-x (number)", "Maximum number of iterations - in case of infinite loops (default 100)"),
				SamplerHelpers.SamplerUsageParameter(),
				MetricHelpers.MetricUsageParameter()
			],
			EnumParameters = [
				new UsageEnum<Mode>(1, "Available Modes") { DescriptionMap = ModeDesc }
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if(p.Scan("-n", 1)
			.WhenGoodOrMissing(r => { Passes = r.Value; return r; })
			.WhenInvalidTellDefault()
			.BeGreaterThanZero()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-m", Mode.StairCaseDescend)
			.WhenGoodOrMissing(r => { WhichMode = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-x", 100)
			.WhenGoodOrMissing(r => { MaxIters = r.Value; return r; })
			.WhenInvalidTellDefault()
			.BeGreaterThanZero()
			.IsInvalid()
		) {
			return false;
		}

		if(p.ScanSampler(register)
			.WhenGood(r => { Sampler = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		if(p.ScanMetric(register)
			.WhenGood(r => { Metric = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		return true;
	}

	static string ModeDesc(object m)
	{
		switch(m) {
		case Mode.StairCaseDescend: return "move towards smallest distance";
		case Mode.StairCaseAscend: return "move towards largest distance";
		case Mode.StairCaseClosest: return "move towards closest distance";
		case Mode.StairCaseFarthest: return "move towards farthest distance";
		}
		return "";
	}
}

public enum Mode
{
	None = 0,
	StairCaseDescend = 1,
	StairCaseAscend = 2,
	StairCaseClosest = 3,
	StairCaseFarthest = 4
}
