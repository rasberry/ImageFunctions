using System;
using System.Drawing;
using ImageFunctions.Helpers;

namespace ImageFunctions.Swirl
{
	public class Processor : IFAbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			double swirlRadius;
			double swirlTwists = O.Rotations;
			double swirlx, swirly;
			Rectangle rect = this.Bounds;
			var Iis = Engines.Engine.GetConfig();

			if (O.RadiusPx != null) {
				swirlRadius = O.RadiusPx.Value;
			}
			else {
				double m = O.RadiusPp != null ? O.RadiusPp.Value : 0.9;
				swirlRadius = m * Math.Min(rect.Width,rect.Height);
			}

			if (O.CenterPx != null) {
				swirlx = O.CenterPx.Value.X;
				swirly = O.CenterPx.Value.Y;
			}
			else {
				double px = O.CenterPp != null ? O.CenterPp.Value.X : 0.5;
				double py = O.CenterPp != null ? O.CenterPp.Value.Y : 0.5;
				swirlx = rect.Width * px + rect.Left;
				swirly = rect.Height * py + rect.Top;
			}

			using (var progress = new ProgressBar())
			using (var canvas = Iis.NewImage(rect.Width,rect.Height))
			{
				MoreHelpers.ThreadPixels(rect, MaxDegreeOfParallelism, (x,y) => {
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					IFColor nc = SwirlPixel(Source,x,y,swirlx,swirly,swirlRadius,swirlTwists);
					canvas[cx,cy] = nc;
				},progress);

				Source.BlitImage(canvas,rect);
			}
		}

		IFColor SwirlPixel(IFImage frame,
			double x, double y, double swirlx, double swirly,
			double swirlRadius, double swirlTwists)
		{
			double pixelx = x - swirlx;
			double pixely = y - swirly;
			//double pixelDist = Math.Sqrt((pixelx * pixelx) + (pixely * pixely));
			double pixelDist = O.Measurer.Measure(x,y,swirlx,swirly);
			double swirlAmount = 1.0 - (pixelDist / swirlRadius);

			if (swirlAmount > 0.0) {
				double twistAngle = swirlTwists * swirlAmount * Math.PI * 2;
				if (!O.CounterClockwise) { twistAngle *= -1.0; }
				double pixelAng = Math.Atan2(pixely,pixelx) + twistAngle;
				pixelx = Math.Cos(pixelAng) * pixelDist;
				pixely = Math.Sin(pixelAng) * pixelDist;
			}

			var c = ImageHelpers.GetPixelSafe(frame,
				(int)(swirlx + pixelx),(int)(swirly + pixely));
			//var c = frame.Sample(swirlx + pixelx,swirly + pixely,O.Sampler);
			//TODO samplers need to get fixed
			return c;
		}

		public override void Dispose() {}
	}

	#if false
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Options O = null;

		// https://stackoverflow.com/questions/30448045/how-do-you-add-a-swirl-to-an-image-image-distortion
		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			double swirlRadius;
			double swirlTwists = O.Rotations;
			double swirlx, swirly;

			if (O.RadiusPx != null) {
				swirlRadius = O.RadiusPx.Value;
			}
			else {
				double m = O.RadiusPp != null ? O.RadiusPp.Value : 0.9;
				swirlRadius = m * Math.Min(rect.Width,rect.Height);
			}

			if (O.CenterPx != null) {
				swirlx = O.CenterPx.Value.X;
				swirly = O.CenterPx.Value.Y;
			}
			else {
				double px = O.CenterPp != null ? O.CenterPp.Value.X : 0.5;
				double py = O.CenterPp != null ? O.CenterPp.Value.Y : 0.5;
				swirlx = rect.Width * px + rect.Left;
				swirly = rect.Height * py + rect.Top;
			}

			using (var progress = new ProgressBar())
			using (var canvas = new Image<TPixel>(config,rect.Width,rect.Height))
			{
				MoreHelpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					TPixel nc = SwirlPixel(frame,x,y,swirlx,swirly,swirlRadius,swirlTwists);
					int coff = cy * rect.Width + cx;
					canvas.GetPixelSpan()[coff] = nc;
				},progress);

				frame.BlitImage(canvas.Frames.RootFrame,rect);
			}
		}

		TPixel SwirlPixel(ImageFrame<TPixel> frame,
			double x, double y, double swirlx, double swirly, double swirlRadius, double swirlTwists)
		{
			double pixelx = x - swirlx;
			double pixely = y - swirly;
			//double pixelDist = Math.Sqrt((pixelx * pixelx) + (pixely * pixely));
			double pixelDist = O.Measurer.Measure(x,y,swirlx,swirly);
			double swirlAmount = 1.0 - (pixelDist / swirlRadius);

			if (swirlAmount > 0.0) {
				double twistAngle = swirlTwists * swirlAmount * Math.PI * 2;
				if (!O.CounterClockwise) { twistAngle *= -1.0; }
				double pixelAng = Math.Atan2(pixely,pixelx) + twistAngle;
				pixelx = Math.Cos(pixelAng) * pixelDist;
				pixely = Math.Sin(pixelAng) * pixelDist;
			}

			var c = frame.Sample(swirlx + pixelx,swirly + pixely,O.Sampler);
			return c;
		}
	}
	#endif
}