using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions.ZoomBlur
{
	public class Function : AbstractFunction, IHasResampler, IHasDistance
	{
		public override bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-z" && ++a < len) {
					if (!double.TryParse(args[a],out O.ZoomAmount)) {
						Log.Error("invalid number "+args[a]);
						return false;
					}
					if (O.ZoomAmount < 0.0 || double.IsInfinity(O.ZoomAmount) || double.IsNaN(O.ZoomAmount)) {
						Log.Error("zoom amount "+O.ZoomAmount+" is invalid");
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
					O.CenterPx = new Point(px,py);
				}
				else if (curr == "-cp" && (a+=2) < len) {
					if (!OptionsHelpers.ParseNumberPercent(args[a-1], out double px)) {
						return false;
					}
					if (!OptionsHelpers.ParseNumberPercent(args[a], out double py)) {
						return false;
					}
					O.CenterRt = new PointF((float)px,(float)py);
				}
				else if (OptionsHelpers.HasSamplerArg(args,ref a)) {
					if (!OptionsHelpers.TryParseSampler(args,ref a,out IResampler sampler)) {
						return false;
					}
					O.Sampler = sampler;
				}
				else if (OptionsHelpers.HasMetricArg(args,ref a)) {
					if (!OptionsHelpers.TryParseMetric(args, ref a, out IMeasurer mf)) {
						return false;
					}
					O.Measurer = mf;
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
				OutImage = OptionsHelpers.CreateOutputFileName(InImage);
			}
			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.ZoomBlur);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" Blends rays of pixels to produce a 'zoom' effect");
			sb.AppendLine(" -z  (number)[%]             Zoom amount (default 1.1)");
			sb.AppendLine(" -cc (number) (number)       Coordinates of zoom center in pixels");
			sb.AppendLine(" -cp (number)[%] (number)[%] Coordinates of zoom center by proportion (default 50% 50%)");
			//sb.AppendLine(" -oh                         Only zoom horizontally");
			//sb.AppendLine(" -ov                         Only zoom vertically");
			sb.SamplerHelpLine();
			sb.MetricHelpLine();
		}

		public override IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle)
		{
			var proc = new Processor<TPixel>();
			proc.O = O;
			proc.Source = source;
			proc.SourceRectangle = sourceRectangle;
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
}
