using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions.AreaSmoother
{
	public class Function : AbstractFunction, IHasResampler, IHasDistance
	{
		public override bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-t" && ++a < len) {
					if (!int.TryParse(args[a],out TotalTries)) {
						Log.Error("invalid number "+args[a]);
						return false;
					}
					if (TotalTries < 1) {
						Log.Error("-t number must be greater than zero");
						return false;
					}
				}
				else if (OptionsHelpers.HasSamplerArg(args,ref a)) {
					if (!OptionsHelpers.TryParseSampler(args,ref a,out IResampler sampler)) {
						return false;
					}
					Sampler = sampler;
				}
				else if (OptionsHelpers.HasMetricArg(args,ref a)) {
					if (!OptionsHelpers.TryParseMetric(args, ref a, out MetricFunction mf)) {
						return false;
					}
					Measurer = mf;
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

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Action.AreaSmoother);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" Blends adjacent areas of flat color together by sampling the nearest two colors to the area");
			sb.AppendLine(" -t (number)                 Number of times to run fit function (default 7)");
			sb.SamplerHelpLine();
			sb.MetricHelpLine();
		}

		protected override void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			proc.TotalTries = TotalTries;
			proc.Sampler = Sampler;
			proc.Measurer = Measurer;
			if (Rect.IsEmpty) {
				ctx.ApplyProcessor(proc);
			} else {
				ctx.ApplyProcessor(proc,Rect);
			}

		}

		public IResampler Sampler { get; set; } = null;
		public MetricFunction Measurer { get; set; } = null;
		int TotalTries = 7;
	}
}