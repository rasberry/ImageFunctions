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

namespace ImageFunctions.Deform
{
	public class Function : AbstractFunction, IHasResampler
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
			var pcp = p.Default("-cp",out double ppx, out double ppy,
				tpar:OptionsHelpers.ParseNumberPercent,
				upar:OptionsHelpers.ParseNumberPercent
			);
			if (pcp.IsInvalid()) {
				return false;
			}
			else if(pcp.IsGood()) {
				O.CenterPp = new PointF((float)ppx,(float)ppy);
			}

			var pcx = p.Default("-cx",out int cx, out int cy);
			if (pcx.IsInvalid()) {
				return false;
			}
			else if (pcx.IsGood()) {
				O.CenterPx = new Point(cx,cy);
			}

			// -cp and -cx are either/or options so choose a default if neither were specified
			if (O.CenterPx == null && O.CenterPp == null) {
				O.CenterPp = new PointF(0.5f,0.5f);
			}

			if (p.Default("-e",out O.Power,2.0).IsInvalid()) {
				return false;
			}
			if (p.Default("-m",out O.WhichMode,Mode.Polynomial).IsInvalid()) {
				return false;
			}
			if (p.DefaultSampler(out O.Sampler,Registry.DefaultResampler).IsInvalid()) {
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
			string name = OptionsHelpers.FunctionName(Activity.Deform);
			sb.WL();
			sb.WL(0,name + " [options] (input image) [output image]");
			sb.WL(1,"Warps an image using a mapping function");
			sb.WL(1,"-cc (number) (number)"      ,"Coordinates of center in pixels");
			sb.WL(1,"-cp (number)[%] (number)[%]","Coordinates of center by proportion (default 50% 50%)");
			sb.WL(1,"-e (number)"                ,"(e) Power Exponent (default 2.0)");
			sb.WL(1,"-m (mode)"                  ,"Choose mode (default Polynomial)");
			sb.SamplerHelpLine();
			sb.WL();
			sb.WL(1,"Available Modes");
			sb.WL(1,"1. Polynomial","x^e/w, y^e/h");
			sb.WL(1,"2. Inverted"  ,"n/x, n/y; n = (x^e + y^e)");
 		}

		public override void Main()
		{
			Main<RgbaD>();
		}

		Options O = new Options();
		public IResampler Sampler { get { return O.Sampler; }}
	}
}