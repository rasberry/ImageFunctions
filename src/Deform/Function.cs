using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions.Deform
{
	public class Function : AbstractFunction, IHasResampler
	{
		protected override void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			proc.CenterPp = CenterPp;
			proc.CenterPx = CenterPx;
			proc.WhichMode = WhichMode;
			proc.Power = Power;
			proc.Sampler = Sampler ?? Registry.DefaultResampler;
			if (Rect.IsEmpty) {
				ctx.ApplyProcessor(proc);
			} else {
				ctx.ApplyProcessor(proc,Rect);
			}
		}

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
					CenterPx = new Point(cx,cy);
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
					CenterPp = new PointF((float)ppx,(float)ppy);
				}
				else if (curr == "-e" && ++a < len) {
					if (!OptionsHelpers.TryParse(args[a],out double power)) {
						Log.Error("Could not parse "+args[a]);
						return false;
					}
					Power = power;
				}
				else if (curr == "-m" && ++a < len) {
					Mode which;
					if (!OptionsHelpers.TryParse<Mode>(args[a],out which)) {
						Log.Error("unkown mode \""+args[a]+"\"");
						return false;
					}
					WhichMode = which;
				}
				else if (OptionsHelpers.HasSamplerArg(args,ref a)) {
					if (!OptionsHelpers.TryParseSampler(args,ref a,out IResampler sampler)) {
						return false;
					}
					Sampler = sampler;
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
			if (CenterPx == null && CenterPp == null) {
				CenterPp = new PointF(0.5f,0.5f);
			}

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Action.Deform);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" Warps an image using a mapping function");
			sb.AppendLine(" -cc (number) (number)       Coordinates of center in pixels");
			sb.AppendLine(" -cp (number)[%] (number)[%] Coordinates of center by proportion (default 50% 50%)");
			sb.AppendLine(" -e (number)                 (e) Power Exponent (default 2.0)");
			sb.AppendLine(" -m (mode)                   Choose mode (default Polynomial)");
			sb.SamplerHelpLine();
			sb.AppendLine();
			sb.AppendLine(" Available Modes");
			sb.AppendLine(" 1. Polynomial - x^e/w,y^e/h");
			sb.AppendLine(" 2. Inverted   - TODO");
		}

		public enum Mode {
			None = 0,
			Polynomial = 1,
			Inverted = 2
		}

		Point? CenterPx = null;
		PointF? CenterPp = null;
		double Power = 2.0;
		Mode WhichMode = Mode.Polynomial;
		public IResampler Sampler { get; set; } = null;
	}
}