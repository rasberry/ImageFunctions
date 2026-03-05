using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Gradients;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.ComplexPlot;

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
				new UsageOne<string>(1,"-e", "Math expression evaluated at each pixel"),
				new UsageOne<RangeD>(1, "-rx", "Horizontal range of window (default -2.0,2.0)") { Min = -20.0, Max = 20.0, Default = new RangeD(-2.0,2.0) },
				new UsageOne<RangeD>(1, "-ry", "Vertical range of window (default -2.0,2.0)") { Min = -20.0, Max = 20.0, Default = new RangeD(-2.0,2.0) },
				new UsageOne<double>(1, "-f", "Apply flattening to magnitude with given strength (default 2.0)") { Min = 0.01, Max = 20.0, Default = 2.0 },
				new UsageOne<double>(1, "-go", "Gradient offset 0.0 to 1.0 (default 0.0)") { Min = 0.0, Max = 1.0, Default = 0.0 },
				new UsageOne<double>(1, "-gs", "Scale gradient by specified multiple (default 2.0)") { Min = 0.0, Max = 1000.0, Default = 2.0},
				new UsageOne<bool>(1, "-mo", "Use only maginitude for coloring"),
				new UsageOne<bool>(1, "-po", "Use only phase for coloring"),
				GradientHelpers.GradientUsageParameter(1)
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parseRange = new ParseParams.Parser<RangeD>(OptionsAide.ParseSeq2Type<RangeD>);

		static void SetMinMax(RangeD source, ref double min, ref double max)
		{
			min = Math.Min(source.Start, source.End);
			max = Math.Max(source.Start, source.End);
		}

		if(p.Scan<string>("-e")
			.WhenGoodOrMissing(r => { Expression = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(String.IsNullOrWhiteSpace(Expression)) {
			Log.Error(Note.MustNotBeNullOrEmpty("-e"));
			return false;
		}

		if(p.ScanGradient(Log, register, true)
			.WhenGood(r => { Gradient = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<RangeD>("-rx", new RangeD(-2.0, 2.0), parseRange)
			.WhenGoodOrMissing(r => { SetMinMax(r.Value, ref MinX, ref MaxX); return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<RangeD>("-ry", new RangeD(-2.0, 2.0), parseRange)
			.WhenGoodOrMissing(r => { SetMinMax(r.Value, ref MinY, ref MaxY); return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<double>("-f", 2.0)
			.WhenGoodOrMissing(r => { FlatPower = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<double>("-gs", 2.0)
			.WhenGoodOrMissing(r => { GradientScale = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<double>("-go", 0.0)
			.WhenGoodOrMissing(r => { GradOffset = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Has("-mo").IsGood()) {
			MagColorOnly = true;
		}
		if(p.Has("-po").IsGood()) {
			PhaColorOnly = true;
		}

		return true;
	}

	readonly ICoreLog Log;

	public string Expression;
	public Lazy<IColorGradient> Gradient;
	public double MinX;
	public double MaxX;
	public double MinY;
	public double MaxY;
	public double FlatPower;
	public bool MagColorOnly;
	public bool PhaColorOnly;
	public double GradientScale;
	public double GradOffset;
}
