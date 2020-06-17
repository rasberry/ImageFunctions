using System;
using System.Drawing;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.Swirl
{
	public class Function : IFAbstractFunction, IFHasResampler, IHasDistance
	{
		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			var pcx = p.Default("-cx",out int cx,out int cy);
			if (pcx.IsInvalid()) {
				return false;
			}
			else if (pcx.IsGood()) {
				O.CenterPx = new Point(cx,cy);
			}

			var pcp = p.Default("-cp",out double ppx,out double ppy,
				tpar: OptionsHelpers.ParseNumberPercent,
				upar: OptionsHelpers.ParseNumberPercent
			);
			if (pcp.IsInvalid()) {
				return false;
			}
			else if (pcp.IsGood()) {
				O.CenterPp = new PointF((float)ppx,(float)ppy);
			}
			//-cx and -cp are either/or options so set a default if neither are specified
			if (O.CenterPx == null && O.CenterPp == null) {
				O.CenterPp = new PointF(0.5f,0.5f);
			}

			if (p.Default("-rx",out O.RadiusPx).IsInvalid()) {
				return false;
			}
			if (p.Default("-rp",out O.RadiusPp,par:OptionsHelpers.ParseNumberPercent).IsInvalid()) {
				return false;
			}
			//-rx and -rp are either/or options so set a default if neither are specified
			if (O.RadiusPx == null && O.RadiusPp == null) {
				O.RadiusPp = 0.9;
			}

			if (p.Default("-s",out O.Rotations,0.9).IsInvalid()) {
				return false;
			}
			if (p.Has("-ccw").IsGood()) {
				O.CounterClockwise = true;
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
			string name = OptionsHelpers.FunctionName(Activity.Swirl);
			sb.WL();
			sb.WL(0,name + " [options] (input image) [output image]");
			sb.WL(1,"Smears pixels in a circle around a point");
			sb.WL(1,"-cx (number) (number)"      ,"Swirl center X and Y coordinate in pixels");
			sb.WL(1,"-cp (number)[%] (number)[%]","Swirl center X and Y coordinate proportionaly (default 50%,50%)");
			sb.WL(1,"-rx (number)"               ,"Swirl radius in pixels");
			sb.WL(1,"-rp (number)[%]"            ,"Swirl radius proportional to smallest image dimension (default 90%)");
			sb.WL(1,"-s  (number)[%]"            ,"Number of rotations (default 0.9)");
			sb.WL(1,"-ccw"                       ,"Rotate Counter-clockwise. (default is clockwise)");
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

			var pcx = p.Default("-cx",out int cx,out int cy);
			if (pcx.IsInvalid()) {
				return false;
			}
			else if (pcx.IsGood()) {
				O.CenterPx = new Point(cx,cy);
			}

			var pcp = p.Default("-cp",out double ppx,out double ppy,
				tpar: OptionsHelpers.ParseNumberPercent,
				upar: OptionsHelpers.ParseNumberPercent
			);
			if (pcp.IsInvalid()) {
				return false;
			}
			else if (pcp.IsGood()) {
				O.CenterPp = new PointF((float)ppx,(float)ppy);
			}
			//-cx and -cp are either/or options so set a default if neither are specified
			if (O.CenterPx == null && O.CenterPp == null) {
				O.CenterPp = new PointF(0.5f,0.5f);
			}

			if (p.Default("-rx",out O.RadiusPx).IsInvalid()) {
				return false;
			}
			if (p.Default("-rp",out O.RadiusPp,par:OptionsHelpers.ParseNumberPercent).IsInvalid()) {
				return false;
			}
			//-rx and -rp are either/or options so set a default if neither are specified
			if (O.RadiusPx == null && O.RadiusPp == null) {
				O.RadiusPp = 0.9;
			}

			if (p.Default("-s",out O.Rotations,0.9).IsInvalid()) {
				return false;
			}
			if (p.Has("-ccw").IsGood()) {
				O.CounterClockwise = true;
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
			string name = OptionsHelpers.FunctionName(Activity.Swirl);
			sb.WL();
			sb.WL(0,name + " [options] (input image) [output image]");
			sb.WL(1,"Smears pixels in a circle around a point");
			sb.WL(1,"-cx (number) (number)"      ,"Swirl center X and Y coordinate in pixels");
			sb.WL(1,"-cp (number)[%] (number)[%]","Swirl center X and Y coordinate proportionaly (default 50%,50%)");
			sb.WL(1,"-rx (number)"               ,"Swirl radius in pixels");
			sb.WL(1,"-rp (number)[%]"            ,"Swirl radius proportional to smallest image dimension (default 90%)");
			sb.WL(1,"-s  (number)[%]"            ,"Number of rotations (default 0.9)");
			sb.WL(1,"-ccw"                       ,"Rotate Counter-clockwise. (default is clockwise)");
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
