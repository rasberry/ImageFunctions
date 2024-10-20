using ImageFunctions.Core.Aides;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Core.Functions.Polygon;

public sealed class Options : IOptions, IUsageProvider
{
	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, "Draws a polygon"),
			Parameters = [
				new UsageOne<LineKind>(1, "-m", "Method used to draw the line") { Default = LineKind.DDA, Name = "Type" },
				new UsageOne<Point>(1, "-p", "Specify a point. Can be specified multiple times" ),
				new UsageOne<ColorRGBA>(1, "-c", "Color for the polygon (default black)"),
			],
			EnumParameters = [
				new UsageEnum<LineKind>(1, "Available methods:") { DescriptionMap = LineKindDescription }
			]
		};

		return u;
	}

	string LineKindDescription(object k)
	{
		return k switch {
			LineKind.DDA => "Digital Differential Analyzer",
			_ => "",
		};
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		//use ParseNumberPercent for parsing numbers like 0.5 or 50%
		//var parser = new ParseParams.Parser<double>((string n) => {
		//	return ExtraParsers.ParseNumberPercent(n);
		//});

		// if (p.Scan<double>("-w", 1.0)
		// 	.WhenGoodOrMissing(r => { Width = r.Value; return r; })
		// 	.WhenInvalidTellDefault()
		// 	.IsInvalid()
		// ) {
		// 	return false;
		// }

		if (p.Scan<LineKind>("-m", LineKind.Bresenham)
			.WhenGoodOrMissing(r => { Kind = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan<ColorRGBA>("-c", ColorAide.Black, OptionsAide.ParseColor)
			.WhenGoodOrMissing(r => { Color = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		bool done = false;
		do {
			if(p.Scan("-p", Point.Empty, OptionsAide.ParsePoint<Point>)
				.WhenMissing(r => { done = true; return r; })
				.WhenGood(r => {
					PointList ??= new();
					PointList.Add(r.Value);
					return r;
				})
				.WhenInvalidTellDefault()
				.IsInvalid()
			) {
				return false;
			};
		} while(!done);

		if (PointList == null || PointList.Count < 2) {
			Log.Error(Note.MissingArgument($"-p. drawing a line requires at least two points"));
			return false;
		}

		return true;
	}

	public enum LineKind
	{
		Bresenham = 0,
		DDA = 1,
		XiaolinWu = 2,
		RunLengthSlice = 3,
	}

	internal ColorRGBA Color;
	internal List<Point> PointList;
	internal LineKind Kind;
}