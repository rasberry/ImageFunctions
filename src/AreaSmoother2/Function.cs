using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.AreaSmoother2
{
	public class Function : AbstractFunction
	{
		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);
			if (p.Has("-H").IsGood()) {
				O.HOnly = true;
			}
			if (p.Has("-V").IsGood()) {
				O.VOnly = true;
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
			string name = OptionsHelpers.FunctionName(Activity.AreaSmoother2);
			sb.WL();
			sb.WL(0,name + " [options] (input image) [output image]");
			sb.WL(1,"Blends adjacent areas of flat color together by blending horizontal and vertical gradients");
			sb.WL(1,"-H","Horizontal only");
			sb.WL(1,"-V","Vertical only");
		}

		protected override AbstractProcessor CreateProcessor()
		{
			return new Processor { O = O };
		}

		Options O = new Options();
	}

}
