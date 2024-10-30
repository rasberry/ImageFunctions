using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.GraphNet;

public sealed class Options : IOptions, IUsageProvider
{
	public int? NodeCount;
	public int Connectivity;
	public int States;
	public int? RandomSeed = null;
	public double PerturbationRate;
	public const int DefaultWidth = 1024;
	public const int DefaultHeight = 1024;
	readonly ICoreLog Log;

	public Options(IFunctionContext context)
	{
		if (context == null) { throw Squeal.ArgumentNull(nameof(context)); }
		Log = context.Log;
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, "Creates a plot of a boolean-like network with a random starring state."),
			Parameters = [
				new UsageOne<int>(1, "-b", "Number of states (default 2)") { Default = 2, Min = 1, Max = 999 },
				new UsageOne<int?>(1, "-n", "Number of nodes in the network (defaults to width of image)") { Min = 1 },
				new UsageOne<int>(1, "-c", "Connections per node (default 3)") { Default = 3, Min = 1, Max = 99 },
				new UsageOne<double>(1, "-p", "Chance of inserting a perturbation (default 0)") { IsNumberPct = true },
				new UsageOne<int>(1, "-rs", "Random Int32 seed value (defaults to system picked)"),
			],
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string s) => {
			return ExtraParsers.ParseNumberPercent(s);
		});

		if(p.Scan<int>("-b", 2)
			.WhenGoodOrMissing(r => { States = r.Value; return r; })
			.BeGreaterThan(Log, 1, true)
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}
		if(p.Scan<int?>("-n")
			.WhenGood(r => { NodeCount = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}
		if(p.Scan<int>("-c", 3)
			.WhenGoodOrMissing(r => { Connectivity = r.Value; return r; })
			.BeGreaterThanZero(Log)
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
		if(p.Scan<double>("-p", 0.0, parser)
			.WhenGoodOrMissing(r => { PerturbationRate = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		return true;
	}
}
