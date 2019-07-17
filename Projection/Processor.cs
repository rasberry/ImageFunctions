using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;

namespace ImageFunctions.Projection
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			var canvas = new Image<TPixel>(config,rect.Width,rect.Height);
			double r = Math.Min(frame.Width,frame.Height);

			Helpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
				int cy = y - rect.Top;
				int cx = x - rect.Left;
				TPixel nc = ProjectPixel(frame,x,y,r);
				int coff = cy * rect.Width + cx;
				canvas.GetPixelSpan()[coff] = nc;
			});

			frame.BlitImage(canvas,rect);

			Log.Debug("ppxmin = "+ppxmin);
			Log.Debug("ppxmax = "+ppxmax);
			Log.Debug("ppymin = "+ppymin);
			Log.Debug("ppymax = "+ppymax);
		}

		TPixel ProjectPixel(ImageFrame<TPixel> frame,double x, double y,double r)
		{
			int fw = frame.Width / 2;
			int fh = frame.Height / 2;
			x -= fw; y -= fh;

			#if false
			double exp = 2.0;
			double den = Math.Pow(Math.Abs(x),exp) + Math.Pow(Math.Abs(y),exp);
			double px = 1.0 * den / x;
			double py = 1.0 * den / y;
			#else
			//TODO to be able to change the center we would split the image into
			// 4 quadrants and do the calculation as if each part is a seperate image
			// with it's own width and height
			// TODO also need to include rectangle.. so maybe just do the calc 4 times
			// with different rectangle quadrants
			double exp = 2.0;
			//solve(w^q/n = w,n) : n = w^(q-1)
			double dx = Math.Pow(Math.Abs(fw),exp - 1);
			double dy = Math.Pow(Math.Abs(fh),exp - 1);
			double px = Math.Sign(x) * Math.Pow(Math.Abs(x),exp) / dx;
			double py = Math.Sign(y) * Math.Pow(Math.Abs(y),exp) / dy;

			#endif

			//Log.Debug("px = "+px+" py = "+py);
			if (px < ppxmin) { ppxmin = px; }
			if (px > ppxmax) { ppxmax = px; }
			if (py < ppymin) { ppymin = py; }
			if (py > ppymax) { ppymax = py; }

			px += fw; py += fh;

			return ImageHelpers.Sample(frame,px,py);
		}
		double ppxmin = double.MaxValue;
		double ppxmax = double.MinValue;
		double ppymin = double.MaxValue;
		double ppymax = double.MinValue;
	}
}
