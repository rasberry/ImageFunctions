using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.ProbableImg;

public sealed class Options : IOptions, IUsageProvider
{
	public int? RandomSeed = null;
	public int? TotalNodes = null;
	public List<StartPoint> StartLoc = new List<StartPoint>();
	public bool UseNonLookup = false;
	readonly ICoreLog Log;

	public Options(IFunctionContext context)
	{
		if(context == null) { throw Squeal.ArgumentNull(nameof(context)); }
		Log = context.Log;
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, "Generate a new image using a probability profile based on the input image"),
			Parameters = [
				new UsageOne<int>(1, "-n", "Max Number of start nodes (defaults to 1 or number of -pp/-xy options)") { Min = 1, Default = 1, Max = 999 },
				new UsageOne<int>(1, "-rs", "Options number seed for the random number generator") { TypeText = "seed" },
				new UsageOne<Point>(1, "-xy", "Add a start node (in pixels) - multiple allowed"),
				new UsageOne<PointF>(1, "-pp", "Add a start node (by proportion) - multiple allowed") { IsNumberPct = true },
				new UsageOne<bool>(1, "-alt", "Use alternate rendering uses slightly less memory"),
			],
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if(p.Scan<int>("-rs")
			.WhenGood(r => { RandomSeed = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<int>("-n")
			.WhenGood(r => { TotalNodes = r.Value; return r; })
			.BeGreaterThan(Log, 1, true)
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Has("-alt").IsGood()) {
			UseNonLookup = true;
		}

		var parser = new ParseParams.Parser<double>((string s) => {
			return ExtraParsers.ParseNumberPercent(s);
		});

		while(true) {
			var pcp = p.Scan<double, double>("-pp", leftPar: parser, rightPar: parser);
			if(pcp.IsMissing()) { break; }
			if(pcp.IsInvalid()) { return false; }
			if(pcp.IsGood()) {
				var (ppx, ppy) = pcp.Value;
				StartLoc.Add(StartPoint.FromPro(ppx, ppy));
			}
		}
		while(true) {
			var pcx = p.Scan<int, int>("-xy");
			if(pcx.IsMissing()) { break; }
			if(pcx.IsInvalid()) { return false; }
			if(pcx.IsGood()) {
				var (cx, cy) = pcx.Value;
				StartLoc.Add(StartPoint.FromLinear(cx, cy));
			}
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

	public static StartPoint FromLinear(int x, int y)
	{
		return new StartPoint {
			IsLinear = true,
			PX = 0.0,
			PY = 0.0,
			LX = x,
			LY = y
		};
	}
	public static StartPoint FromPro(double x, double y)
	{
		return new StartPoint {
			IsLinear = false,
			PX = x,
			PY = y,
			LX = 0,
			LY = 0
		};
	}
}
