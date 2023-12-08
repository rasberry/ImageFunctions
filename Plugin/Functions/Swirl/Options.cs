using System.Drawing;
using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Swirl;

public sealed class Options : IOptions
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
		sb.ND(1,"Smears pixels in a circle around a point");
		sb.ND(1,"-cx (number) (number)"      ,"Swirl center X and Y coordinate in pixels");
		sb.ND(1,"-cp (number)[%] (number)[%]","Swirl center X and Y coordinate proportionally (default 50%,50%)");
		sb.ND(1,"-rx (number)"               ,"Swirl radius in pixels");
		sb.ND(1,"-rp (number)[%]"            ,"Swirl radius proportional to smallest image dimension (default 90%)");
		sb.ND(1,"-s  (number)[%]"            ,"Number of rotations (default 0.9)");
		sb.ND(1,"-ccw"                       ,"Rotate Counter-clockwise. (default is clockwise)");
		sb.SamplerHelpLine();
		sb.MetricHelpLine();
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		var parser = new ParseParams.Parser<double>((string s) => {
			return ExtraParsers.ParseNumberPercent(s);
		});

		if (p.Scan<int,int>("-cx")
			.WhenGood(r => {
				var (cx,cy) = r.Value;
				CenterPx = new Point(cx,cy);
				return r;
			})
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan<double,double>("-cp", leftPar: parser, rightPar: parser)
			.WhenGood(r => {
				var (ppx,ppy) = r.Value;
				CenterPp = new PointF((float)ppx,(float)ppy);
				return r;
			})
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		//-cx and -cp are either/or options so set a default if neither are specified
		if (CenterPx == null && CenterPp == null) {
			CenterPp = new PointF(0.5f,0.5f);
		}

		if (p.Scan<int>("-rx")
			.WhenGood(r => { RadiusPx = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan<double>("-rp", par: parser)
			.WhenGood(r => { RadiusPp = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		//-rx and -rp are either/or options so set a default if neither are specified
		if (RadiusPx == null && RadiusPp == null) {
			RadiusPp = 0.9;
		}

		if (p.Scan("-s", 0.9)
			.WhenGoodOrMissing(r => { Rotations = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Has("-ccw").IsGood()) {
			CounterClockwise = true;
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
}