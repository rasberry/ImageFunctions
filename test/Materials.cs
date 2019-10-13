using System;
using ImageFunctions;

namespace test
{
	public static class Materials
	{
		public static void EnsureImagesExist()
		{

		}

		public static string[] GetTestImageNames(Activity which)
		{
			switch(which)
			{
			default: case Activity.None:
				return new string[0];
			case Activity.PixelateDetails:
				return new string[] { "boy","building","cats","cloud","cookie","creek","flower" };
			case Activity.Derivatives:
				return new string[] { "fractal","handle","harddrive","lego","pool","rainbow","road" };
			case Activity.AreaSmoother:
			case Activity.AreaSmoother2:
				return new string[] { "rock-p","scorpius-p","shack-p","shell-p","skull-p","spider-p","toes-p" };
			case Activity.ZoomBlur:
				return new string[] { "zebra","boy","building","cats","cloud","cookie","creek" };
			case Activity.Swirl:
				return new string[] { "flower","fractal","handle","harddrive","lego","pool","rainbow" };
			case Activity.Deform:
				return new string[] { "road","rock","scorpius","shack","shell","skull","spider" };
			case Activity.Encrypt:
				return new string[] { "toes","zebra" };
			case Activity.PixelRules:
				return new string[] { "boy","building","cats","cloud","cookie","creek","flower" };
			case Activity.ImgDiff:
				return new string[] { "rock","rock-p","skull","skull-p" };
			}
		}
	}
}