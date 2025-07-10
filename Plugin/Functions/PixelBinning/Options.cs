using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.PixelBinning;

public sealed class Options : IOptions, IUsageProvider
{
	public Calculation PickCalc;
	public Size BinSize;
	public bool IncludeAlpha;
	public bool MakeNewLayer;
	public bool ResizeLayer;

	readonly ICoreLog Log;
	static Size DefaultBinSize = new Size(2, 2);

	public Options(IFunctionContext context)
	{
		if(context == null) { throw Squeal.ArgumentNull(nameof(context)); }
		Log = context.Log;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if(p.Scan<Size>("-s", DefaultBinSize, OptionsAide.ParsePointSize<Size>)
			.WhenGoodOrMissing(r => { BinSize = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<Calculation>("-c", Calculation.Add)
			.WhenGoodOrMissing(r => { PickCalc = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Has("-c").IsGood()) {
			IncludeAlpha = true;
		}
		if(p.Has("-l").IsGood()) {
			MakeNewLayer = true;
		}
		if(p.Has("-r").IsGood()) {
			ResizeLayer = true;
		}

		return true;
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, "Groups neighbor pixels into bins and calculates a resulting single color for that group"),
			Parameters = [
				new UsageOne<Size>(1, "-s", "Size of Bin in pixels (default 2x2)") { Default = DefaultBinSize, Max = 32.0, Min = 1.0 },
				new UsageOne<Calculation>(1, "-c", "Calculation to use (default Add)") { Default = Calculation.Add },
				new UsageOne<bool>(1, "-a", "Include alpha channel (normally averaged)"),
				new UsageOne<bool>(1, "-l", "Create a new layer for the output"),
				new UsageOne<bool>(1, "-r", "Resize the layer to fit the resulting bins"),
			],
			EnumParameters = [
				new UsageEnum<Calculation>(1,"Available Calculations") { ExcludeZero = true, DescriptionMap = CalculationDescription }
			]
		};

		return u;
	}

	static string CalculationDescription(Calculation c)
	{
		return c switch {
			Calculation.Add => "color values added",
			Calculation.Average => "color values are averaged",
			Calculation.RMS => "color values are combined using root mean square",
			_ => ""
		};
	}

	public enum Calculation
	{
		None = 0,
		Add = 1,
		Average = 2,
		RMS = 3,
		Min = 4,
		Max = 5
	}
}