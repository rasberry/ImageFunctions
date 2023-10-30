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

	public void Usage(StringBuilder sb)
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
		var parser = new ParseParams.Parser<double>((string n, out double p) => {
			return ExtraParsers.TryParseNumberPercent(n, out p);
		});

		var pcp = p.Default("-cp",
			out double ppx, out double ppy, //results
			0.5, 0.5,                       //defaults
			null,                           //condition
			parser, parser                  //custom parser
		);
		if (pcp.IsInvalid()) {
			Tell.CouldNotParse("-cp");
			return false;
		}
		else if(pcp.IsGood()) {
			CenterPp = new PointF((float)ppx,(float)ppy);
		}

		var pcx = p.Default("-cx", out int cx, out int cy);
		if (pcx.IsInvalid()) {
			Tell.CouldNotParse("-cx");
			return false;
		}
		else if (pcx.IsGood()) {
			CenterPx = new Point(cx,cy);
		}

		// -cp and -cx are either/or options so choose a default if neither were specified
		if (CenterPx == null && CenterPp == null) {
			CenterPp = new PointF(0.5f,0.5f);
		}

		if (p.Default("-e", out Power, 2.0).IsInvalid()) {
			Tell.CouldNotParse("-e");
			return false;
		}
		if (p.Default("-m", out WhichMode, Mode.Polynomial).IsInvalid()) {
			Tell.CouldNotParse("-m");
			return false;
		}
		if (p.DefaultSampler(register, out Sampler).IsInvalid()) {
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
