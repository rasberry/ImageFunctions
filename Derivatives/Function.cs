using System;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageFunctions.Derivatives
{
	public class Function : IFunction
	{
		public void Main()
		{
			using (var img = Image.Load(InImage))
			{
				img.Mutate(Process);
				Helpers.SaveAsPng(OutImage,img);
			}
		}

		public bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-g") {
					DoGrayscale = true;
				}
				else if (curr == "-a") {
					UseABS = true;
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
				OutImage = Helpers.CreateOutputFileName(InImage);
			}
			return true;
		}

		public void Usage(StringBuilder sb)
		{
			string name = Helpers.FunctionName(Action.Derivatives);
			sb.AppendLine();
			sb.AppendLine(name+" [options] (input image) [output image]");
			sb.AppendLine(" Computes the color change rate - similar to edge detection");
			sb.AppendLine(" -g                          Grayscale output");
			sb.AppendLine(" -a                          Calculate absolute value difference");
		}

		void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			proc.DoGrayscale = DoGrayscale;
			proc.UseABS = UseABS;
			ctx.ApplyProcessor(proc);
		}

		string InImage = null;
		string OutImage = null;
		bool DoGrayscale = false;
		bool UseABS = false;
	}
}
