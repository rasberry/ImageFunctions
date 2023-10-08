using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.ProbableImg;

public sealed class Options : IOptions
{
	public int? RandomSeed = null;
	public int? TotalNodes = null;
	public List<StartPoint> StartLoc = new List<StartPoint>();

	public void Usage(StringBuilder sb)
	{
		sb.ND(1,"Generate a new image using a probability profile based on the input image");
		sb.ND(1,"-n (number)"                ,"Max Number of start nodes (defaults to 1 or number of -pp/-xy options)");
		sb.ND(1,"-rs (seed)"                 ,"Options number seed for the random number generator");
		sb.ND(1,"-xy (number) (number)"      ,"Add a start node (in pixels) - multiple allowed");
		sb.ND(1,"-pp (number)[%] (number)[%]","Add a start node (by proportion) - multiple allowed");
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if (p.Default("-rs",out RandomSeed,null).IsInvalid()) {
			return false;
		}
		if (p.Default("-n",out TotalNodes,null).IsInvalid()) {
			return false;
		}

		var parser = new ParseParams.Parser<double>((string s, out double p) => {
			return ExtraParsers.TryParseNumberPercent(s, out p);
		});

		while(true) {
			var pcp = p.Default("-pp",out double ppx, out double ppy,
				lefthPar:parser,
				rightPar:parser
			);
			if (pcp.IsMissing()) { break; }
			if (pcp.IsInvalid()) {
				return false;
			}
			else if(pcp.IsGood()) {
				StartLoc.Add(StartPoint.FromPro(ppx,ppy));
			}
		}
		while(true) {
			var pcx = p.Default("-xy",out int cx, out int cy);
			if (pcx.IsMissing()) { break; }
			if (pcx.IsInvalid()) {
				return false;
			}
			else if (pcx.IsGood()) {
				StartLoc.Add(StartPoint.FromLinear(cx,cy));
			}
		}

		if (TotalNodes != null && TotalNodes < 1) {
			Tell.MustBeGreaterThanZero("-n");
			return false;
		}

		return true;
	}
}

	public struct StartPoint
	{
		public bool IsLinear;
		//proportional
		public double PX;
		public double PY;
		//linear
		public int LX;
		public int LY;

		public static StartPoint FromLinear(int x,int y) {
			return new StartPoint {
				IsLinear = true,
				PX = 0.0, PY = 0.0,
				LX = x, LY = y
			};
		}
		public static StartPoint FromPro(double x, double y) {
			return new StartPoint {
				IsLinear = false,
				PX = x, PY = y,
				LX = 0, LY = 0
			};
		}
	}