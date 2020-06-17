using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.AreaSmoother
{
	public class Function : IFAbstractFunction, IFHasResampler, IHasDistance
	{
		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);
			if (p.Default("-t",out O.TotalTries,7)
				.BeGreaterThanZero("-t",O.TotalTries).IsInvalid()) {
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
			if (p.DefaultFile(out OutImage,InImage).IsBad()) {
				return false;
			}

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.AreaSmoother);
			sb.WL();
			sb.WL(0,name + " [options] (input image) [output image]");
			sb.WL(1,"Blends adjacent areas of flat color together by sampling the nearest two colors to the area");
			sb.WL(1,"-t (number)","Number of times to run fit function (default 7)");
			sb.SamplerHelpLine();
			sb.MetricHelpLine();
		}

		protected override IFAbstractProcessor CreateProcessor()
		{
			return new Processor();
		}

		Options O = new Options();
		public IFResampler Sampler { get { return O.Sampler; }}
		public IMeasurer Measurer { get { return O.Measurer; }}
	}

	#if false
	public class Function : AbstractFunction, IHasResampler, IHasDistance
	{
		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);
			if (p.Default("-t",out O.TotalTries,7)
				.BeGreaterThanZero("-t",O.TotalTries).IsInvalid()) {
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
			if (p.DefaultFile(out OutImage,InImage).IsBad()) {
				return false;
			}

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.AreaSmoother);
			sb.WL();
			sb.WL(0,name + " [options] (input image) [output image]");
			sb.WL(1,"Blends adjacent areas of flat color together by sampling the nearest two colors to the area");
			sb.WL(1,"-t (number)","Number of times to run fit function (default 7)");
			sb.SamplerHelpLine();
			sb.MetricHelpLine();
		}

		public override IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle)
		{
			var proc = new Processor<TPixel>();
			proc.O = O;
			proc.Source = source;
			proc.Bounds = sourceRectangle;
			return proc;
		}

		public override void Main()
		{
			Main<RgbaD>();
		}

		Options O = new Options();
		public IResampler Sampler { get { return O.Sampler; }}
		public IMeasurer Measurer { get { return O.Measurer; }}
	}
	#endif
}
