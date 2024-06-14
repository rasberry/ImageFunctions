using ImageFunctions.Core;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.SpearGraphic;

public sealed class Options : IOptions, IUsageProvider
{
	public Graphic Spear;
	public ColorRGBA BackgroundColor;
	public int? RandomSeed;

	public const int DefaultWidth = 1024;
	public const int DefaultHeight = 1024;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1,"Creates a spear graphic"),
			Parameters = [
				new UsageOne<Graphic>(1, "-g (name)", "Choose which graphic to create"),
				new UsageOne<Color>(1, "-bg (color)", "Change Background color (default transparent)"),
				new UsageOne<int>(1, "-rs (number)", "Random Int32 seed value (defaults to system picked)")
			],
			EnumParameters = [
				new UsageEnum<Graphic>(1, "Available Graphics") { ExcludeZero = true }
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if(p.Scan("-bg", Color.Transparent)
			.WhenGoodOrMissing(r => {
				var c = r.Value;
				BackgroundColor = ColorRGBA.FromRGBA255(c.R, c.G, c.B, c.A);
				return r;
			})
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<int>("-rs")
			.WhenGood(r => { RandomSeed = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<Graphic>("-g")
			.WhenGoodOrMissing(r => { Spear = r.Value; return r; })
			.WhenInvalidTellDefault()
			.WhenMissing(r => { Log.Error(Note.MustProvideInput(r.Name)); return r; })
			.IsBad() //option is required
		) {
			return false;
		}

		return true;
	}
}

public enum Graphic
{
	None = 0,
	First_Twist1 = 1,
	First_Twist2 = 2,
	First_Twist3 = 3,
	Second_Twist3a = 4,
	Second_Twist3b = 5,
	Second_Twist3c = 6,
	Second_Twist4 = 7,
	Third = 8,
	Fourth = 9
}
