using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;

namespace ImageFunctions.Derivatives
{
	public class Function : AbstractFunction
	{
		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.Has("-g").IsGood()) {
				O.DoGrayscale = true;
			}
			if (p.Has("-a").IsGood()) {
				O.UseABS = true;
			}

			if (p.Expect(out InImage,"input image").IsBad()) {
				return false;
			}
			if (p.DefaultFile(out OutImage,InImage).IsBad()) {
				return false;
			}

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.Derivatives);
			sb.WL();
			sb.WL(0,name+" [options] (input image) [output image]");
			sb.WL(1,"Computes the color change rate - similar to edge detection");
			sb.WL(1,"-g","Grayscale output");
			sb.WL(1,"-a","Calculate absolute value difference");
		}

		public override IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle)
		{
			var proc = new Processor<TPixel>();
			proc.O = O;
			proc.Source = source;
			proc.Bounds = sourceRectangle;
			return proc;
		}

		public override void Main()
		{
			Main<RgbaD>();
		}

		Options O = new Options();
	}
}
