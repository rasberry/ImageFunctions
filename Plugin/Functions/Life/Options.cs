using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Life;

public sealed class Options : IOptions
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

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1, "Runs the Game of Life simulation and captures the state in various ways - The given starting image is turned into black/white");
		sb.ND(1, "-i (number)", "maximum number of iterations (default 10000)");
		//sb.ND(1,"-t (name)"    ,"start with a template instead of using the first layer");
		sb.ND(1, "-nl", "render the output on a new layer instead of replacing the original one");
		sb.ND(1, "-ch", "run one simulation per channel (RGB)");
		sb.ND(1, "-th (number%)", "threshold for picking white/black (default 50%)");
		sb.ND(1, "-nh", "disable recording history trails");
		sb.ND(1, "-b (number%)", "brighten history pixels by amount");
		sb.ND(1, "-s", "Stop when population stabilizes for 10 iterations");
		sb.ND(1, "-w", "Let world wrap around at the edges");
		//add way to change rule B3/S23 is the standard one
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});

		if(p.Scan("-i", 10000u)
			.WhenGoodOrMissing(r => { IterationMax = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-th", 0.5, parser)
			.WhenGoodOrMissing(r => { Threshhold = r.Value; return r; })
			.WhenInvalidTellDefault()
			.BeBetween(0.0, 1.0)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<double>("-b", par: parser)
			.WhenGood(r => {
				Brighten = r.Value;
				return r.BeBetween(0.0, 1.0);
			})
			.WhenInvalidTellDefault()
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
