using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.AutoWhiteBalance;

public sealed class Options : IOptions
{
	public double DiscardRatio;
	public int BucketCount;
	public bool StretchAlpha;

	public void Usage(StringBuilder sb)
	{
		sb.ND(1,"Balances the image by stretching the color channels separately. The process"
			+" 'trims' the color histogram removing sections of low color use at the top and bottom of the range"
			+" then stretching the remaining colors to fill the space");
		sb.ND(1,"-p (number)[%]","Threshold for discarding infrequently used colors (default 0.05%)");
		sb.ND(1,"-b (number)"    ,"Number of buckets to use for histogram (default 256)");
		sb.ND(1,"-a"             ,"Also stretch alpha channel");
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string n, out double p) => {
			return ExtraParsers.TryParseNumberPercent(n, out p);
		});

		if (p.Default("-p",out DiscardRatio, 0.0005, parser).IsInvalid()) {
			return false;
		}
		if (p.Default("-b",out BucketCount, 256).IsInvalid()) {
			return false;
		}
		if (p.Has("-a").IsGood()) {
			StretchAlpha = true;
		}

		return true;
	}
}