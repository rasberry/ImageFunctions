using System;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ImageFunctions.Projection
{
	public class Function : AbstractFunction
	{
		protected override void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			proc.CenterPp = CenterPp;
			proc.CenterPx = CenterPx;
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
				if (false) {
				}
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
			string name = OptionsHelpers.FunctionName(Action.Projection);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" TODO");
			sb.AppendLine(" -cc (number) (number)       Coordinates of center in pixels");
			sb.AppendLine(" -cp (number)[%] (number)[%] Coordinates of center by proportion (default 50% 50%)");
		}

		Point? CenterPx = null;
		PointF? CenterPp = null;
	}
}