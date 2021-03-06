using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.ImgDiff
{
	public class Function : AbstractFunction
	{
		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.Has("-i").IsGood()) {
				O.MatchSamePixels = true;
			}
			if (p.Has("-x").IsGood()) {
				O.OutputOriginal = true;
			}

			if(p.Default("-o",out O.HilightOpacity, par:OptionsHelpers.ParseNumberPercent)
				.BeGreaterThanZero("-o",O.HilightOpacity,true).IsInvalid()) {
				return false;
			}

			if (p.Default("-c",out O.HilightColor,Helpers.ColorHelpers.Magenta).IsInvalid()) {
				return false;
			}
			if (p.ExpectFile(out InImage,"first image").IsBad()) {
				return false;
			}
			if (p.ExpectFile(out O.CompareImage,"second image").IsBad()) {
				return false;
			}
			string outDef = $"{Path.GetFileNameWithoutExtension(InImage)}-{Path.GetFileNameWithoutExtension(O.CompareImage)}";
			if (p.DefaultFile(out OutImage,outDef).IsInvalid()) {
				return false;
			}

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.ImgDiff);
			sb.WL();
			sb.WL(0,name + " [options] (image one) (image two) [output image]");
			sb.WL(1,"Highlights differences between two images.");
			sb.WL(1,"By default differeces are hilighted based on distance ranging from hilight color to white");
			sb.WL(1,"-o (number)[%]","Overlay hilight color at given opacity");
			sb.WL(1,"-i"            ,"Match identical pixels instead of differences");
			sb.WL(1,"-x"            ,"Output original pixels instead of hilighting them");
			sb.WL(1,"-c (color)"    ,"Change hilight color (default is magenta)");
		}

		protected override AbstractProcessor CreateProcessor()
		{
			return new Processor { O = O };
		}

		Options O = new Options();
	}

}