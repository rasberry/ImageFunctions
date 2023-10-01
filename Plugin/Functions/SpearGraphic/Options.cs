using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.SpearGraphic;

public sealed class Options : IOptions
{
	public static Graphic Spear;
	public static ColorRGBA BackgroundColor;
	public static int? RandomSeed;

	public const int DefaultWidth = 1024;
	public const int DefaultHeight = 1024;

	public static void Usage(StringBuilder sb)
	{
		sb.ND(1,"Creates a spear graphic");
		sb.ND(1,"-g (name)"   ,"Choose which graphic to create");
		sb.ND(1,"-bg (color)" ,"Change Background color (default transparent)");
		sb.ND(1,"-rs (number)","Random Int32 seed value (defaults to system picked)");
		sb.WT();
		sb.ND(1,"Available Graphics");
		sb.PrintEnum<Graphic>(1,excludeZero:true);
	}

	public static bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if (p.Default("-bg",out BackgroundColor,PlugColors.Transparent).IsInvalid()) {
			return false;
		}
		if (p.Default("-rs",out RandomSeed,null).IsInvalid()) {
			return false;
		}
		if (p.Expect("-g",out Spear).IsInvalid()) {
			Tell.MustProvideInput("-g");
			return false;
		}

		return true;
	}
}

public enum Graphic
{
	None = 0,
	First_Twist1,
	First_Twist2,
	First_Twist3,
	Second_Twist3a,
	Second_Twist3b,
	Second_Twist3c,
	Second_Twist4,
	Third,
	Fourth
}

/*
	public enum Graphic
	{
		None = 0,
		First_Twist1,
		First_Twist2,
		First_Twist3,
		Second_Twist3a,
		Second_Twist3b,
		Second_Twist3c,
		Second_Twist4,
		Third,
		Fourth
	}

	public class Options
	{
		public Graphic Spear = Graphic.None;
		public IColor BackgroundColor = ColorHelpers.Transparent;
		public int? RandomSeed = null;
	}
*/