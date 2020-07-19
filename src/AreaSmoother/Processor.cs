using System;
using System.Drawing;
using System.Numerics;
using ImageFunctions.Helpers;

namespace ImageFunctions.AreaSmoother
{
	public class Processor : AbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			var Iis = Registry.GetImageEngine();
			var frame = Source;
			var rect = Bounds;

			using (var progress = new ProgressBar())
			using (var canvas = Iis.NewImage(rect.Width,rect.Height))
			{
				MoreHelpers.ThreadPixels(rect, MaxDegreeOfParallelism, (x,y) => {
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					IColor nc = SmoothPixel(frame,x,y);
					canvas[cx,cy] = nc;
				},progress);

				frame.BlitImage(canvas,rect);
			}
		}

		IColor SmoothPixel(IImage frame,int px, int py)
		{
			IColor start = frame[px,py];

			//Log.Debug("px="+px+" py="+py+" start = "+start);
			double bestlen = double.MaxValue;
			double bestang = double.NaN;
			double bestratio = 1;
			IColor bestfc = start;
			IColor bestbc = start;
			Point bestfpx = new Point(px,py);
			Point bestbpx = new Point(px,py);
			double ahigh = Math.PI;
			double alow = 0;

			for(int tries=1; tries <= O.TotalTries; tries++)
			{
				double dang = (ahigh - alow)/3;
				for(double a = alow; a<ahigh; a+=dang)
				{
					Point fp = FindColorAlongRay(frame,a,px,py,false,start,out IColor fc);
					Point bp = FindColorAlongRay(frame,a,px,py,true,start,out IColor bc);

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
						// Log.Debug("bestratio="+bestratio+" bestfc = "+bestfc+" bestbc="+bestbc);
					}
				}

				alow = bestang - Math.PI/3/tries;
				ahigh = bestang + Math.PI/3/tries;
			}

			IColor final;
			// Log.Debug("bestfc = "+bestfc+" bestbc="+bestbc);
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

		Point FindColorAlongRay(IImage lb, double a, int px, int py, bool back, IColor start, out IColor c)
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
					IColor f = O.Sampler.GetSample(lb,(int)fx,(int)fy);
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

		public override void Dispose() {}
	}

}
