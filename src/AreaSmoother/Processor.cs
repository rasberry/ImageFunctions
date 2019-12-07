using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;
using System;
using System.Numerics;

namespace ImageFunctions.AreaSmoother
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Options O = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			using (var progress = new ProgressBar())
			using (var canvas = new Image<TPixel>(config,rect.Width,rect.Height))
			{
				MoreHelpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					TPixel nc = SmoothPixel(frame,x,y);
					int coff = cy * rect.Width + cx;
					canvas.GetPixelSpan()[coff] = nc;
				},progress);

				frame.BlitImage(canvas.Frames.RootFrame,rect);
			}
		}

		TPixel SmoothPixel(ImageFrame<TPixel> frame,int px, int py)
		{
			TPixel start = frame.GetPixelRowSpan(py)[px];

			//Log.Debug("px="+px+" py="+py+" start = "+start);
			double bestlen = double.MaxValue;
			double bestang = double.NaN;
			double bestratio = 1;
			TPixel bestfc = start;
			TPixel bestbc = start;
			Point bestfpx = new Point(px,py);
			Point bestbpx = new Point(px,py);
			double ahigh = Math.PI;
			double alow = 0;

			for(int tries=1; tries <= O.TotalTries; tries++)
			{
				double dang = (ahigh - alow)/3;
				for(double a = alow; a<ahigh; a+=dang)
				{
					Point fp = FindColorAlongRay(frame,a,px,py,false,start,out TPixel fc);
					Point bp = FindColorAlongRay(frame,a,px,py,true,start,out TPixel bc);

					double len = O.Measurer.Measure(fp.X,fp.Y,bp.X,bp.Y);

					if (len < bestlen) {
						bestang = a;
						bestlen = len;
						bestfc = ImageHelpers.BetweenColor(fc,start,0.5);
						bestbc = ImageHelpers.BetweenColor(bc,start,0.5);
						bestfpx = fp;
						bestbpx = bp;
						double flen = O.Measurer.Measure(px,py,fp.X,fp.Y);
						bestratio = flen/len;
						//Log.Debug("bestratio="+bestratio+" bestfc = "+bestfc+" bestbc="+bestbc);
					}
				}

				alow = bestang - Math.PI/3/tries;
				ahigh = bestang + Math.PI/3/tries;
			}

			TPixel final;
			//Log.Debug("bestfc = "+bestfc+" bestbc="+bestbc);
			if (bestfc.Equals(start) && bestbc.Equals(start)) {
				final = start;
			}
			else if (bestratio > 0.5) {
				final = ImageHelpers.BetweenColor(start,bestbc,(bestratio-0.5)*2);
			}
			else {
				final = ImageHelpers.BetweenColor(bestfc,start,bestratio*2);
			}
			return final;
		}

		Point FindColorAlongRay(ImageFrame<TPixel> lb, double a, int px, int py, bool back, TPixel start, out TPixel c)
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
					TPixel f = ImageHelpers.Sample(lb,fx,fy,O.Sampler);
					if (!f.Equals(start)) {
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
