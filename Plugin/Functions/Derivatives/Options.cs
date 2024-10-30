using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Derivatives;

public sealed class Options : IOptions, IUsageProvider
{
	public bool UseABS = false;
	public bool DoGrayscale = false;
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
			Description = new UsageDescription(1, "Computes the color change rate - similar to edge detection"),
			Parameters = [
				new UsageOne<bool>(1, "-g", "Enable grayscale output"),
				new UsageOne<bool>(1, "-a", "Calculate absolute value difference")
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if(p.Has("-g").IsGood()) {
			DoGrayscale = true;
		}
		if(p.Has("-a").IsGood()) {
			UseABS = true;
		}

		return true;
	}
}
