using System.Drawing;
using ImageFunctions.Core;
using ImageFunctions.Core.Samplers;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Deform;

public sealed class Options : IOptions
{
	public Point? CenterPx;
	public PointF? CenterPp;
	public Mode WhichMode;
	public double Power;
	public Lazy<ISampler> Sampler;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1,"Warps an image using a mapping function");
		sb.ND(1,"-cx (number) (number)"      ,"Coordinates of center in pixels");
		sb.ND(1,"-cp (number)[%] (number)[%]","Coordinates of center by proportion (default 50% 50%)");
		sb.ND(1,"-e (number)"                ,"(e) Power Exponent (default 2.0)");
		sb.ND(1,"-m (mode)"                  ,"Choose mode (default Polynomial)");
		sb.SamplerHelpLine();
		sb.WT();
		sb.ND(1,"Available Modes");
		sb.ND(1,"1. Polynomial","x^e/w, y^e/h");
		sb.ND(1,"2. Inverted"  ,"n/x, n/y; n = (x^e + y^e)");
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});

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

		// -cp and -cx are either/or options so choose a default if neither were specified
		if (CenterPx == null && CenterPp == null) {
			CenterPp = new PointF(0.5f,0.5f);
		}

		if (p.Scan("-e", 2.0)
			.WhenGoodOrMissing(r => { Power = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan("-m", Mode.Polynomial)
			.WhenGoodOrMissing(r => { WhichMode = r.Value; return r; })
			.WhenInvalidTellDefault()
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

		return true;
	}

	public enum Mode {
		None = 0,
		Polynomial = 1,
		Inverted = 2
	}
}
