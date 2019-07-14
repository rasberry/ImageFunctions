using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace ImageFunctions.AreaSmoother
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public int TotalTries = 7;
		public IResampler Sampler = null;
		public MetricFunction Measurer = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			if (Measurer == null) {
				Measurer = Helpers.DistanceEuclidean;
			}
			using (var canvas = new Image<TPixel>(config,rect.Width,rect.Height))
			{
				Helpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					TPixel nc = SmoothPixel(frame,x,y);
					int coff = cy * rect.Width + cx;
					canvas.GetPixelSpan()[coff] = nc;
				});

				frame.BlitImage(canvas,rect);
			}
		}

		TPixel SmoothPixel(ImageFrame<TPixel> frame,int px, int py)
		{
			TPixel tpstart = frame.GetPixelRowSpan(py)[px];
			Rgba32 start = tpstart.ToColor();
			
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
					Point fp = FindColorAlongRay(frame,a,px,py,false,start,out Rgba32 fc);
					Point bp = FindColorAlongRay(frame,a,px,py,true,start,out Rgba32 bc);

					double len = Measurer(fp.X,fp.Y,bp.X,bp.Y);

					if (len < bestlen) {
						bestang = a;
						bestlen = len;
						bestfc = Between(fc,start,0.5);
						bestbc = Between(bc,start,0.5);
						bestfpx = fp;
						bestbpx = bp;
						double flen = Measurer(px,py,fp.X,fp.Y);
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

		Point FindColorAlongRay(ImageFrame<TPixel> lb, double a, int px, int py, bool back, Rgba32 start, out Rgba32 c)
		{
			double r=1;
			c = start;
			bool done = false;
			double cosa = Math.Cos(a) * (back ? -1 : 1);
			double sina = Math.Sin(a) * (back ? -1 : 1);
			int maxx = lb.Width -1;
			int maxy = lb.Height -1;
			
			while(true) {
				double fx = (int)(cosa * r) + px;
				double fy = (int)(sina * r) + py;
				if (fx < 0 || fy < 0 || fx > maxx || fy > maxy) {
					done = true;
				}
				if (!done) {
					Rgba32 f = ImageHelpers.Sample(lb,fx,fy,Sampler).ToColor();
					//Rgba32 f = lb.GetPixelRowSpan(fy)[fx].ToColor();
					if (f != start) {
						c = f;
						done = true;

					}
				}
				if (done) {
					int ix = (int)fx;
					int iy = (int)fy;
					return new Point(
						ix < 0 ? 0 : ix > maxx ? maxx : ix
						,iy < 0 ? 0 : iy > maxy ? maxy : iy
					);
				}
				r+=1;
			}
		}
	}
}
