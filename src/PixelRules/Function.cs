using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.PixelRules
{
	public class Function : IFAbstractFunction, IHasDistance, IFHasResampler
	{
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

		protected override IFAbstractProcessor CreateProcessor()
		{
			return new Processor { O = O };
		}

		public IMeasurer Measurer { get { return O.Measurer; }}
		public IFResampler Sampler { get { return O.Sampler; }}
		Options O = new Options();
	}

}