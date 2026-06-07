using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.TrimEdge;

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
			Description = new UsageDescription(1, "Removes similar colored outside edges from an image"),
			Parameters = [
				new UsageOne<TrimKind>(1, "-t", $"Type of trim (default {nameof(TrimKind.EdgeStripe)})") { Default = TrimKind.EdgeStripe },
				new UsageOne<ColorRGBA>(1, "-c", "Use this color as the edge color"),
				new UsageOne<double>(1, "-f", "Fuzz factor or how much deviation to allow [0-100%] (default 0)") { Default = 0.0, Min = 0.0, Max = 1.0, IsNumberPct = true },
				new UsageOne<bool>(1, "-t", "Replace trimmed pixels with transparecy instead of cropping"),
				new UsageOne<bool>(1, "-nl", "Keep original layer instead of replacing it")
			],
			EnumParameters = [
				new UsageEnum<TrimKind>(1, "Available Trim Types") { DescriptionMap = TrimDesc }
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		// use ParseNumberPercent for parsing numbers like 0.5 or 50%
		var pctParser = new ParseParams.Parser<double>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});

		if (p.Scan<ColorRGBA?>("-c", null)
			.WhenGoodOrMissing(r => { BackColor = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan<double>("-f", 0.0, pctParser)
			.WhenGoodOrMissing(r => { Fuzz = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if (p.Has("-t").IsGood()) {
			FillTransparent = true;
		}
		if (p.Has("-nl").IsGood()) {
			KeepOrigLayer = true;
		}

		if (p.Scan<TrimKind>("-t", TrimKind.EdgeStripe)
			.WhenGoodOrMissing(r => { TrimType = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if (Fuzz< 0.0 || Fuzz > 1.0) {
			Log.Error(Note.MustBeBetween("-f","0.0 / 0%","1.0 / 100%"));
			return false;
		}

		return true;
	}

	public enum TrimKind
	{
		EdgeStripe,
		DwindleTrim
	}

	static string TrimDesc(TrimKind kind)
	{
		return kind switch {
			TrimKind.EdgeStripe => "Trim 1px stripes outside-in based on similarity accross stipe",
			TrimKind.DwindleTrim => "Trim by reducing image size and colors to remove border color variability",
			_ => ""
		};
	}

	public TrimKind TrimType;
	public double Fuzz;
	public ColorRGBA? BackColor;
	public bool FillTransparent;
	public bool KeepOrigLayer;
	readonly ICoreLog Log;
}