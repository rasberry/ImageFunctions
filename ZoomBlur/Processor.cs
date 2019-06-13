using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageFunctions.ZoomBlur
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public double ZoomAmount = 1.1;
		public Point? CenterPx = null;
		public PointF? CenterRt = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			using (var canvas = new Image<TPixel>(config,rect.Width,rect.Height))
			{
				double w2 = rect.Width / 2.0;
				double h2 = rect.Height / 2.0;

				if (CenterPx.HasValue) {
					w2 = CenterPx.Value.X;
					h2 = CenterPx.Value.Y;
				}
				else if (CenterRt.HasValue) {
					w2 = rect.Width * CenterRt.Value.X;
					h2 = rect.Height * CenterRt.Value.Y;
				}

				Helpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
					TPixel nc = ZoomPixel(frame,rect,x,y,w2,h2);
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					int coff = cy * rect.Width + cx;
					canvas.GetPixelSpan()[coff] = nc;
				});

				frame.BlitImage(canvas,rect);
			}
		}

		TPixel ZoomPixel(ImageFrame<TPixel> frame, Rectangle rect, int x, int y,double cx, double cy)
		{
			double dist = Math.Sqrt((y - cy) * (y - cy) + (x - cx) * (x - cx));
			int idist = (int)Math.Ceiling(dist);

			List<Rgba32> vector = new List<Rgba32>(idist);
			double ang = Math.Atan2(y - cy, x - cx);
			double sd = dist;
			double ed = dist * ZoomAmount;

			for (double d = sd; d < ed; d++)
			{
				double px = Math.Cos(ang) * d + cx;
				double py = Math.Sin(ang) * d + cy;
				int ipx = (int)Math.Round(px, 0);
				int ipy = (int)Math.Round(py, 0);
				Rgba32 c = GetExtendedPixel(frame,ipx,ipy);
				//Rgba32 c = GetAliasedColor(frame, ipx, ipy);
				vector.Add(c);
			}

			Rgba32 avg;
			int count = vector.Count;
			if (count < 2)
			{
				avg = frame.GetPixelRowSpan((int)y)[(int)x].ToColor();
			}
			else
			{
				int cr = 0, cg = 0, cb = 0, ca = 0;
				foreach (Rgba32 c in vector)
				{
					cr += c.R; cg += c.G; cb += c.B;
					ca += c.A;
				}
				avg = new Rgba32(
					(byte)(cr / count),
					(byte)(cg / count),
					(byte)(cb / count),
					(byte)(ca / count)
				);
			}
			return avg.FromColor<TPixel>();
		}

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
	}
}
