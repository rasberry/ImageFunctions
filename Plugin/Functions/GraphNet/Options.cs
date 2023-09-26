using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.GraphNet;

public sealed class Options : IOptions
{
	public static int NodeCount = 32;
	public static int Connectivity = 3;
	public static int States = 2;
	public static int? RandomSeed = null;
	public static double PertubationRate = 0.0;
	public const int DefaultWidth = 1024;
	public const int DefaultHeight = 1024;

	public static void Usage(StringBuilder sb)
	{
		sb.ND(1,"Creates a plot of a boolean-like network with a random starring state.");
		sb.ND(1,"-b (number)"    ,"Number of states (default 2)");
		sb.ND(1,"-n (number)"    ,"Number of nodes in the network (defaults to width of image)");
		sb.ND(1,"-c (number)"    ,"Connections per node (default 3)");
		sb.ND(1,"-p (number)"    ,"Chance of inserting a perturbation (default 0)");
		sb.ND(1,"-rs (number)"   ,"Random Int32 seed value (defaults to system picked)");
	}

	public static bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if (p.Default("-b",out States,2).IsInvalid()) {
			return false;
		}
		if (p.Default("-n",out NodeCount, 32).IsInvalid()) {
			return false;
		}
		if (p.Default("-c",out Connectivity, 3).IsInvalid()) {
			return false;
		}
		if (p.Default("-rs",out RandomSeed, null).IsInvalid()) {
			return false;
		}
		if (p.Default("-p",out PertubationRate, 0.0, PlugTools.ParseNumberPercent).IsInvalid()) {
			return false;
		}

		return true;
	}
}