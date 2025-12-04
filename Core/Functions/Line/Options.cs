using ImageFunctions.Core.Aides;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Core.Functions.Line;

public sealed class Options : IOptions, IUsageProvider
{
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
			Description = new UsageDescription(1, "Draws a line or sequence of lines"),
			Parameters = [
				new UsageOne<LineKind>(1, "-m", "Method used to draw the line") { Default = LineKind.RunLengthSlice },
				//new UsageOne<double>(1, "-w", "Line width in pixels. Partial pixels are ok (defaults to 1.0)") { Default = 1.0, Max = 1024.0, Min = 0.0 },
				new UsageMany<Point>(1, "-p", "Specify a point. Can be specified multiple times") { AllowCount = int.MaxValue },
				new UsageMany<PointD>(1, "-pp", "Specify a proportional point (0.1,20%)") { AllowCount = int.MaxValue },
				new UsageOne<ColorRGBA>(1, "-c", "Color for the line (default black)"),
			],
			EnumParameters = [
				new UsageEnum<LineKind>(1, "Available methods:") { DescriptionMap = LineKindDescription }
			]
		};

		return u;
	}

	string LineKindDescription(LineKind k)
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

		// if (p.Scan<double>("-w", 1.0)
		// 	.WhenGoodOrMissing(r => { Width = r.Value; return r; })
		// 	.WhenInvalidTellDefault()
		// 	.IsInvalid()
		// ) {
		// 	return false;
		// }

		// need this to support multiple point types
		static object parsePointHandler(string name, string value)
		{
			if(name == "-p") {
				return OptionsAide.ParsePointSize<Point>(value);
			}
			else if(name == "-pp") {
				return OptionsAide.ParsePointSize(value,
					(a, b) => new PointD(a, b),
					s => ExtraParsers.ParseNumberPercent(s, null)
				);
			}
			throw Squeal.NotSupported(name);
		}

		if(p.Scan<LineKind>("-m", LineKind.RunLengthSlice)
			.WhenGoodOrMissing(r => { Kind = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<ColorRGBA>("-c", ColorAide.Black, OptionsAide.ParseColor)
			.WhenGoodOrMissing(r => { Color = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.ScanMany(new string[] { "-p", "-pp" }, parsePointHandler)
			.WhenGoodOrMissing(r => { PointList = r.Value.ToList(); return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(PointList.Count < 2) {
			Log.Error(Note.MissingArgument($"-p / -pp. drawing a line requires at least two points"));
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
	internal List<object> PointList;
	internal LineKind Kind;
	readonly ICoreLog Log;
}
