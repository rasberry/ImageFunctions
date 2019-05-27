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
	public class PixelateDetailsFunc : IFunction
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
			ctx.ApplyProcessor(new PixelateDetailsProcessor<Rgba32>());
		}

		public bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (String.IsNullOrEmpty(InImage)) {
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
		}

		string InImage = null;
		string OutImage = null;
	}
}