using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace ImageFunctions.Deform
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Point? CenterPx = null;
		public PointF? CenterPp = null;
		public Function.Mode WhichMode = Function.Mode.Polynomial;
		public double Power = 2.0;
		public IResampler Sampler { get; set; } = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			using (var progress = new ProgressBar())
			using (var canvas = new Image<TPixel>(config,rect.Width,rect.Height))
			{
				double ccx,ccy;
				if (CenterPx != null) {
					ccx = CenterPx.Value.X;
					ccy = CenterPx.Value.Y;
				}
				else {
					ccx = frame.Width * (CenterPp == null ? 0.5 : CenterPp.Value.X);
					ccy = frame.Height * (CenterPp == null ? 0.5 : CenterPp.Value.Y);
				}

				Helpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					TPixel nc = ProjectPixel(frame,x,y,ccx,ccy,Power);
					int coff = cy * rect.Width + cx;
					canvas.GetPixelSpan()[coff] = nc;
				},progress);

				frame.BlitImage(canvas,rect);

				//Log.Debug("ppxmin = "+ppxmin);
				//Log.Debug("ppxmax = "+ppxmax);
				//Log.Debug("ppymin = "+ppymin);
				//Log.Debug("ppymax = "+ppymax);
			}
		}

		TPixel ProjectPixel(ImageFrame<TPixel> frame,double x, double y,double ccx, double ccy,double exp)
		{
			double qw = x <= ccx ? ccx : frame.Width - ccx;
			double qh = y <= ccy ? ccy : frame.Height - ccy;

			x -= ccx; y -= ccy;
			double px = 0.0, py = 0.0;

			switch(WhichMode)
			{
			case Function.Mode.Polynomial: {
				//solve(w^q/n = w,n) : n = w^(q-1)
				double dx = Math.Pow(Math.Abs(qw),exp - 1.0);
				double dy = Math.Pow(Math.Abs(qh),exp - 1.0);
				px = Math.Sign(x) * Math.Pow(Math.Abs(x),exp) / dx;
				py = Math.Sign(y) * Math.Pow(Math.Abs(y),exp) / dy;
			}; break;
			case Function.Mode.Inverted: {
				////TODO scaling doesn't quite work. exp=2.0 and mx=my=1.0 works best
				//double num = Math.Pow(Math.Abs(x),exp) + Math.Pow(Math.Abs(y),exp);
				////double mm = Math.Pow(Math.Abs(qw),exp) + Math.Pow(Math.Abs(qh),exp);
				//double mx = 1.0; //qw * qw / mm;
				//double my = 1.0; //qh * qh / mm;
				//px = mx * num / x;
				//py = my * num / y;

				double ax = Math.Pow(Math.Abs(x),exp);
				double ay = Math.Pow(Math.Abs(y),exp);
				double aw = Math.Pow(Math.Abs(qw),exp);
				double ah = Math.Pow(Math.Abs(qh),exp);
				double num = ax + ay;
				// solve(((w^e+h^e)/w)*n=w,n);
				double dx = (qw * qw)/(aw+ah);
				double dy = (qh * qh)/(ah+aw);

				px = num / x * dx;
				py = num / y * dy;
			}; break;
			}

			px += ccx; py += ccy;

			return ImageHelpers.Sample(frame,px,py,Sampler);
		}

		#if false
		TPixel ProjectPixelB(ImageFrame<TPixel> frame,double x, double y,double ccx,double ccy)
		{
			double fw = frame.Width / 2;
			double fh = frame.Height / 2;
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

			return ImageHelpers.Sample(frame,px,py,Sampler);
		}


		double ppxmin = double.MaxValue;
		double ppxmax = double.MinValue;
		double ppymin = double.MaxValue;
		double ppymax = double.MinValue;
		#endif
	}
}
