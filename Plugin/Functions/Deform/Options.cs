using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Samplers;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.Deform;

public sealed class Options : IOptions, IUsageProvider
{
	public Point? CenterPx;
	public PointF? CenterPp;
	public Mode WhichMode;
	public double Power;
	public Lazy<ISampler> Sampler;
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
			Description = new UsageDescription(1, "Warps an image using a mapping function"),
			Parameters = [
				new UsageOne<Point>(1, "-cx", "Coordinates of center in pixels"),
				new UsageOne<PointF>(1, "-cp", "Coordinates of center by proportion (default 50% 50%)") { Default = 0.5f, IsNumberPct = true },
				new UsageOne<double>(1, "-e", "(e) Power Exponent (default 2.0)") { Default = 2.0, Min = -20.0, Max = 20 },
				new UsageOne<Mode>(1, "-m", "Choose mode (default Polynomial)") { Default = Mode.Polynomial },
				SamplerHelpers.SamplerUsageParameter()
			],
			EnumParameters = [
				new UsageEnum<Mode>(1, "Available Modes") { DescriptionMap = ModeDesc, ExcludeZero = true }
			]
		};

		return u;
	}

	static string ModeDesc(object mode)
	{
		Mode m = (Mode)mode;
		if(m == Mode.Polynomial) { return "x^e/w, y^e/h"; }
		if(m == Mode.Inverted) { return "n/x, n/y; n = (x^e + y^e)"; }
		return "";
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});

		if(p.Scan<PointF>("-cp", par: OptionsAide.ParsePoint<PointF>)
			.WhenGood(r => { CenterPp = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<Point>("-cx", par: OptionsAide.ParsePoint<Point>)
			.WhenGood(r => { CenterPx = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		// -cp and -cx are either/or options so choose a default if neither were specified
		if(CenterPx == null && CenterPp == null) {
			CenterPp = new PointF(0.5f, 0.5f);
		}

		if(p.Scan("-e", 2.0)
			.WhenGoodOrMissing(r => { Power = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-m", Mode.Polynomial)
			.WhenGoodOrMissing(r => { WhichMode = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.ScanSampler(Log, register)
			.WhenGood(r => { Sampler = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		return true;
	}

	public enum Mode
	{
		None = 0,
		Polynomial = 1,
		Inverted = 2
	}
}
