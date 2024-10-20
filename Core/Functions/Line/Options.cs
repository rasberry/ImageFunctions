using ImageFunctions.Core.Aides;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Core.Functions.Line;

public sealed class Options : IOptions, IUsageProvider
{
	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, "Draws a line or sequence of lines"),
			Parameters = [
				new UsageOne<LineKind>(1, "-m", "Method used to draw the line") { Default = LineKind.RunLengthSlice, Name = "Type" },
				//new UsageOne<double>(1, "-w", "Line width in pixels. Partial pixels are ok (defaults to 1.0)") { Default = 1.0, Max = 1024.0, Min = 0.0 },
				new UsageOne<Point>(1, "-p", "Specify a point. Can be specified multiple times" ),
				new UsageOne<ColorRGBA>(1, "-c", "Color for the line (default black)"),
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
			LineKind.RunLengthSlice => "Run-length slice algorithm",
			LineKind.Bresenham => "Bresenham line algorithm",
			LineKind.DDA => "Digital Differential Analyzer method",
			LineKind.XiaolinWu => "Xiaolin Wu Anti-Aliased line algorithm",
			LineKind.WuBlackBook => "Wu's algorithm from Graphics Programming Black Book",
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

		if (p.Scan<LineKind>("-m", LineKind.RunLengthSlice)
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
		RunLengthSlice = 0,
		Bresenham = 1,
		DDA = 2,
		XiaolinWu = 3,
		WuBlackBook = 4,
	}

	internal ColorRGBA Color;
	//internal double Width;
	internal List<Point> PointList;
	internal LineKind Kind;
}