using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Life;

public sealed class Options : IOptions, IUsageProvider
{
	public ulong IterationMax;
	public double Threshhold;
	public double? Brighten;
	public bool MakeNewLayer;
	public bool UseChannels;
	public bool NoHistory;
	public bool UseLog;
	public bool StopWhenStable;
	public bool Wrap;
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
			Description = new UsageDescription(1, "Runs the Game of Life simulation and captures the state in various ways - The given starting image is turned into black/white"),
			Parameters = [
				new UsageOne<ulong>(1, "-i", "maximum number of iterations (default 10000)") { Min = 1, Default = 10000, Max = 1E+12 },
				//TODO new UsageOne<>(1, "-t (name)", "start with a template instead of using the first layer"),
				new UsageOne<bool>(1, "-nl", "render the output on a new layer instead of replacing the original one"),
				new UsageOne<bool>(1, "-ch", "run one simulation per channel (RGB)"),
				new UsageOne<double>(1, "-th", "threshold for picking white/black (default 50%)") { Default = 0.5, IsNumberPct = true },
				new UsageOne<bool>(1, "-nh", "disable recording history trails"),
				new UsageOne<double>(1, "-b", "brighten history pixels by amount") { IsNumberPct = true },
				new UsageOne<bool>(1, "-s", "Stop when population stabilizes for 10 iterations"),
				new UsageOne<bool>(1, "-w", "Let world wrap around at the edges"),
				//TODO add way to change rule B3/S23 is the standard one
			],
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});

		if(p.Scan<ulong>("-i", 10000ul)
			.WhenGoodOrMissing(r => { IterationMax = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-th", 0.5, parser)
			.WhenGoodOrMissing(r => { Threshhold = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.BeBetween(Log, 0.0, 1.0)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<double>("-b", par: parser)
			.WhenGood(r => {
				Brighten = r.Value;
				return r.BeBetween(Log, 0.0, 1.0);
			})
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Has("-nl").IsGood()) { MakeNewLayer = true; }
		if(p.Has("-ch").IsGood()) { UseChannels = true; }
		if(p.Has("-nh").IsGood()) { NoHistory = true; }
		if(p.Has("-s").IsGood()) { StopWhenStable = true; }
		if(p.Has("-w").IsGood()) { Wrap = true; }
		if(p.Has("-log").IsGood()) { UseLog = true; }

		return true;
	}
}

public enum Channel
{
	All = 0,
	Red = 1,
	Green = 2,
	Blue = 3
}
