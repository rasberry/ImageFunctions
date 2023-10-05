using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.Derivatives
{
	public class Function : AbstractFunction
	{
		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.Has("-g").IsGood()) {
				O.DoGrayscale = true;
			}
			if (p.Has("-a").IsGood()) {
				O.UseABS = true;
			}

			if (p.Expect(out InImage,"input image").IsBad()) {
				return false;
			}
			if (p.DefaultFile(out OutImage,InImage).IsBad()) {
				return false;
			}

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.Derivatives);
			sb.WL();
			sb.WL(0,name+" [options] (input image) [output image]");
			sb.WL(1,"Computes the color change rate - similar to edge detection");
			sb.WL(1,"-g","Grayscale output");
			sb.WL(1,"-a","Calculate absolute value difference");
		}

		protected override AbstractProcessor CreateProcessor()
		{
			return new Processor { O = O };
		}

		Options O = new Options();
	}
}
