using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.ZoomBlur;

public class Options : IOptions, IUsageProvider
{
	public Lazy<ISampler> Sampler;
	public Lazy<IMetric> Measurer;
	public Point? CenterPx;
	public PointF? CenterRt;
	public double ZoomAmount;
	readonly ICoreLog Log;

	public Options(IFunctionContext context)
	{
		if (context == null) { throw Squeal.ArgumentNull(nameof(context)); }
		Log = context.Log;
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, "Blends rays of pixels to produce a 'zoom' effect"),
			Parameters = [
				new UsageOne<double>(1, "-z", "Zoom amount (default 1.1)") { Max = 200.0, Default = 1.1, IsNumberPct = true },
				new UsageOne<Point>(1, "-cx", "Coordinates of zoom center in pixels"),
				new UsageOne<PointF>(1, "-cp", "Coordinates of zoom center by proportion (default 50% 50%)") { Default = 0.5, IsNumberPct = true },
				//new UsageOne<>(" -oh", "Only zoom horizontally");
				//new UsageOne<>(" -ov", "Only zoom vertically");
				SamplerHelpers.SamplerUsageParameter(),
				MetricHelpers.MetricUsageParameter()
			]
		};
		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});

		if(p.Scan("-z", 1.1)
			.WhenGoodOrMissing(r => { ZoomAmount = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.BeGreaterThanZero(Log, true)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<PointF>("-cp", par: Core.Aides.OptionsAide.ParsePoint<PointF>)
			.WhenGood(r => { CenterRt = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<Point>("-cx", par: Core.Aides.OptionsAide.ParsePoint<Point>)
			.WhenGood(r => { CenterPx = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		//-cx / -cp are either/or options. if neither are specified set the default
		if(CenterPx == null && CenterRt == null) {
			CenterRt = new PointF(0.5f, 0.5f);
		}

		if(p.ScanSampler(Log, register)
			.WhenGood(r => { Sampler = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}
		if(p.ScanMetric(Log, register)
			.WhenGood(r => { Measurer = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		return true;
	}
}
