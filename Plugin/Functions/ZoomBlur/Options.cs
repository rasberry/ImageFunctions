using System.Drawing;
using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Core.Samplers;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.ZoomBlur;

public class Options : IOptions
{
	public Lazy<ISampler> Sampler;
	public Lazy<IMetric> Measurer;
	public Point? CenterPx;
	public PointF? CenterRt;
	public double ZoomAmount;

	public void Usage(StringBuilder sb)
	{
		sb.ND(1,"Blends rays of pixels to produce a 'zoom' effect");
		sb.ND(1,"-z  (number)[%]"             ,"Zoom amount (default 1.1)");
		sb.ND(1,"-cc (number) (number)"       ,"Coordinates of zoom center in pixels");
		sb.ND(1,"-cp (number)[%] (number)[%]" ,"Coordinates of zoom center by proportion (default 50% 50%)");
		//sb.WL(" -oh"                        ,"Only zoom horizontally");
		//sb.WL(" -ov"                        ,"Only zoom vertically");
		sb.SamplerHelpLine();
		sb.MetricHelpLine();
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string n, out double p) => {
			return ExtraParsers.TryParseNumberPercent(n, out p);
		});

		if (p.Default("-z",out ZoomAmount,1.1)
			.BeGreaterThanZero("-z",ZoomAmount,true).IsInvalid()) {
			return false;
		}
		var pcc = p.Default("-cc",out int cx, out int cy);
		if (pcc.IsInvalid()) {
			return false;
		}
		else if (pcc.IsGood()) {
			CenterPx = new Point(cx,cy);
		}
		var pcp = p.Default("-cp",out double px, out double py,
			leftPar: parser, rightPar: parser
		);
		if (pcp.IsInvalid()) {
			return false;
		}
		else if (pcp.IsGood()) {
			CenterRt = new PointF((float)px,(float)py);
		}

		//-cc / -cp are either/or options. if neither are specified set the default
		if (CenterPx == null && CenterRt == null) {
			CenterRt = new PointF(0.5f,0.5f);
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