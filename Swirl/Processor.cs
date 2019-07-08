using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System.Collections.Generic;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace ImageFunctions.Swirl
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Point? CenterPx = null;
		public PointF? CenterPp = null;
		public int? RadiusPx = null;
		public double? RadiusPp = null;
		public double Rotations = 0.9;
		public bool CounterClockwise = false;
		public IResampler Sampler = null;

		// https://stackoverflow.com/questions/30448045/how-do-you-add-a-swirl-to-an-image-image-distortion
		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			double swirlRadius;
			double swirlTwists = Rotations;
			double swirlx, swirly;

			if (RadiusPx != null) {
				swirlRadius = RadiusPx.Value;
			}
			else {
				double m = RadiusPp != null ? RadiusPp.Value : 0.9;
				swirlRadius = m * Math.Min(rect.Width,rect.Height);
			}

			if (CenterPx != null) {
				swirlx = CenterPx.Value.X;
				swirly = CenterPx.Value.Y;
			}
			else {
				double px = CenterPp != null ? CenterPp.Value.X : 0.5;
				double py = CenterPp != null ? CenterPp.Value.Y : 0.5;
				swirlx = rect.Width * px + rect.Left;
				swirly = rect.Height * py + rect.Top;
			}

			var canvas = new Image<TPixel>(config,rect.Width,rect.Height);

			Helpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
				int cy = y - rect.Top;
				int cx = x - rect.Left;
				TPixel nc = SwirlPixel(frame,x,y,swirlx,swirly,swirlRadius,swirlTwists);
				int coff = cy * rect.Width + cx;
				canvas.GetPixelSpan()[coff] = nc;
			});

			frame.BlitImage(canvas,rect);
		}

		TPixel SwirlPixel(ImageFrame<TPixel> frame,
			double x, double y, double swirlx, double swirly, double swirlRadius, double swirlTwists)
		{
			double pixelx = x - swirlx;
			double pixely = y - swirly;
			double pixelDist = Math.Sqrt((pixelx * pixelx) + (pixely * pixely));

			double swirlAmount = 1.0 - (pixelDist / swirlRadius);

			if (swirlAmount > 0.0) {
				double twistAngle = swirlTwists * swirlAmount * Math.PI * 2;
				if (!CounterClockwise) { twistAngle *= -1.0; }
				double pixelAng = Math.Atan2(pixely,pixelx) + twistAngle;
				pixelx = Math.Cos(pixelAng) * pixelDist;
				pixely = Math.Sin(pixelAng) * pixelDist;
			}

			var c = frame.Sample(swirlx + pixelx,swirly + pixely,Sampler);
			return c;
		}
	}
}