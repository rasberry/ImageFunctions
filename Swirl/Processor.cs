using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System.Collections.Generic;

namespace ImageFunctions.Swirl
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		// https://stackoverflow.com/questions/30448045/how-do-you-add-a-swirl-to-an-image-image-distortion
		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			double swirlRadius = 0.9 * Math.Min(rect.Width,rect.Height);
			double swirlTwists = 0.9;

			var canvas = new Image<TPixel>(config,rect.Width,rect.Height);
			double swirlx = rect.Width / 2 + rect.Left;
			double swirly = rect.Height / 2 + rect.Top;

			Helpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
				int cy = y - rect.Top;
				int cx = x - rect.Left;
				TPixel nc = SwirlPixel(frame,x,y,swirlx,swirly,swirlRadius,swirlTwists);
				int coff = cy * rect.Width + cx;
				canvas.GetPixelSpan()[coff] = nc;
			});

			frame.BlitImage(canvas,rect);
		}

		static TPixel SwirlPixel(ImageFrame<TPixel> frame,
			double x, double y, double swirlx, double swirly, double swirlRadius, double swirlTwists)
		{
			double pixelx = x - swirlx;
			double pixely = y - swirly;
			double pixelDist = Math.Sqrt((pixelx * pixelx) + (pixely * pixely));
			double pixelAng = Math.Atan2(pixely,pixelx);

			double swirlAmount = 1.0 - (pixelDist / swirlRadius);
			if (swirlAmount > 0.0) {
				double twistAngle = swirlTwists * swirlAmount * Math.PI * 2;
				pixelAng += twistAngle;
				pixelx = Math.Cos(pixelAng) * pixelDist;
				pixely = Math.Sin(pixelAng) * pixelDist;
			}
			int samplex = Math.Clamp((int)(swirlx + pixelx),0,frame.Width - 1);
			int sampley = Math.Clamp((int)(swirly + pixely),0,frame.Height - 1);
			var c = frame.GetPixelRowSpan(sampley)[samplex];
			return c;
		}
	}
}