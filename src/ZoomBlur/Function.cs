using System;
using System.Drawing;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.ZoomBlur
{
	public class Function : IFAbstractFunction
	{
		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.Default("-z",out O.ZoomAmount,1.1)
				.BeGreaterThanZero("-z",O.ZoomAmount,true).IsInvalid()) {
				return false;
			}
			var pcc = p.Default("-cc",out int cx, out int cy);
			if (pcc.IsInvalid()) {
				return false;
			}
			else if (pcc.IsGood()) {
				O.CenterPx = new Point(cx,cy);
			}
			var pcp = p.Default("-cp",out double px, out double py,
				tpar: OptionsHelpers.ParseNumberPercent,
				upar: OptionsHelpers.ParseNumberPercent
			);
			if (pcp.IsInvalid()) {
				return false;
			}
			else if (pcp.IsGood()) {
				O.CenterRt = new PointF((float)px,(float)py);
			}

			//-cc / -cp are either/or options. if neither are specified set the default
			if (O.CenterPx == null && O.CenterRt == null) {
				O.CenterRt = new PointF(0.5f,0.5f);
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
			string name = OptionsHelpers.FunctionName(Activity.ZoomBlur);
			sb.WL();
			sb.WL(0,name + " [options] (input image) [output image]");
			sb.WL(1,"Blends rays of pixels to produce a 'zoom' effect");
			sb.WL(1,"-z  (number)[%]"             ,"Zoom amount (default 1.1)");
			sb.WL(1,"-cc (number) (number)"       ,"Coordinates of zoom center in pixels");
			sb.WL(1,"-cp (number)[%] (number)[%]" ,"Coordinates of zoom center by proportion (default 50% 50%)");
			//sb.WL(" -oh"                        ,"Only zoom horizontally");
			//sb.WL(" -ov"                        ,"Only zoom vertically");
			sb.SamplerHelpLine();
			sb.MetricHelpLine();
		}

		protected override IFAbstractProcessor CreateProcessor()
		{
			var proc = new Processor();
			proc.O = O;
			return proc;
		}

		Options O = new Options();
	}

	#if false
	public class Function : AbstractFunction, IHasResampler, IHasDistance
	{
		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.Default("-z",out O.ZoomAmount,1.1)
				.BeGreaterThanZero("-z",O.ZoomAmount,true).IsInvalid()) {
				return false;
			}
			var pcc = p.Default("-cc",out int cx, out int cy);
			if (pcc.IsInvalid()) {
				return false;
			}
			else if (pcc.IsGood()) {
				O.CenterPx = new Point(cx,cy);
			}
			var pcp = p.Default("-cp",out double px, out double py,
				tpar: OptionsHelpers.ParseNumberPercent,
				upar: OptionsHelpers.ParseNumberPercent
			);
			if (pcp.IsInvalid()) {
				return false;
			}
			else if (pcp.IsGood()) {
				O.CenterRt = new PointF((float)px,(float)py);
			}

			//-cc / -cp are either/or options. if neither are specified set the default
			if (O.CenterPx == null && O.CenterRt == null) {
				O.CenterRt = new PointF(0.5f,0.5f);
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
			string name = OptionsHelpers.FunctionName(Activity.ZoomBlur);
			sb.WL();
			sb.WL(0,name + " [options] (input image) [output image]");
			sb.WL(1,"Blends rays of pixels to produce a 'zoom' effect");
			sb.WL(1,"-z  (number)[%]"             ,"Zoom amount (default 1.1)");
			sb.WL(1,"-cc (number) (number)"       ,"Coordinates of zoom center in pixels");
			sb.WL(1,"-cp (number)[%] (number)[%]" ,"Coordinates of zoom center by proportion (default 50% 50%)");
			//sb.WL(" -oh"                        ,"Only zoom horizontally");
			//sb.WL(" -ov"                        ,"Only zoom vertically");
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

		public IResampler Sampler { get { return O.Sampler; }}
		public IMeasurer Measurer { get { return O.Measurer; }}
		Options O = new Options();
	}
	#endif
}
