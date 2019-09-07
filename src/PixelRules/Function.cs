using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions.PixelRules
{
	public class Function : AbstractFunction
	{
		protected override void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			proc.Passes = Passes;
			proc.WhichMode = WhichMode;
			proc.MaxIters = MaxIters;
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
				if (curr == "-n" && ++a<len) {
					if (!Helpers.OptionsHelpers.TryParse(args[a],out Passes)) {
						Log.Error("invalid passes");
						return false;
					}
					if (Passes < 1) {
						Log.Error("passes must be greater than zero");
						return false;
					}
				}
				else if (curr == "-m" && ++a<len) {
					Mode which;
					if (!OptionsHelpers.TryParse<Mode>(args[a],out which)) {
						Log.Error("unkown mode \""+args[a]+"\"");
						return false;
					}
					WhichMode = which;
				}
				else if (curr == "-x" && ++a<len) {
					if (!Helpers.OptionsHelpers.TryParse(args[a],out MaxIters)) {
						Log.Error("invalid max iterations");
						return false;
					}
					if (MaxIters < 1) {
						Log.Error("max iterations must be greater than zero");
						return false;
					}
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
			string name = OptionsHelpers.FunctionName(Action.PixelRules);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" TODO");
			sb.AppendLine(" -m (mode)                   Which mode to use (default StairCaseDescend)");
			sb.AppendLine(" -n (number)                 Number of times to apply operation (default 1)");
			sb.AppendLine(" -x (number)                 Maximum number of iterations - in case of infinte loops (default 100)");
			sb.AppendLine();
			sb.AppendLine(" Available Modes");
			sb.AppendLine(" 1. StairCaseDescend");
			sb.AppendLine(" 2. StairCaseAscend");
			sb.AppendLine(" 3. StairCaseClosest");
			sb.AppendLine(" 4. StairCaseFarthest");
		}

		public enum Mode {
			None = 0,
			StairCaseDescend = 1,
			StairCaseAscend = 2,
			StairCaseClosest = 3,
			StairCaseFarthest = 4
		}

		Mode WhichMode = Mode.StairCaseDescend;
		int Passes = 1;
		int MaxIters = 100;
	}
}