using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Metrics;
using Rasberry.Cli;
using System.Drawing;
using CoreColors = ImageFunctions.Core.Aides.ColorAide;

namespace ImageFunctions.Plugin.Functions.FloodFill;

public sealed class Options : IOptions, IUsageProvider
{
	public FillMethodKind FillType;
	public PixelMapKind MapType;
	public bool MapSecondLayer;
	public double Similarity;
	public List<Point> StartPoints;
	public Lazy<IMetric> Metric;
	public ColorRGBA FillColor;
	public ColorRGBA? ReplaceColor;
	public bool MakeNewLayer;

	internal Random Rnd = new();

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, "Fills area(s) of color with another color"),
			Parameters = [
				new UsageOne<ColorRGBA>(1, "-c", "Fill color (default white)") { Default = CoreColors.White },
				new UsageOne<Point>(1, "-p", "Pick starting coordinate (can be specified multiple times)"),
				new UsageOne<bool>(1, "-i", "Replace pixels with ones taken the second layer"),
				new UsageOne<ColorRGBA>(1, "-r", "Treat all pixels of given color as starting points"),
				new UsageOne<double>(1, "-s", "How similar pixels should be to match range: [0.0,1.0] (default 100%)") { Default = 1.0, IsNumberPct = true },
				new UsageOne<FillMethodKind>(1, "-f", $"Use specified fill method (default {nameof(FillMethodKind.BreadthFirst)})") { Default = FillMethodKind.BreadthFirst, TypeText = "Fill" },
				new UsageOne<PixelMapKind>(1, "-m", $"Use specified mapping method (default {nameof(PixelMapKind.Coordinate)})") { Default = PixelMapKind.Coordinate, TypeText = "Map" },
				new UsageOne<bool>(1, "-nl", "Create a new layer instead of replacing original(s)"),
				MetricHelpers.MetricUsageParameter()
			],
			EnumParameters = [
				new UsageEnum<FillMethodKind>(1, "Fill Type:"),
				new UsageEnum<PixelMapKind>(1, "Map Type:"),
			]
		};

		return u;
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

		if(p.Scan("-c", CoreColors.White, parseColor)
			.WhenGoodOrMissing(r => { FillColor = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-r", par: parseColor)
			.WhenGood(r => { FillColor = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-f", FillMethodKind.BreadthFirst)
			.WhenGoodOrMissing(r => { FillType = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-m", PixelMapKind.Coordinate)
			.WhenGoodOrMissing(r => { MapType = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Has("-i").IsGood()) { MapSecondLayer = true; }
		if(p.Has("-nl").IsGood()) { MakeNewLayer = true; }

		if(p.Scan("-s", 1.0, parserNum)
			.WhenGoodOrMissing(r => { Similarity = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.ScanMetric(register)
			.WhenGood(r => { Metric = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		bool done = false;
		do {
			if(p.Scan("-p", Point.Empty, Core.Aides.OptionsAide.ParsePoint<Point>)
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

		if(Similarity < 0.0 || Similarity > 1.0) {
			Log.Error(Note.MustBeBetween("Similarity", "-0.0 (0%)", "1.0 (100%)"));
			return false;
		}

		return true;
	}
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
