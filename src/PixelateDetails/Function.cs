using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.PixelateDetails
{
	public class Function : IFAbstractFunction
	{
		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.Has("-p").IsGood()) {
				O.UseProportionalSplit = true;
			}
			if (p.Default("-s",out O.ImageSplitFactor,2.0)
				.BeGreaterThanZero("-s",O.ImageSplitFactor).IsInvalid()) {
				return false;
			}
			if(p.Default("-r",out O.DescentFactor,0.5)
				.BeGreaterThanZero("-r",O.DescentFactor).IsInvalid()) {
				return false;
			}

			if (p.ExpectFile(out InImage,"input image").IsBad()) {
				return false;
			}
			if (p.DefaultFile(out OutImage,InImage).IsInvalid()) {
				return false;
			}

			return true;
		}

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

		protected override IFAbstractProcessor CreateProcessor()
		{
			return new Processor();
		}

		Options O = new Options();
	}

	#if false
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
			if (p.Default("-s",out O.ImageSplitFactor,2.0)
				.BeGreaterThanZero("-s",O.ImageSplitFactor).IsInvalid()) {
				return false;
			}
			if(p.Default("-r",out O.DescentFactor,0.5)
				.BeGreaterThanZero("-r",O.DescentFactor).IsInvalid()) {
				return false;
			}

			if (p.ExpectFile(out InImage,"input image").IsBad()) {
				return false;
			}
			if (p.DefaultFile(out OutImage,InImage).IsInvalid()) {
				return false;
			}

			return true;
		}

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
	#endif
}