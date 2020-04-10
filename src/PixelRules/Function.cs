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
			proc.Bounds = sourceRectangle;
			return proc;
		}

		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.Default("-n",out O.Passes,1)
				.BeGreaterThanZero("-n",O.Passes).IsInvalid()) {
				return false;
			}
			if (p.Default("-m",out O.WhichMode,Function.Mode.StairCaseDescend).IsInvalid()) {
				return false;
			}
			if (p.Default("-x",out O.MaxIters,100)
				.BeGreaterThanZero("-x",O.MaxIters).IsInvalid()) {
				return false;
			}
			if (p.DefaultSampler(out O.Sampler).IsInvalid()) {
				return false;
			}
			if (p.DefaultMetric(out O.Measurer).IsInvalid()) {
				return false;
			}

			if (p.ExpectFile(out InImage,"input image").IsBad()) {
				return false;
			}
			if (p.DefaultFile(out OutImage,InImage).IsInvalid()) {
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
		#endif

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.PixelRules);
			sb.WL();
			sb.WL(0,name + " [options] (input image) [output image]");
			sb.WL(1,"Average a set of pixels by following a minimaztion function");
			sb.WL(1,"-m (mode)"  ,"Which mode to use (default StairCaseDescend)");
			sb.WL(1,"-n (number)","Number of times to apply operation (default 1)");
			sb.WL(1,"-x (number)","Maximum number of iterations - in case of infinte loops (default 100)");
			sb.SamplerHelpLine();
			sb.MetricHelpLine();
			sb.WL();
			sb.WL(1,"Available Modes");
			sb.PrintEnum<Mode>(1,ModeDesc);
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

		static string ModeDesc(Mode m)
		{
			switch(m)
			{
			case Mode.StairCaseDescend:  return "move towards smallest distance";
			case Mode.StairCaseAscend:   return "move towards largest distance";
			case Mode.StairCaseClosest:  return "move towards closest distance";
			case Mode.StairCaseFarthest: return "move towards farthest distance";
			}
			return "";
		}

		public override void Main()
		{
			Main<RgbaD>();
		}

		Options O = new Options();
	}
}