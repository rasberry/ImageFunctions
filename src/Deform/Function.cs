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

			var ppw = p.Default("-e",out int power);
			if (ppw.IsInvalid()) {
				return false;
			}
			else if (ppw.IsGood()) {
				O.Power = power;
			}

			var ppm = p.Default("-m",out Mode mode,Mode.None);
			if (ppm.IsInvalid()) {
				return false;
			}
			else if (ppm.IsGood()) {
				O.WhichMode = mode;
			}

			var psam = p.DefaultSampler(out IResampler sampler);
			if (psam.IsInvalid()) {
				return false;
			}
			else if(psam.IsGood()) {
				O.Sampler = sampler;
			}

			if (p.Expect(out InImage,"input image").IsBad()) {
				return false;
			}
			if (p.Default(out OutImage).IsBad()) {
				OutImage = OptionsHelpers.CreateOutputFileName(InImage);
			}

			if (!File.Exists(InImage)) {
				Tell.CannotFindFile(InImage);
				return false;
			}

			if (O.CenterPx == null && O.CenterPp == null) {
				O.CenterPp = new PointF(0.5f,0.5f);
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
				if (curr == "-cx" && (a+=2) < len) {
					if (!int.TryParse(args[a-1],out int cx)) {
						Log.Error("Could not parse "+args[a-1]);
						return false;
					}
					if (!int.TryParse(args[a],out int cy)) {
						Log.Error("Could not parse "+args[a]);
						return false;
					}
					O.CenterPx = new Point(cx,cy);
				}
				else if (curr == "-cp" && (a+=2) < len) {
					if (!OptionsHelpers.ParseNumberPercent(args[a-1],out double ppx)) {
						Log.Error("Could not parse "+args[a-1]);
						return false;
					}
					if (!OptionsHelpers.ParseNumberPercent(args[a],out double ppy)) {
						Log.Error("Could not parse "+args[a]);
						return false;
					}
					O.CenterPp = new PointF((float)ppx,(float)ppy);
				}
				else if (curr == "-e" && ++a < len) {
					if (!OptionsHelpers.TryParse(args[a],out double power)) {
						Log.Error("Could not parse "+args[a]);
						return false;
					}
					O.Power = power;
				}
				else if (curr == "-m" && ++a < len) {
					Mode which;
					if (!OptionsHelpers.TryParse<Mode>(args[a],out which)) {
						Log.Error("unkown mode \""+args[a]+"\"");
						return false;
					}
					O.WhichMode = which;
				}
				else if (OptionsHelpers.HasSamplerArg(args,ref a)) {
					if (!OptionsHelpers.TryParseSampler(args,ref a,out IResampler sampler)) {
						return false;
					}
					O.Sampler = sampler;
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
			if (O.CenterPx == null && O.CenterPp == null) {
				O.CenterPp = new PointF(0.5f,0.5f);
			}

			return true;
		}
		#endif

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