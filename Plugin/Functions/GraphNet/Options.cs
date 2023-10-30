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

	public void Usage(StringBuilder sb)
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
		var parser = new ParseParams.Parser<double>((string s, out double p) => {
			return ExtraParsers.TryParseNumberPercent(s,out p);
		});

		if (p.Default("-b",out States, 2).IsInvalid()) {
			Tell.CouldNotParse("-b");
			return false;
		}
		if (p.Default("-n",out NodeCount).IsInvalid()) {
			Tell.CouldNotParse("-n");
			return false;
		}
		if (p.Default("-c",out Connectivity, 3).IsInvalid()) {
			Tell.CouldNotParse("-c");
			return false;
		}
		if (p.Default("-rs",out RandomSeed).IsInvalid()) {
			Tell.CouldNotParse("-rs");
			return false;
		}
		if (p.Default("-p",out PerturbationRate, 0.0, parser).IsInvalid()) {
			Tell.CouldNotParse("-p");
			return false;
		}

		return true;
	}
}