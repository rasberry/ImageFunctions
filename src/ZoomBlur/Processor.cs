using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using ImageFunctions.Helpers;

namespace ImageFunctions.ZoomBlur
{
	public class Processor : IFAbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			using (var progress = new ProgressBar())
			{
				Iic = Engines.Engine.GetConfig();
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
						IFColor nc = ZoomPixel(canvas,Bounds,x,y,w2,h2);
						int cy = y - Bounds.Top;
						int cx = x - Bounds.Left;
						canvas[cx,cy] = nc;
					},progress);

					Source.BlitImage(canvas,Bounds);
				}
			}
		}

		IFColor ZoomPixel(IFImage frame, IFRectangle rect, int x, int y,double cx, double cy)
		{
			double dist = O.Measurer.Measure(x,y,cx,cy);
			int idist = (int)Math.Ceiling(dist);

			List<IFColor> vector = new List<IFColor>(idist);
			double ang = Math.Atan2(y - cy, x - cx);
			double sd = dist;
			double ed = dist * O.ZoomAmount;

			for (double d = sd; d < ed; d++)
			{
				double px = Math.Cos(ang) * d + cx;
				double py = Math.Sin(ang) * d + cy;
				IFColor c = ImageHelpers.Sample(frame,px,py);
				vector.Add(c);
			}

			IFColor avg;
			int count = vector.Count;
			if (count < 1) {
				avg = ImageHelpers.Sample(frame,x,y);
			}
			else if (count == 1) {
				avg = vector[0];
			}
			else {
				double cr = 0, cg = 0, cb = 0, ca = 0;
				foreach (IFColor tpc in vector)
				{
					cr += tpc.R; cg += tpc.G; cb += tpc.B;
					ca += tpc.A;
				}
				avg = new IFColor {
					R = (float)(cr / count),
					G = (float)(cg / count),
					B = (float)(cb / count),
					A = (float)(ca / count)
				};
			}
			return avg;
		}

		public override void Dispose() {}
		IFImageConfig Iic = null;
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
				double w2 = rect.Width / 2.0;
				double h2 = rect.Height / 2.0;

				if (O.CenterPx.HasValue) {
					w2 = O.CenterPx.Value.X;
					h2 = O.CenterPx.Value.Y;
				}
				else if (O.CenterRt.HasValue) {
					w2 = rect.Width * O.CenterRt.Value.X;
					h2 = rect.Height * O.CenterRt.Value.Y;
				}

				MoreHelpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
					TPixel nc = ZoomPixel(frame,rect,x,y,w2,h2);
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					int coff = cy * rect.Width + cx;
					canvas.GetPixelSpan()[coff] = nc;
				},progress);

				frame.BlitImage(canvas.Frames.RootFrame,rect);
			}
		}

		TPixel ZoomPixel(ImageFrame<TPixel> frame, Rectangle rect, int x, int y,double cx, double cy)
		{
			double dist = O.Measurer.Measure(x,y,cx,cy);
			int idist = (int)Math.Ceiling(dist);

			List<TPixel> vector = new List<TPixel>(idist);
			double ang = Math.Atan2(y - cy, x - cx);
			double sd = dist;
			double ed = dist * O.ZoomAmount;

			for (double d = sd; d < ed; d++)
			{
				double px = Math.Cos(ang) * d + cx;
				double py = Math.Sin(ang) * d + cy;
				TPixel c = ImageHelpers.Sample(frame,px,py,O.Sampler);
				vector.Add(c);
			}

			TPixel avg;
			int count = vector.Count;
			if (count < 1) {
				avg = ImageHelpers.Sample(frame,x,y,O.Sampler);
			}
			else if (count == 1) {
				avg = vector[0];
			}
			else {
				double cr = 0, cg = 0, cb = 0, ca = 0;
				foreach (TPixel tpc in vector)
				{
					var c = tpc.ToColor();
					cr += c.R; cg += c.G; cb += c.B;
					ca += c.A;
				}
				var rgbaAvg = new RgbaD(
					cr / count,
					cg / count,
					cb / count,
					ca / count
				);
				avg = rgbaAvg.FromColor<TPixel>();
			}
			return avg;
		}

		#if false
		//TODO this is actually a regular blur function
		static Rgba32 GetAliasedColor(ImageFrame<TPixel> lb, int x, int y)
		{
			Rgba32
				 c00 = GetExtendedPixel(lb,x-1,y-1)
				,c01 = GetExtendedPixel(lb,x+0,y-1)
				,c02 = GetExtendedPixel(lb,x+1,y-1)
				,c10 = GetExtendedPixel(lb,x-1,y+0)
				,c11 = GetExtendedPixel(lb,x+0,y+0)
				,c12 = GetExtendedPixel(lb,x+1,y+0)
				,c20 = GetExtendedPixel(lb,x-1,y+1)
				,c21 = GetExtendedPixel(lb,x+0,y+1)
				,c22 = GetExtendedPixel(lb,x+1,y+1)
			;

			double d1 = 1.0/16.0;
			double d2 = 2.0/16.0;
			double d4 = 4.0/16.0;

			double a =
				  d1 * c00.A + d2 * c01.A + d1 * c02.A
				+ d2 * c10.A + d4 * c11.A + d2 * c12.A
				+ d1 * c20.A + d2 * c21.A + d1 * c22.A
			;
			double r =
				  d1 * c00.R + d2 * c01.R + d1 * c02.R
				+ d2 * c10.R + d4 * c11.R + d2 * c12.R
				+ d1 * c20.R + d2 * c21.R + d1 * c22.R
			;
			double g =
				  d1 * c00.G + d2 * c01.G + d1 * c02.G
				+ d2 * c10.G + d4 * c11.G + d2 * c12.G
				+ d1 * c20.G + d2 * c21.G + d1 * c22.G
			;
			double b =
				  d1 * c00.B + d2 * c01.B + d1 * c02.B
				+ d2 * c10.B + d4 * c11.B + d2 * c12.B
				+ d1 * c20.B + d2 * c21.B + d1 * c22.B
			;

			return new Rgba32((byte)r,(byte)g,(byte)b,(byte)a);
		}

		static Rgba32 GetExtendedPixel(ImageFrame<TPixel> b,int x, int y)
		{
			int bx = Math.Clamp(x,0,b.Width-1);
			int by = Math.Clamp(y,0,b.Height-1);
			return b.GetPixelRowSpan(by)[bx].ToColor();
		}
		#endif
	}
	#endif
}
