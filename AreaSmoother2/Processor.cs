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
		public bool HOnly = false;
		public bool VOnly = false;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			var canvas = new Image<TPixel>(config,rect.Width,rect.Height);

			var cspan = canvas.GetPixelSpan();

			if (!VOnly) {
				for(int y = rect.Top; y < rect.Bottom; y++ ) {
					Visited.Clear();
					for(int x = rect.Left; x < rect.Right; x++) {
						if (Visited.Contains(x)) { continue; }
						DrawGradientH(frame,canvas,rect,x,y);
					}
				}
			}

			if (!HOnly) {
				for(int x = rect.Left; x < rect.Right; x++) {
					Visited.Clear();
					for(int y = rect.Top; y < rect.Bottom; y++ ) {
						if (Visited.Contains(y)) { continue; }
						DrawGradientV(frame,canvas,rect,x,y,!VOnly);
					}
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

		void DrawGradientH(ImageFrame<TPixel> frame, Image<TPixel> canvas, Rectangle rect, int x, int y)
		{
			var cSpan = canvas.GetPixelSpan();
			var fSpan = frame.GetPixelSpan();
			TPixel seed = fSpan[y * rect.Width + x];
			int lx = x;
			int rx = x;
			while(lx > rect.Left) {
				if (!fSpan[y * rect.Width + lx].Equals(seed)) { break; }
				lx--;
			}
			while(rx < rect.Right - 1) {
				if (!fSpan[y * rect.Width + rx].Equals(seed)) { break; }
				rx++;
			}

			int len = rx - lx - 1;
			if (len <= 2) {
				// color span is to small so just use colors as-is
				Visited.Add(x);
				int off = GetOffset(x,y,rect);
				cSpan[off] = seed;
				return;
			}
		
			var sColor = seed.ToColor();
			var lColor = Between(fSpan[y * rect.Width + lx].ToColor(),sColor,0.5);
			var rColor = Between(fSpan[y * rect.Width + rx].ToColor(),sColor,0.5);

			for(int gi=0; gi<len; gi++)
			{
				double ratio = (double)(gi+1)/(double)len;
				Rgba32 nc;
				if (ratio > 0.5) {
					nc = Between(sColor,rColor,(ratio - 0.5) * 2.0);
				} else {
					nc = Between(lColor,sColor,ratio * 2.0);
				}
				int gx = lx + gi + 1;
				int off = GetOffset(gx,y,rect);
				cSpan[off] = nc.FromColor<TPixel>();
				Visited.Add(gx);
			}
		}

		void DrawGradientV(ImageFrame<TPixel> frame, Image<TPixel> canvas, Rectangle rect, int x, int y, bool blend)
		{
			var cSpan = canvas.GetPixelSpan();
			var fSpan = frame.GetPixelSpan();
			var seed = fSpan[y * rect.Width + x].ToColor();
			int ty = y;
			int by = y;
			while(ty > rect.Top) {
				if (!fSpan[ty * rect.Width + x].Equals(seed)) { break; }
				ty--;
			}
			while(by < rect.Bottom - 1) {
				if (!fSpan[by * rect.Width + x].Equals(seed)) { break; }
				by++;
			}

			int len = by - ty - 1;
			if (len <= 2) {
				// color span is to small so just use colors as-is
				Visited.Add(y);
				int off = GetOffset(x,y,rect);
				var fc = blend ? Between(seed,cSpan[off].ToColor(),0.5) : seed;
				cSpan[off] = fc.FromColor<TPixel>();
				return;
			}
		
			var tColor = Between(fSpan[ty * rect.Width + x].ToColor(),seed,0.5);
			var bColor = Between(fSpan[by * rect.Width + x].ToColor(),seed,0.5);

			for(int gi=0; gi<len; gi++)
			{
				double ratio = (double)(gi+1)/(double)len;
				Rgba32 nc;
				if (ratio > 0.5) {
					nc = Between(seed,bColor,(ratio - 0.5) * 2.0);
				} else {
					nc = Between(tColor,seed,ratio * 2.0);
				}
				int gy = ty + gi + 1;
				int off = GetOffset(x,gy,rect);
				var fc = blend ? Between(nc,cSpan[off].ToColor(),0.5) : nc;
				cSpan[off] = fc.FromColor<TPixel>();
				Visited.Add(gy);
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

		HashSet<int> Visited = new HashSet<int>();
	}
}
