using System.Drawing;
using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.FloodFill;

public sealed class Options : IOptions
{
	public FillMethodKind FillType;
	public PixelMapKind MapType;
	public bool MapSecondLayer;
	public double Similarity;
	public List<Point> StartPoints;
	public Lazy<IMetric> Metric;
	public ColorRGBA FillColor;
	public ColorRGBA? ReplaceColor;

	internal Random Rnd = new();

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1,"Fills area(s) of color with another color");
		sb.ND(1,"-c (color)"    ,"Fill color (default white)");
		sb.ND(1,"-p (x,y)"      ,"Pick starting coordinate (can be specified multiple times)");
		sb.ND(1,"-i"            ,"Replace pixels with ones taken the second layer");
		sb.ND(1,"-r (color)"    ,"Treat all pixels of given color as starting points");
		sb.ND(1,"-s (number[%])","How similar pixels should be to match range: [-1.0,1.0] (default 100%)");
		sb.ND(1,"-f (type)"     ,$"Use specified fill method (default {nameof(FillMethodKind.BreadthFirst)})");
		sb.ND(1,"-m (type)"     ,$"Use specified mapping method (default {nameof(PixelMapKind.Horizontal)})");
		sb.MetricHelpLine();
		sb.WT();
		sb.WT(1,"Fill Type:");
		sb.PrintEnum<FillMethodKind>(1);
		sb.WT();
		sb.WT(1,"Map Type:");
		sb.PrintEnum<PixelMapKind>(1);
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parserNum = new ParseParams.Parser<double>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});
		var parseColor = new ParseParams.Parser<ColorRGBA>((string n) => {
			var c = ExtraParsers.ParseColor(n);
			return ColorRGBA.FromRGBA255(c.R, c.G, c.B, c.A);
		});

		if (p.Scan("-c", PlugColors.White, parseColor)
			.WhenGoodOrMissing(r => { FillColor = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan("-r", par:parseColor)
			.WhenGood(r => { FillColor = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan("-f", FillMethodKind.BreadthFirst)
			.WhenGoodOrMissing(r => { FillType = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan("-m", PixelMapKind.Horizontal)
			.WhenGoodOrMissing(r => { MapType = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Has("-i").IsGood()) { MapSecondLayer = true; }

		if (p.Scan("-s", 1.0, parserNum)
			.WhenGoodOrMissing(r => { Similarity = r.Value; return r; })
			.WhenInvalidTellDefault()
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

		bool done = false;
		do {
			if (p.Scan("-p", Point.Empty, PlugTools.ParsePoint)
				.WhenMissing(r => { done = true; return r; })
				.WhenGood(r => {
					StartPoints ??= new();
					StartPoints.Add(r.Value);
					return r;
				})
				.WhenInvalidTellDefault()
				.IsInvalid()
			) {
				return false;
			};
		} while(!done);

		if (Similarity < -1.0 || Similarity > 1.0) {
			Log.Error(Note.MustBeBetween("Similarity","-1.0 (-100%)","1.0 (100%)"));
			return false;
		}

		return true;
	}

	static char[] Delimiters = new char[] { ',',' ','x',';' };
}

/*
fill method
	depth first - stack
	breadth first - queue
take pixels from another image
	mask method - coordinate of filled pixel from other image
	corner method - top right corner in - maybe other corners ?
	line by line - top to bottom / left to right
	spiral - out to in / in to out
	random
match rule
	exact match
	% similarity
start location
	point
	all matching
non exact match process
	copy lightness
	copy hue
	copy saturation
	invert copy (-distance)
	replace
	apply copy to alpha
match distance
	= rgb component distance
	= chose a component
	= different color space ?
*/
