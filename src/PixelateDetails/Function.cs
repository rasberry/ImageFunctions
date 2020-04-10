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

namespace ImageFunctions.PixelateDetails
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

			if (p.Has("-p").IsGood()) {
				O.UseProportionalSplit = true;
			}

			var psp = p.Default("-s",out O.ImageSplitFactor,2.0);
			if (psp.IsInvalid()) {
				return false;
			}
			else if (psp.IsGood()) {
				if (O.ImageSplitFactor < double.Epsilon) {
					Tell.MustBeGreaterThanZero("-s");
					return false;
				}
			}

			var prs = p.Default("-r",out O.DescentFactor,0.5);
			if (prs.IsInvalid()) {
				return false;
			}
			else if (prs.IsGood()) {
				if (O.DescentFactor < double.Epsilon) {
					Tell.MustBeGreaterThanZero("-r");
					return false;
				}
			}

			if (p.ExpectFile(out InImage,"input image").IsBad()) {
				return false;
			}
			if (p.DefaultFile(out OutImage,InImage).IsInvalid()) {
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
				if (curr == "-p") {
					O.UseProportionalSplit = true;
				}
				else if (curr == "-s" && ++a<len) {
					string num = args[a];
					OptionsHelpers.ParseNumberPercent(num,out double d);
					if (d < double.Epsilon) {
						Log.Error("invalid splitting factor \""+d+"\"");
						return false;
					}
					O.ImageSplitFactor = d;
				}
				else if (curr == "-r" && ++a<len) {
					string num = args[a];
					OptionsHelpers.ParseNumberPercent(num,out double d);
					if (d < double.Epsilon) {
						Log.Error("invalid re-split factor \""+d+"\"");
						return false;
					}
					O.DescentFactor = d;
				}
				else if (String.IsNullOrEmpty(InImage)) {
					InImage = curr;
				}
				else if (String.IsNullOrEmpty(OutImage)) {
					OutImage = curr;
				}
			}

			if (String.IsNullOrEmpty(InImage)) {
				Log.Error("input image must be provided");
				return false;
			}
			if (!File.Exists(InImage)) {
				Log.Error("cannot find input image \""+InImage+"\"");
				return false;
			}
			if (String.IsNullOrEmpty(OutImage)) {
				OutImage = OptionsHelpers.CreateOutputFileName(InImage);
			}

			return true;
		}
		#endif

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.PixelateDetails);
			sb.WL();
			sb.WL(0,name + " [options] (input image) [output image]");
			sb.WL(1,"Creates areas of flat color by recusively splitting high detail chunks");
			sb.WL(1,"-p"            ,"Use proportianally sized sections (default is square sized sections)");
			sb.WL(1,"-s (number)[%]","Multiple or percent of image dimension used for splitting (default 2.0)");
			sb.WL(1,"-r (number)[%]","Count or percent or sections to re-split (default 50%)");
		}

		public override void Main()
		{
			Main<RgbaD>();
		}

		Options O = new Options();
	}
}