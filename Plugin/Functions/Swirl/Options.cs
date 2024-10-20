using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.Swirl;

public sealed class Options : IOptions, IUsageProvider
{
	public Point? CenterPx;
	public PointF? CenterPp;
	public int? RadiusPx;
	public double? RadiusPp;
	public double Rotations;
	public bool CounterClockwise;
	public Lazy<ISampler> Sampler;
	public Lazy<IMetric> Metric;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1,"Smears pixels in a circle around a point"),
			Parameters = [
				new UsageOne<Point>(1, "-cx", "Swirl center X and Y coordinate in pixels"),
				new UsageOne<PointF>(1, "-cp", "Swirl center X and Y coordinate proportionally (default 50%,50%)") { Default = 0.5, IsNumberPct = true },
				new UsageOne<int>(1, "-rx", "Swirl radius in pixels") { Min = 0, Max = 9999 },
				new UsageOne<double>(1, "-rp", "Swirl radius proportional to smallest image dimension (default 90%)") { Default = 0.9, IsNumberPct = true },
				new UsageOne<double>(1, "-s", "Number of rotations (default 0.9)") { Min = 0.01, Default = 0.9, Max = 99 },
				new UsageOne<bool>(1, "-ccw", "Rotate Counter-clockwise. (default is clockwise)"),
				SamplerHelpers.SamplerUsageParameter(),
				MetricHelpers.MetricUsageParameter(),
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		var parser = new ParseParams.Parser<double>((string s) => {
			return ExtraParsers.ParseNumberPercent(s);
		});

		if(p.Scan<PointF>("-cp", par: Core.Aides.OptionsAide.ParsePoint<PointF>)
			.WhenGood(r => { CenterPp = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<Point>("-cx", par: Core.Aides.OptionsAide.ParsePoint<Point>)
			.WhenGood(r => { CenterPx = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		//-cx and -cp are either/or options so set a default if neither are specified
		if(CenterPx == null && CenterPp == null) {
			CenterPp = new PointF(0.5f, 0.5f);
		}

		if(p.Scan<int>("-rx")
			.WhenGood(r => { RadiusPx = r.Value; return r; })
			.BeGreaterThanZero()
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<double>("-rp", par: parser)
			.WhenGood(r => { RadiusPp = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		//-rx and -rp are either/or options so set a default if neither are specified
		if(RadiusPx == null && RadiusPp == null) {
			RadiusPp = 0.9;
		}

		if(p.Scan("-s", 0.9)
			.WhenGoodOrMissing(r => { Rotations = r.Value; return r; })
			.BeGreaterThanZero()
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Has("-ccw").IsGood()) {
			CounterClockwise = true;
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
}
