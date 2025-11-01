using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Gradients;
using ImageFunctions.Core.Metrics;
using Rasberry.Cli;
using System.Drawing;

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
				new UsageOne<DirectionKind>(1, "-d", "Gradient Direction (default Forward)") { Default = DirectionKind.Forward },
				new UsageOne<Point>(1, "-ps", "Staring point coordinates"),
				new UsageOne<Point>(1, "-pe", "Ending point coordinates"),
				new UsageOne<PointD>(1, "-pps", "Staring point relative(%) coordinates"),
				new UsageOne<PointD>(1, "-ppe", "Ending point relative(%) coordinates"),
				new UsageOne<double>(1, "-s", "Gradient speed multiplier (default 1.0)") { Default = 1.0, Min = -20.0, Max = 20.0 },
				new UsageOne<double>(1, "-o", "Gradient color offset (default 0.0)") { Default = 0.0, Min = 0.0, Max = 1.0 },
				new UsageOne<bool>(1, "-r", "Restrict gradient to area defined by the coordinates") { Default = false },
				GradientHelpers.GradientUsageParameter(1),
				MetricHelpers.MetricUsageParameter(1),
			],
			EnumParameters = [
				new UsageEnum<GradientKind>(1, "Available Gradient Shapes:"),
				new UsageEnum<DirectionKind>(1, "Available Gradient Directions:")
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		static PointD? parsePointPct(string value)
		{
			return OptionsAide.ParsePointSize(value,
				(a, b) => new PointD(a, b),
				s => ExtraParsers.ParseNumberPercent(s, null)
			);
		}

		static Point? parsePoint(string value)
		{
			return OptionsAide.ParsePointSize<Point>(value);
		}

		if(p.ScanGradient(Log, register)
			.WhenGood(r => { Gradient = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.ScanMetric(Log, register)
			.WhenGood(r => { Metric = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<GradientKind>("-m", GradientKind.Linear)
			.WhenGoodOrMissing(r => { Kind = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<DirectionKind>("-d", DirectionKind.Forward)
			.WhenGoodOrMissing(r => { Direction = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<Point?>("-ps", null, parsePoint)
			.WhenGoodOrMissing(r => { Start = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<Point?>("-pe", null, parsePoint)
			.WhenGoodOrMissing(r => { End = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-pps", null, parsePointPct)
			.WhenGoodOrMissing(r => { StartPct = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-ppe", null, parsePointPct)
			.WhenGoodOrMissing(r => { EndPct = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-s", 1.0)
			.WhenGoodOrMissing(r => { Speed = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-o", 0.0)
			.WhenGoodOrMissing(r => { Phase = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-g", GradientKind.Linear)
			.WhenGoodOrMissing(r => { Kind = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		Restrict = p.Has("-r").IsGood();

		if(Gradient == null) {
			throw Squeal.ArgumentNullOrEmpty(GradientHelpers.ParamName);
		}

		return true;
	}

	public enum GradientKind
	{
		Linear = 0,
		Radial = 1,
		Square = 2,
		Conical = 3,
		Bilinear = 4
	}

	public enum DirectionKind
	{
		Forward = 0,
		Backward = 1,
		ForBack = 2,
		BackFor = 3
	}

	internal Lazy<IColorGradient> Gradient;
	internal Point? Start;
	internal Point? End;
	internal PointD? StartPct;
	internal PointD? EndPct;
	internal GradientKind Kind;
	internal DirectionKind Direction;
	internal Lazy<IMetric> Metric;
	internal double Speed;
	internal double Phase;
	internal bool Restrict;

	readonly ICoreLog Log;
}
