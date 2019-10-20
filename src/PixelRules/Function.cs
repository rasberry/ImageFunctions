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
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions.PixelRules
{
	public class Function : AbstractFunction, IHasDistance, IHasResampler
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
				if (curr == "-n" && ++a<len) {
					if (!Helpers.OptionsHelpers.TryParse(args[a],out O.Passes)) {
						Log.Error("invalid passes");
						return false;
					}
					if (O.Passes < 1) {
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
					O.WhichMode = which;
				}
				else if (curr == "-x" && ++a<len) {
					if (!Helpers.OptionsHelpers.TryParse(args[a],out O.MaxIters)) {
						Log.Error("invalid max iterations");
						return false;
					}
					if (O.MaxIters < 1) {
						Log.Error("max iterations must be greater than zero");
						return false;
					}
				}
				else if (OptionsHelpers.HasSamplerArg(args,ref a)) {
					if (!OptionsHelpers.TryParseSampler(args,ref a,out IResampler sampler)) {
						return false;
					}
					O.Sampler = sampler;
				}
				else if (OptionsHelpers.HasMetricArg(args,ref a)) {
					if (!OptionsHelpers.TryParseMetric(args, ref a, out IMeasurer mf)) {
						return false;
					}
					O.Measurer = mf;
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
			string name = OptionsHelpers.FunctionName(Activity.PixelRules);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" Average a set of pixels by following a minimaztion function");
			sb.AppendLine(" -m (mode)                   Which mode to use (default StairCaseDescend)");
			sb.AppendLine(" -n (number)                 Number of times to apply operation (default 1)");
			sb.AppendLine(" -x (number)                 Maximum number of iterations - in case of infinte loops (default 100)");
			sb.SamplerHelpLine();
			sb.MetricHelpLine();
			sb.AppendLine();
			sb.AppendLine(" Available Modes");
			sb.AppendLine(" 1. StairCaseDescend         move towards smallest distance");
			sb.AppendLine(" 2. StairCaseAscend          move towards largest distance");
			sb.AppendLine(" 3. StairCaseClosest         move towards closest distance");
			sb.AppendLine(" 4. StairCaseFarthest        move towards farthest distance");
		}

		public IMeasurer Measurer { get { return O.Measurer; }}
		public IResampler Sampler { get { return O.Sampler; }}

		public enum Mode {
			None = 0,
			StairCaseDescend = 1,
			StairCaseAscend = 2,
			StairCaseClosest = 3,
			StairCaseFarthest = 4
		}

		Options O = new Options();
	}
}