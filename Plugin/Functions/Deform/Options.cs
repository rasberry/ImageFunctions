using ImageFunctions.Core;
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

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, "Warps an image using a mapping function"),
			Parameters = [
				new UsageTwo<int,int>(1, "-cx (number) (number)", "Coordinates of center in pixels") { Min = 0 },
				new UsageTwo<double,double>(1, "-cp (number)[%] (number)[%]", "Coordinates of center by proportion (default 50% 50%)") { Min = 0.0, Max = 1.0 },
				new UsageOne<double>(1, "-e (number)", "(e) Power Exponent (default 2.0)") { Default = 2.0 },
				new UsageOne<Mode>(1, "-m (mode)", "Choose mode (default Polynomial)") { Default = Mode.Polynomial },
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
		if (m == Mode.Polynomial) { return "x^e/w, y^e/h"; }
		if (m == Mode.Inverted) { return "n/x, n/y; n = (x^e + y^e)"; }
		return "";
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});

		if(p.Scan<double, double>("-cp", leftPar: parser, rightPar: parser)
			.WhenGood(r => {
				var (ppx, ppy) = r.Value;
				CenterPp = new PointF((float)ppx, (float)ppy);
				return r;
			})
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<int, int>("-cx")
			.WhenGood(r => {
				var (cx, cy) = r.Value;
				CenterPx = new Point(cx, cy);
				return r;
			})
			.WhenInvalidTellDefault()
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
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-m", Mode.Polynomial)
			.WhenGoodOrMissing(r => { WhichMode = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.ScanSampler(register)
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
