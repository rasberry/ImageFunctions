using System;
using System.Drawing;
using ImageFunctions.Helpers;

namespace ImageFunctions.Deform
{
	public class Processor : IFAbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			var Iis = Engines.Engine.GetConfig();
			var frame = Source;
			var rect = Bounds;

			using (var progress = new ProgressBar())
			using (var canvas = Iis.NewImage(rect.Width,rect.Height))
			{
				double ccx,ccy;
				if (O.CenterPx != null) {
					ccx = O.CenterPx.Value.X;
					ccy = O.CenterPx.Value.Y;
				}
				else {
					ccx = frame.Width * (O.CenterPp == null ? 0.5 : O.CenterPp.Value.X);
					ccy = frame.Height * (O.CenterPp == null ? 0.5 : O.CenterPp.Value.Y);
				}

				MoreHelpers.ThreadPixels(rect, MaxDegreeOfParallelism, (x,y) => {
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					IFColor nc = ProjectPixel(frame,x,y,ccx,ccy,O.Power);
					canvas[cx,cy] = nc;
				},progress);

				frame.BlitImage(canvas,rect);
			}
		}

		IFColor ProjectPixel(IFImage frame,double x, double y,double ccx, double ccy,double exp)
		{
			double qw = x <= ccx ? ccx : frame.Width - ccx;
			double qh = y <= ccy ? ccy : frame.Height - ccy;

			x -= ccx; y -= ccy;
			double px = 0.0, py = 0.0;

			switch(O.WhichMode)
			{
			case Mode.Polynomial: {
				//solve(w^q/n = w,n) : n = w^(q-1)
				double dx = Math.Pow(Math.Abs(qw),exp - 1.0);
				double dy = Math.Pow(Math.Abs(qh),exp - 1.0);
				px = Math.Sign(x) * Math.Pow(Math.Abs(x),exp) / dx;
				py = Math.Sign(y) * Math.Pow(Math.Abs(y),exp) / dy;
			}; break;
			case Mode.Inverted: {
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

			return ImageHelpers.Sample(frame,px,py,O.Sampler);
		}

		public override void Dispose() {}
	}

	#if false
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Options O = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			using (var progress = new ProgressBar())
			using (var canvas = new Image<TPixel>(config,rect.Width,rect.Height))
			{
				double ccx,ccy;
				if (O.CenterPx != null) {
					ccx = O.CenterPx.Value.X;
					ccy = O.CenterPx.Value.Y;
				}
				else {
					ccx = frame.Width * (O.CenterPp == null ? 0.5 : O.CenterPp.Value.X);
					ccy = frame.Height * (O.CenterPp == null ? 0.5 : O.CenterPp.Value.Y);
				}

				MoreHelpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					TPixel nc = ProjectPixel(frame,x,y,ccx,ccy,O.Power);
					int coff = cy * rect.Width + cx;
					canvas.GetPixelSpan()[coff] = nc;
				},progress);

				frame.BlitImage(canvas.Frames.RootFrame,rect);
			}
		}

		TPixel ProjectPixel(ImageFrame<TPixel> frame,double x, double y,double ccx, double ccy,double exp)
		{
			double qw = x <= ccx ? ccx : frame.Width - ccx;
			double qh = y <= ccy ? ccy : frame.Height - ccy;

			x -= ccx; y -= ccy;
			double px = 0.0, py = 0.0;

			switch(O.WhichMode)
			{
			case Mode.Polynomial: {
				//solve(w^q/n = w,n) : n = w^(q-1)
				double dx = Math.Pow(Math.Abs(qw),exp - 1.0);
				double dy = Math.Pow(Math.Abs(qh),exp - 1.0);
				px = Math.Sign(x) * Math.Pow(Math.Abs(x),exp) / dx;
				py = Math.Sign(y) * Math.Pow(Math.Abs(y),exp) / dy;
			}; break;
			case Mode.Inverted: {
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

			return ImageHelpers.Sample(frame,px,py,O.Sampler);
		}
	}
	#endif
}
