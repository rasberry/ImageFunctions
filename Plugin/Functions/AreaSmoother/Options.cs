using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.AreaSmoother;

public sealed class Options : IOptions, IUsageProvider
{
	public int TotalTries;
	public bool DrawRatio;
	public Lazy<ISampler> Sampler;
	public Lazy<IMetric> Measurer;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, "Blends adjacent areas of flat color together by sampling the nearest two colors to the area"),
			Parameters = [
				new UsageOne<int>(1, "-t (number)", "Number of times to run fit function (default 7)") { Default = 7, Min = 1, Max = 32 },
				new UsageOne<bool>(1, "-r", "Draw the gradient ratio as a grayscale image instead of modifying the original colors"),
				SamplerHelpers.SamplerUsageParameter(),
				MetricHelpers.MetricUsageParameter()
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if(p.Scan<int>("-t", 7)
			.WhenInvalidTellDefault()
			.WhenGoodOrMissing(r => {
				if(r.Value < 1) {
					Log.Error(Note.MustBeGreaterThan(r.Name, 0));
					return r with { Result = ParseParams.Result.UnParsable };
				}
				TotalTries = r.Value; return r;
			})
			.IsInvalid()
		) {
			return false;
		}

		if(p.Has("-r").IsGood()) {
			DrawRatio = true;
		}

		if(p.ScanSampler(register)
			.WhenGood(r => { Sampler = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		if(p.ScanMetric(register)
			.WhenGood(r => { Measurer = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		return true;
	}

}
