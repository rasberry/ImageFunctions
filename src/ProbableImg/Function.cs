using System;
using System.Drawing;
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
			if (p.Default("-n",out O.TotalNodes,null).IsInvalid()) {
				return false;
			}

			while(true) {
				var pcp = p.Default("-pp",out double ppx, out double ppy,
					tpar:OptionsHelpers.ParseNumberPercent,
					upar:OptionsHelpers.ParseNumberPercent
				);
				if (pcp.IsMissing()) { break; }
				if (pcp.IsInvalid()) {
					return false;
				}
				else if(pcp.IsGood()) {
					O.StartLoc.Add(StartPoint.FromPro(ppx,ppy));
				}
			}
			while(true) {
				var pcx = p.Default("-xy",out int cx, out int cy);
				if (pcx.IsMissing()) { break; }
				if (pcx.IsInvalid()) {
					return false;
				}
				else if (pcx.IsGood()) {
					O.StartLoc.Add(StartPoint.FromLinear(cx,cy));
				}
			}
			var orect = p.Default("-o#",out Rectangle rect);
			if (orect.IsInvalid()) {
				return false;
			}
			else if (orect.IsGood()) {
				O.OutBounds = rect;
			}
			if (p.ExpectFile(out InImage,"input image").IsBad()) {
				return false;
			}
			if (p.DefaultFile(out OutImage,InImage).IsInvalid()) {
				return false;
			}

			if (O.TotalNodes != null && O.TotalNodes < 1) {
				Tell.MustBeGreaterThanZero("-n");
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
			sb.WL(1,"-n (number)"                ,"Max Number of start nodes (defaults to 1 or number of -pp/-xy options)");
			sb.WL(1,"-rs (seed)"                 ,"Options number seed for the random number generator");
			sb.WL(1,"-xy (number) (number)"      ,"Add a start node (in pixels) - multiple allowed");
			sb.WL(1,"-pp (number)[%] (number)[%]","Add a start node (by proportion) - multiple allowed");
			sb.WL(1,"-o# (w,h)"                  ,"Set the output image size (defaults to input image size)");
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