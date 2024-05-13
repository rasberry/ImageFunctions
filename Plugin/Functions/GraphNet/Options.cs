using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.GraphNet;

public sealed class Options : IOptions
{
	public int? NodeCount;
	public int Connectivity;
	public int States;
	public int? RandomSeed = null;
	public double PerturbationRate;
	public const int DefaultWidth = 1024;
	public const int DefaultHeight = 1024;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1,"Creates a plot of a boolean-like network with a random starring state.");
		sb.ND(1,"-b (number)"    ,"Number of states (default 2)");
		sb.ND(1,"-n (number)"    ,"Number of nodes in the network (defaults to width of image)");
		sb.ND(1,"-c (number)"    ,"Connections per node (default 3)");
		sb.ND(1,"-p (number)"    ,"Chance of inserting a perturbation (default 0)");
		sb.ND(1,"-rs (number)"   ,"Random Int32 seed value (defaults to system picked)");
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string s) => {
			return ExtraParsers.ParseNumberPercent(s);
		});

		if (p.Scan<int>("-b", 2)
			.WhenGoodOrMissing(r => { States = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}
		if (p.Scan<int?>("-n")
			.WhenGood(r => { NodeCount = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}
		if (p.Scan<int>("-c", 3)
			.WhenGoodOrMissing(r => { Connectivity = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}
		if (p.Scan<int>("-rs")
			.WhenGood(r => { RandomSeed = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}
		if (p.Scan<double>("-p", 0.0, parser)
			.WhenGoodOrMissing(r => { PerturbationRate = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		return true;
	}
}