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

			if (p.Has("-p").IsGood()) {
				O.DoGrayscale = true;
			}
			if (p.Has("-a").IsGood()) {
				O.UseABS = true;
			}

			if (p.Expect(out InImage,"input image").IsBad()) {
				return false;
			}
			if (p.Default(out OutImage).IsBad()) {
				OutImage = OptionsHelpers.CreateOutputFileName(InImage);
			}

			if (!File.Exists(InImage)) {
				Tell.CannotFindFile(InImage);
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
				if (curr == "-g") {
					O.DoGrayscale = true;
				}
				else if (curr == "-a") {
					O.UseABS = true;
				}
				else if (InImage == null) {
					InImage = curr;
				}
				else if (OutImage == null) {
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
