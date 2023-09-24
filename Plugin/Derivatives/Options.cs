using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Derivatives;

public sealed class Options : IOptions
{
	public static bool UseABS = false;
	public static bool DoGrayscale = false;

	public static void Usage(StringBuilder sb)
	{
		sb.ND(1,"Computes the color change rate - similar to edge detection");
		sb.ND(1,"-g","Grayscale output");
		sb.ND(1,"-a","Calculate absolute value difference");
	}

	public static bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if (p.Has("-g").IsGood()) {
			DoGrayscale = true;
		}
		if (p.Has("-a").IsGood()) {
			UseABS = true;
		}

		return true;
	}
}