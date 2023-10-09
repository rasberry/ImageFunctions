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

	public void Usage(StringBuilder sb)
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

		var parser = new ParseParams.Parser<double?>((string s, out double? p) => {
			return ExtraParsers.TryParseNumberPercent(s,out p);
		});

		var pcx = p.Default("-cx",out int cx,out int cy);
		if (pcx.IsInvalid()) {
			return false;
		}
		else if (pcx.IsGood()) {
			CenterPx = new Point(cx,cy);
		}

		var pcp = p.Default("-cp",out double? ppx,out double? ppy,
			leftPar: parser, rightPar: parser
		);
		if (pcp.IsInvalid()) {
			return false;
		}
		else if (pcp.IsGood()) {
			CenterPp = new PointF((float)ppx,(float)ppy);
		}
		//-cx and -cp are either/or options so set a default if neither are specified
		if (CenterPx == null && CenterPp == null) {
			CenterPp = new PointF(0.5f,0.5f);
		}

		if (p.Default("-rx",out RadiusPx).IsInvalid()) {
			return false;
		}
		if (p.Default("-rp",out RadiusPp,par:parser).IsInvalid()) {
			return false;
		}
		//-rx and -rp are either/or options so set a default if neither are specified
		if (RadiusPx == null && RadiusPp == null) {
			RadiusPp = 0.9;
		}

		if (p.Default("-s",out Rotations,0.9).IsInvalid()) {
			return false;
		}
		if (p.Has("-ccw").IsGood()) {
			CounterClockwise = true;
		}
		if (p.DefaultSampler(register, out Sampler).IsInvalid()) {
			return false;
		}
		if (p.DefaultMetric(register, out Metric).IsInvalid()) {
			return false;
		}

		return true;
	}
}