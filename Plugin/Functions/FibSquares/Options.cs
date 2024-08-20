using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.FibSquares;

public sealed class Options : IOptions
{
	public const int PhiWidth = 1597; //F17
	public const int PhiHeight = 987; //F16

	public enum DrawModeKind
	{
		None = 0,
		Plain = 1,
		Gradient = 2,
		Drag = 3
	}

	public DrawModeKind DraMode;
	public bool UseSpiralOrder;
	public bool DrawBorders;
	public int? Seed;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1,"Divides the canvas into squares and remainders consecutively which creates a Fibonacci-like sequence");
		sb.ND(1,"-m (mode)", "Choose drawing mode (default Plain)");
		sb.ND(1,"-s","Use spiral sequence ordering instead or random");
		sb.ND(1,"-b","Also draw borders around each square");
		sb.ND(1,"-rs (number)","Set the random number seed to produce consistent imges");
		sb.WT();
		sb.WT(1,"Available Modes:");
		sb.PrintEnum<DrawModeKind>(1, excludeZero:true);
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		//use ParseNumberPercent for parsing numbers like 0.5 or 50%
		//var parser = new ParseParams.Parser<double>((string n) => {
		//	return ExtraParsers.ParseNumberPercent(n);
		//});

		if (p.Scan<DrawModeKind>("-m", DrawModeKind.Plain)
			.WhenGoodOrMissing(r => { DraMode = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan<int?>("-rs", null)
			.WhenGoodOrMissing(r => { Seed = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Has("-s").IsGood()) { UseSpiralOrder = true; }
		if (p.Has("-b").IsGood()) { DrawBorders = true; }

		return true;
	}
}