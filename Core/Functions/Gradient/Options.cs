using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Gradients;
using Rasberry.Cli;
using System.Drawing;
using System.Globalization;

namespace ImageFunctions.Core.Functions.Gradient;

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
			Description = new UsageDescription(1, "Draws a Gradient"),
			Parameters = [
				new UsageOne<GradientKind>(1, "-g", "Gradient Shape (default Linear)") { Default = GradientKind.Linear },
				new UsageOne<Point>(1, "-ps", "Staring point coordinates"),
				new UsageOne<Point>(1, "-pps", "Staring point relative(%) coordinates"),
				new UsageOne<Point>(1, "-pe", "Ending point coordinates"),
				new UsageOne<Point>(1, "-ppe", "Ending point relative(%) coordinates"),
				GradientHelpers.GradientUsageParameter(1)
			],
			EnumParameters = [
				new UsageEnum<GradientKind>(1, "Available Gradient Shapes:")
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		// use ParseNumberPercent for parsing numbers like 0.5 or 50%
		var pctparser = new ParseParams.Parser<double>(n => {
			return ExtraParsers.ParseNumberPercent(n);
		});

		if(p.Scan<GradientKind>("-m", GradientKind.Linear)
			.WhenGoodOrMissing(r => { Kind = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.ScanGradient(Log, register)
			.WhenGood(r => { Gradient = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-ps", Point.Empty, OptionsAide.ParsePointSize<Point>)
			.WhenGoodOrMissing(r => { Start = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-pps", PointF.Empty, OptionsAide.ParsePointSize<PointF>)
			.WhenGoodOrMissing(r => { StartPct = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-pe", Point.Empty, OptionsAide.ParsePointSize<Point>)
			.WhenGoodOrMissing(r => { End = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-ppe", PointF.Empty, OptionsAide.ParsePointSize<PointF>)
			.WhenGoodOrMissing(r => { EndPct = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		return true;
	}

	public enum GradientKind
	{
		Linear = 0,
		BiLinear = 1,
		Radial = 2,
		Sqare = 3,
		Conical = 4,
		BiConical = 5
	}

	internal Lazy<IColorGradient> Gradient;
	internal Point Start;
	internal Point End;
	internal PointF StartPct;
	internal PointF EndPct;
	internal GradientKind Kind;
	readonly ICoreLog Log;
}
