using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System.Collections.Generic;

namespace ImageFunctions.AreaSmoother
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public int TotalTries = 7;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			using (var canvas = new Image<TPixel>(config,rect.Width,rect.Height))
			{
				var cspan = canvas.GetPixelSpan();
				for(int y = rect.Top; y < rect.Bottom; y++ ) {
					int cy = y - rect.Top;
					for(int x = rect.Left; x < rect.Right; x++) {
						int cx = x - rect.Left;
						TPixel nc = SmoothPixel(frame,x,y);
						//Log.Debug("x="+x+" y="+y+" nc = "+nc);
						int coff = cy * rect.Width + cx;
						cspan[coff] = nc;
					}
				}

				frame.BlitImage(canvas,rect);
			}
		}

		TPixel SmoothPixel(ImageFrame<TPixel> frame,int px, int py)
		{
			Rgba32 start = frame.GetPixelRowSpan(py)[px].ToColor();
			//Log.Debug("px="+px+" py="+py+" start = "+start);
			double bestlen = double.MaxValue;
			double bestang = double.NaN;
			double bestratio = 1;
			Rgba32 bestfc = start;
			Rgba32 bestbc = start;
			Point bestfpx = new Point(px,py);
			Point bestbpx = new Point(px,py);
			double ahigh = Math.PI;
			double alow = 0;

			for(int tries=1; tries <= TotalTries; tries++)
			{
				double dang = (ahigh - alow)/3;
				for(double a = alow; a<ahigh; a+=dang)
				{
					Rgba32 fc;
					Rgba32 bc;

					Point fp = FindColorAlongRay(frame,a,px,py,false,start,out fc);
					Point bp = FindColorAlongRay(frame,a,px,py,true,start,out bc);

					double len = Dist(fp.X,fp.Y,bp.X,bp.Y);

					if (len < bestlen) {
						bestang = a;
						bestlen = len;
						bestfc = Between(fc,start,0.5);
						bestbc = Between(bc,start,0.5);
						bestfpx = fp;
						bestbpx = bp;
						double flen = Dist(px,py,fp.X,fp.Y);
						bestratio = flen/len;
						//Log.Debug("bestratio="+bestratio+" bestfc = "+bestfc+" bestbc="+bestbc);
					}
				}

				alow = bestang - Math.PI/3/tries;
				ahigh = bestang + Math.PI/3/tries;
			}

			Rgba32 final;
			//Log.Debug("bestfc = "+bestfc+" bestbc="+bestbc);
			if (bestfc == start && bestbc == start) {
				final = start;
			}
			else if (bestratio > 0.5) {
				final = Between(start,bestbc,(bestratio-0.5)*2);
			}
			else {
				final = Between(bestfc,start,bestratio*2);
			}
			return final.FromColor<TPixel>();
		}

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

		static double Dist(int x1,int y1,int x2,int y2)
		{
			int dx = x2 - x1;
			int dy = y2 - y1;
			return Math.Sqrt((double)dx*dx + (double)dy*dy);
			// return Sqrt((double)dx*dx + (double)dy*dy);
		}

		static Point FindColorAlongRay(ImageFrame<TPixel> lb, double a, int px, int py, bool back, Rgba32 start, out Rgba32 c)
		{
			double r=1;
			c = start;
			bool done = false;
			double cosa = Math.Cos(a) * (back ? -1 : 1);
			double sina = Math.Sin(a) * (back ? -1 : 1);
			int maxx = lb.Width -1;
			int maxy = lb.Height -1;
			
			while(true) {
				int fx = (int)(cosa * r) + px;
				int fy = (int)(sina * r) + py;
				if (fx < 0 || fy < 0 || fx > maxx || fy > maxy) {
					done = true;
				}
				if (!done) {
					Rgba32 f = lb.GetPixelRowSpan(fy)[fx].ToColor();
					if (f != start) {
						c = f;
						done = true;

					}
				}
				if (done) {
					return new Point(
						fx < 0 ? 0 : fx > maxx ? maxx : fx
						,fy < 0 ? 0 : fy > maxy ? maxy : fy
					);
				}
				r+=1;
			}
		}
	}
}
