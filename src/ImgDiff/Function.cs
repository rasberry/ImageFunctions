using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;

namespace ImageFunctions.ImgDiff
{
	public class Function : AbstractFunction
	{
		public override IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle)
		{
			var proc = new Processor<TPixel>();
			proc.O = O;
			proc.Source = source;
			proc.Bounds = sourceRectangle;
			return proc;
		}

		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.Has("-i").IsGood()) {
				O.MatchSamePixels = true;
			}
			if (p.Has("-x").IsGood()) {
				O.OutputOriginal = true;
			}

			var pop = p.Default("-o",out double hop, par:OptionsHelpers.ParseNumberPercent);
			if (pop.IsInvalid()) {
				return false;
			}
			else if (pop.IsGood()) {
				if (hop < double.Epsilon) {
					Tell.MustBeGreaterThanZero("-o");
					return false;
				}
				O.HilightOpacity = hop;
			}

			if (p.Default("-c",out O.HilightColor,Color.Magenta).IsInvalid()) {
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

		#if false
		public override bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-i") {
					O.MatchSamePixels = true;
				}
				else if (curr == "-x") {
					O.OutputOriginal = true;
				}
				else if (curr == "-o" && ++a<len) {
					string num = args[a];
					if (!OptionsHelpers.ParseNumberPercent(num,out double d)
						|| d < double.Epsilon)
					{
						Log.Error("invalid opacity \""+num+"\"");
						return false;
					}
					O.HilightOpacity = d;
				}
				else if (curr == "-c" && ++a<len) {
					string clr = args[a];
					if (!OptionsHelpers.TryParseColor(clr,out Color c)) {
						Log.Error("invalid color \""+clr+"\"");
						return false;
					}
					O.HilightColor = c;
				}
				else if (String.IsNullOrEmpty(InImage)) {
					InImage = curr;
				}
				else if (String.IsNullOrEmpty(O.CompareImage)) {
					O.CompareImage = curr;
				}
				else if (String.IsNullOrEmpty(OutImage)) {
					OutImage = curr;
				}
			}

			if (String.IsNullOrEmpty(InImage)) {
				Log.Error("first image must be provided");
				return false;
			}
			if (!File.Exists(InImage)) {
				Log.Error("cannot find image \""+InImage+"\"");
				return false;
			}
			if (String.IsNullOrEmpty(O.CompareImage)) {
				Log.Error("second image must be provided");
				return false;
			}
			if (!File.Exists(InImage)) {
				Log.Error("cannot find image \""+O.CompareImage+"\"");
				return false;
			}
			if (String.IsNullOrEmpty(OutImage)) {
				OutImage = OptionsHelpers.CreateOutputFileName(InImage+"-"+O.CompareImage);
			}

			return true;
		}
		#endif

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

		public override void Main()
		{
			Main<RgbaD>();
		}

		Options O = new Options();
	}
}