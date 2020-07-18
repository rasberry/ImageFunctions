using System;
using System.Collections.Generic;
using System.Drawing;
using ImageFunctions.Helpers;

namespace ImageFunctions.ZoomBlur
{
	public class Processor : AbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			Iic = Registry.GetImageEngine();
			using (var progress = new ProgressBar())
			using (var canvas = Iic.NewImage(Bounds.Width,Bounds.Height)) {
				double w2 = Bounds.Width / 2.0;
				double h2 = Bounds.Height / 2.0;

				if (O.CenterPx.HasValue) {
					w2 = O.CenterPx.Value.X;
					h2 = O.CenterPx.Value.Y;
				}
				else if (O.CenterRt.HasValue) {
					w2 = Bounds.Width * O.CenterRt.Value.X;
					h2 = Bounds.Height * O.CenterRt.Value.Y;
				}

				MoreHelpers.ThreadPixels(Bounds, MaxDegreeOfParallelism, (x,y) => {
					var d = Source[x,y];
					//Log.Debug($"pixel1 [{x},{y}] = ({d.R} {d.G} {d.B} {d.A})");
					IColor nc = ZoomPixel(Source,Bounds,x,y,w2,h2);
					int cy = y - Bounds.Top;
					int cx = x - Bounds.Left;
					//Log.Debug($"pixel2 [{cx},{cy}] = ({nc.R} {nc.G} {nc.B} {nc.A})");
					canvas[cx,cy] = nc;
				},progress);

				Source.BlitImage(canvas,Bounds);
			}
		}

		IColor ZoomPixel(IImage frame, Rectangle rect, int x, int y,double cx, double cy)
		{
			var sampler = O.Sampler;
			double dist = O.Measurer.Measure(x,y,cx,cy);
			int idist = (int)Math.Ceiling(dist);

			List<IColor> vector = new List<IColor>(idist);
			double ang = Math.Atan2(y - cy, x - cx);
			double sd = dist;
			double ed = dist * O.ZoomAmount;

			for (double d = sd; d < ed; d++)
			{
				double px = Math.Cos(ang) * d + cx;
				double py = Math.Sin(ang) * d + cy;
				IColor c = sampler.GetSample(frame,(int)px,(int)py);
				vector.Add(c);
			}

			IColor avg;
			int count = vector.Count;
			if (count < 1) {
				avg  = sampler.GetSample(frame,x,y);
			}
			else if (count == 1) {
				avg = vector[0];
			}
			else {
				double cr = 0, cg = 0, cb = 0, ca = 0;
				foreach (IColor tpc in vector)
				{
					cr += tpc.R; cg += tpc.G; cb += tpc.B;
					ca += tpc.A;
				}
				avg = new IColor(
					cr / count,
					cg / count,
					cb / count,
					ca / count
				);
			}
			return avg;
		}

		public override void Dispose() {}
		IImageEngine Iic = null;
	}

}
