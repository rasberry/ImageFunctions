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
			proc.SourceRectangle = sourceRectangle;
			return proc;
		}

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

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.PixelateDetails);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" Creates areas of flat color by recusively splitting high detail chunks");
			sb.AppendLine(" -p                          Use proportianally sized sections (default is square sized sections)");
			sb.AppendLine(" -s (number)[%]              Multiple or percent of image dimension used for splitting (default 2.0)");
			sb.AppendLine(" -r (number)[%]              Count or percent or sections to re-split (default 50%)");
		}

		public override void Main()
		{
			Main<RgbaD>();
		}

		Options O = new Options();
	}
}