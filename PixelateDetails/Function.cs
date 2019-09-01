using System;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ImageFunctions.PixelateDetails
{
	public class Function : AbstractFunction
	{
		protected override void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			proc.ImageSplitFactor = ImageSplitFactor;
			proc.UseProportionalSplit = UseProportionalSplit;
			proc.DescentFactor = DescentFactor;
			if (Rect.IsEmpty) {
				ctx.ApplyProcessor(proc);
			} else {
				ctx.ApplyProcessor(proc,Rect);
			}
		}

		public override bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-p") {
					UseProportionalSplit = true;
				}
				else if (curr == "-s" && ++a<len) {
					string num = args[a];
					OptionsHelpers.ParseNumberPercent(num,out double d);
					if (d < double.Epsilon) {
						Log.Error("invalid splitting factor \""+d+"\"");
						return false;
					}
					ImageSplitFactor = d;
				}
				else if (curr == "-r" && ++a<len) {
					string num = args[a];
					OptionsHelpers.ParseNumberPercent(num,out double d);
					if (d < double.Epsilon) {
						Log.Error("invalid re-split factor \""+d+"\"");
						return false;
					}
					DescentFactor = d;
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
			string name = OptionsHelpers.FunctionName(Action.PixelateDetails);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" Creates areas of flat color by recusively splitting high detail chunks");
			sb.AppendLine(" -p                          Use proportianally sized sections (default is square sized sections)");
			sb.AppendLine(" -s (number)[%]              Multiple or percent of image dimension used for splitting (default 2.0)");
			sb.AppendLine(" -r (number)[%]              Count or percent or sections to re-split (default 50%)");
		}

		bool UseProportionalSplit = false;
		double ImageSplitFactor = 2.0;
		double DescentFactor = 0.5;
	}
}