using System;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions.ZoomBlur
{
	public class Function : AbstractFunction, IHasResampler
	{
		public override bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-z" && ++a < len) {
					if (!double.TryParse(args[a],out ZoomAmount)) {
						Log.Error("invalid number "+args[a]);
						return false;
					}
					if (ZoomAmount < 0.0 || double.IsInfinity(ZoomAmount) || double.IsNaN(ZoomAmount)) {
						Log.Error("zoom amount "+ZoomAmount+" is invalid");
						return false;
					}
				}
				else if (curr == "-cc" && (a+=2) < len) {
					if (!int.TryParse(args[a-1], out int px)) {
						Log.Error("invalid number "+args[a-1]);
						return false;
					}
					if (!int.TryParse(args[a], out int py)) {
						Log.Error("invalid number "+args[a]);
						return false;
					}
					CenterPx = new Point(px,py);
				}
				else if (curr == "-cp" && (a+=2) < len) {
					if (!Helpers.ParseNumberPercent(args[a-1], out double px)) {
						return false;
					}
					if (!Helpers.ParseNumberPercent(args[a], out double py)) {
						return false;
					}
					CenterRt = new PointF((float)px,(float)py);
				}
				else if (Options.HasSamplerArg(args,ref a)) {
					if (!Options.TryParseSampler(args,ref a,out IResampler sampler)) {
						return false;
					}
					Sampler = sampler;
				}
				else if (InImage == null) {
					InImage = curr;
				}
				else if (OutImage == null) {
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
				OutImage = Helpers.CreateOutputFileName(InImage);
			}
			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = Helpers.FunctionName(Action.ZoomBlur);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" Blends rays of pixels to produce a 'zoom' effect");
			sb.AppendLine(" -z  (number)[%]             Zoom amount (default 1.1)");
			sb.AppendLine(" -cc (number) (number)       Coordinates of zoom center in pixels");
			sb.AppendLine(" -cp (number)[%] (number)[%] Coordinates of zoom center by proportion (default 50% 50%)");
			sb.SamplerHelpLine();
		}

		protected override void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			proc.ZoomAmount = ZoomAmount;
			proc.CenterPx = CenterPx;
			proc.CenterRt = CenterRt;
			proc.Sampler = Sampler;
			if (Rect.IsEmpty) {
				ctx.ApplyProcessor(proc);
			} else {
				ctx.ApplyProcessor(proc,Rect);
			}

		}

		public IResampler Sampler { get; set; } = null;
		Point? CenterPx = null;
		PointF? CenterRt = null;
		double ZoomAmount = 1.1;
	}
}
