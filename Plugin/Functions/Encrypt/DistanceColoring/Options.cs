using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Gradients;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.DistanceColoring;

// https://bsubercaseaux.github.io/blog/2023/packingchromatic/

public sealed class Options : IOptions, IUsageProvider
{
	public string SomeOption;
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
			Description = new UsageDescription(1, "Colors pixels with the smallest color index determined by distance to the nearest same color"),
			Parameters = [
				new UsageOne<PlacementKind>(1, "-p","Placement type (defaults to random)") { Default = PlacementKind.Random },
				new UsageOne<int>(1, "-rs", "Random Int32 seed value (defaults to system picked)"),
				GradientHelpers.GradientUsageParameter(1, true)
			],
			EnumParameters = [
				new UsageEnum<PlacementKind>(1,"Available Placements:")
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if(p.Scan<PlacementKind>("-p", PlacementKind.Random)
			.WhenGoodOrMissing(r => { Kind = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}
		if(p.Scan<int>("-rs")
			.WhenGood(r => { RandomSeed = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.ScanGradient(Log, register, true)
			.WhenGood(r => { Gradient = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		return true;
	}

	public PlacementKind Kind { get; set; }
	public int? RandomSeed { get; set; }
	public Lazy<IColorGradient> Gradient { get; set; }
}
