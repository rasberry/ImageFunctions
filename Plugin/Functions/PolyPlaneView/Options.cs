using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Gradients;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.PolyPlaneView;

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
			Description = new UsageDescription(1, "Fills an image with the values of a given polynomial in the complex plane"),
			Parameters = [
				new UsageMany<double>(1, "-c", "One coefficient of the polynomial (can specify multiple)"
					+ " Note: the order is reversed so 3x^2 + 4x + 1 is specified as '-c 1 -c 4 -c 3'"
				) { Min = -1000.0, Max = 1000.0 },
				new UsageOne<RangeD>(1, "-rx", "Horizontal range of window (default -2.0,2.0)") { Min = -20.0, Max = 20.0 },
				new UsageOne<RangeD>(1, "-ry", "Vertical range of window (default -2.0,2.0)") { Min = -20.0, Max = 20.0 },
				new UsageOne<double>(1, "-l", "Use log scale with given base") { Min = 1.0, Max = 1000.0 },
				GradientHelpers.GradientUsageParameter(1)
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		var parseSeq = new ParseParams.Parser<IEnumerable<double>>(n => {
			return ExtraParsers.ParseSequence<double>(n, new char[] {' '});
		});

		static void SetMinMax(RangeD source, ref double min, ref double max)
		{
			min = Math.Min(source.Start,source.End);
			max = Math.Max(source.Start,source.End);
		}

		if(p.ScanGradient(Log, register)
			.WhenGood(r => { Gradient = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if (p.ScanMany<double>("-c")
			.WhenGoodOrMissing(r => { Coefficients = r.Value.ToList(); return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan<RangeD>("-rx", new RangeD(-2.0,2.0))
			.WhenGoodOrMissing(r => { SetMinMax(r.Value, ref MinX, ref MaxX); return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan<RangeD>("-ry", new RangeD(-2.0,2.0))
			.WhenGoodOrMissing(r => { SetMinMax(r.Value, ref MinY, ref MaxY); return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan<double?>("-l")
			.WhenGoodOrMissing(r => { LogBase = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		return true;
	}

	readonly ICoreLog Log;
	public List<double> Coefficients;
	public double MinX;
	public double MaxX;
	public double MinY;
	public double MaxY;
	public double? LogBase;
	public Lazy<IColorGradient> Gradient;
}
