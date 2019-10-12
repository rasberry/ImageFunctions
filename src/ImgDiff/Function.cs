using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ImageFunctions.ImgDiff
{
	public class Function : AbstractFunction
	{
		protected override void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			proc.O = O;
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
				if (curr == "-i") {
					O.MatchSamePixels = true;
				}
				else if (curr == "-x") {
					O.OutputOriginal = true;
				}
				else if (curr == "-o" && ++a<len) {
					string num = args[a];
					OptionsHelpers.ParseNumberPercent(num,out double d);
					if (d < double.Epsilon) {
						Log.Error("invalid opacity \""+d+"\"");
						return false;
					}
					O.HilightOpacity = d;
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

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Action.ImgDiff);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (image one) (image two) [output image]");
			sb.AppendLine(" Highlights differences between two images.");
			sb.AppendLine(" By default differeces are hilighted based on distance ranging from hilight color to white");
			sb.AppendLine(" -o (number)[%]              Overlay hilight color at given opacity");
			sb.AppendLine(" -i                          Match identical pixels instead of differences");
			sb.AppendLine(" -x                          Output original pixels instead of hilighting them");
			//sb.AppendLine(" -c (color)                  Change hilight color");
		}

		Options O = new Options();
	}
}