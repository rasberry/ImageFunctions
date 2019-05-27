using System;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageFunctions.PixelateDetails
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

		void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			proc.ImageSplitFactor = ImageSplitFactor;
			proc.UseProportionalSplit = UseProportionalSplit;
			ctx.ApplyProcessor(proc);
		}

		public bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-p") {
					UseProportionalSplit = true;
				}
				else if (curr == "-ps" && ++a<len) {
					string num = args[a];
					bool isPercent = false;
					if (num.EndsWith('%')) {
						isPercent = true;
						num = num.Remove(num.Length - 1);
					}
					if (!double.TryParse(num, out double d)) {
						Log.Error("could not parse \""+num+"\" as a number");
						return false;
					}
					if (!double.IsFinite(d) || d < double.Epsilon) {
						Log.Error("invalid splitting factor \""+d+"\"");
						return false;
					}
					ImageSplitFactor = isPercent ? 100.0/d : d;
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
				OutImage = Helpers.CreateOutputFileName(InImage);
			}

			return true;
		}

		public void Usage(StringBuilder sb)
		{
			sb.AppendLine();
			sb.AppendLine("PixelateDetails [options] (input image) [output image]");
			sb.AppendLine(" -p                          Use proportianally sized sections");
			sb.AppendLine(" -ps (number)[%]             Multiple or percent of image dimension used for splitting (default is 2)");
		}

		string InImage = null;
		string OutImage = null;
		bool UseProportionalSplit = false;
		double ImageSplitFactor = 2.0;
	}
}