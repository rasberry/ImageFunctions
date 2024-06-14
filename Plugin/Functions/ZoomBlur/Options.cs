using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
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

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1,"Blends rays of pixels to produce a 'zoom' effect"),
			Parameters = [
				new UsageOne<double>(1, "-z  (number)[%]", "Zoom amount (default 1.1)"),
				new UsageTwo<int,int>(1, "-cc (number) (number)", "Coordinates of zoom center in pixels"),
				new UsageTwo<double,double>(1, "-cp (number)[%] (number)[%]", "Coordinates of zoom center by proportion (default 50% 50%)"),
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
			.WhenInvalidTellDefault()
			.BeGreaterThanZero(true)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<int, int>("-cc")
			.WhenGood(r => {
				var (cx, cy) = r.Value;
				CenterPx = new Point(cx, cy);
				return r;
			})
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<double, double>("-cp", leftPar: parser, rightPar: parser)
			.WhenGood(r => {
				var (px, py) = r.Value;
				CenterRt = new PointF((float)px, (float)py);
				return r;
			})
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		//-cc / -cp are either/or options. if neither are specified set the default
		if(CenterPx == null && CenterRt == null) {
			CenterRt = new PointF(0.5f, 0.5f);
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
