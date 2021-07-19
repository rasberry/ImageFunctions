using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.ProbableImg
{
	public class Function : AbstractFunction
	{
		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);
			if (p.Default("-rs",out O.RandomSeed,null).IsInvalid()) {
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
			string name = OptionsHelpers.FunctionName(Activity.ProbableImg);
			sb.WL();
			sb.WL(0,name + " [options] (input image) [output image]");
			sb.WL(1,"Generate a new image using a probability profile based on the input image");
			sb.WL(1,"-rs (seed)"  ,"Options number seed for the random number generator");
			sb.WL();
		}

		protected override AbstractProcessor CreateProcessor()
		{
			return new Processor { O = O };
		}

		//public IMeasurer Measurer { get { return O.Measurer; }}
		//public ISampler Sampler { get { return O.Sampler; }}
		Options O = new Options();
	}

}