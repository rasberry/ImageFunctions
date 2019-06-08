using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System.Collections.Generic;

namespace ImageFunctions.AreaSmoother2
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{

		// IDEA
		// foreach pixel
		//  skip pixels marked done
		//  find left right boundry
		//    foreach pixel in row
		//      draw gradient 
		//      mark pixel as done
		// clear done marks
		// repeat for top bottom / columns
		//  except when drawing gradient average the previous drawn pixel and new pixel

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			var canvas = new Image<TPixel>(config,rect.Width,rect.Height);

			var cspan = canvas.GetPixelSpan();
			for(int y = rect.Top; y < rect.Bottom; y++ ) {
				for(int x = rect.Left; x < rect.Right; x++) {
					if (Visited.Contains(new Point(x,y))) { continue; }
					DrawGradient(frame,canvas,rect,x,y);
				}
			}

			var fspan = frame.GetPixelSpan();
			for(int y = rect.Top; y < rect.Bottom; y++) {
				int cy = y - rect.Top;
				for(int x = rect.Left; x < rect.Right; x++) {
					int cx = x - rect.Left;
					int foff = y * frame.Width + x;
					int coff = cy * rect.Width + cx;
					fspan[foff] = cspan[coff];
				}
			}
		}

		void DrawGradient(ImageFrame<TPixel> frame, Image<TPixel> canvas, Rectangle rect, int x, int y)
		{
			var cSpan = canvas.GetPixelSpan();
			var ySpan = frame.GetPixelRowSpan(y);
			TPixel seed = ySpan[x];
			int lx = x;
			int rx = x;
			while(lx > rect.Left) {
				if (!ySpan[lx].Equals(seed)) { break; }
				lx--;
			}
			while(rx < rect.Right - 1) {
				if (!ySpan[rx].Equals(seed)) { break; }
				rx++;
			}

			Log.Debug("x="+x+" lx="+lx+" rx="+rx);
			
			int len = rx - lx - 1;
			if (len <= 2) {
				// color span is to small so just use colors as-is
				Visited.Add(new Point(x,y));
				int off = GetOffset(x,y,rect);
				cSpan[off] = seed;
				return;
			}
		
			var sColor = seed.ToColor();
			var lColor = Between(ySpan[lx].ToColor(),sColor,0.5);
			var rColor = Between(ySpan[rx].ToColor(),sColor,0.5);

			int end = len / 2;
			for(int gi = 0; gi < end; gi++)
			{
				double ratio = (double)(gi+1)/(double)end;
				var nc = Between(lColor,sColor,ratio);
				int gx = lx + gi + 1;
				int off = GetOffset(gx,y,rect);
				cSpan[off] = nc.FromColor<TPixel>();
				Visited.Add(new Point(gx,y));
			}

			end += len % 2;
			for(int gi = 0; gi < end; gi++)
			{
				double ratio = (double)(gi+1)/(double)end;
				int gx = rx - end + gi;
				var nc = Between(sColor,rColor,ratio);
				int off = GetOffset(gx,y,rect);
				cSpan[off] = nc.FromColor<TPixel>();
				Visited.Add(new Point(gx,y));
			}
		}

		static int GetOffset(int x,int y,Rectangle rect)
		{
			int oy = y - rect.Top;
			int ox = x - rect.Left;
			int off = oy * rect.Width + ox;
			return off;
		}

		//ratio 0.0 = 100% a
		//ratio 1.0 = 100% b
		static Rgba32 Between(Rgba32 a, Rgba32 b, double ratio)
		{
			byte nr = (byte)Math.Round((1-ratio)*a.R + ratio*b.R,0);
			byte ng = (byte)Math.Round((1-ratio)*a.G + ratio*b.G,0);
			byte nb = (byte)Math.Round((1-ratio)*a.B + ratio*b.B,0);
			byte na = (byte)Math.Round((1-ratio)*a.A + ratio*b.A,0);
			var btw = new Rgba32(nr,ng,nb,na);
			// Log.Debug("between a="+a+" b="+b+" r="+ratio+" nr="+nr+" ng="+ng+" nb="+nb+" na="+na+" btw="+btw);
			return btw;
		}

		HashSet<Point> Visited = new HashSet<Point>();
	}
}
