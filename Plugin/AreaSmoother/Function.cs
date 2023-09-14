using System.Text;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.AreaSmoother;

public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb);
	}

	public bool Run(IRegister register, ILayers layers, string[] args)
	{
		var Iis = Tools.Engine;
		if (layers.Count < 1) {
			PlugTell.LayerMustHaveOne();
			return false;
		}
		var canvas = layers[0];

		using var progress = new ProgressBar();
		Tools.ThreadPixels(canvas, (x,y) => {
			var nc = SmoothPixel(canvas,x,y);
			canvas[x,y] = nc;
		},progress);

		return true;
	}

	ColorRGBA SmoothPixel(ICanvas frame,int px, int py)
	{
		ColorRGBA start = frame[px,py];

		//Log.Debug("px="+px+" py="+py+" start = "+start);
		double bestlen = double.MaxValue;
		double bestang = double.NaN;
		double bestratio = 1;
		ColorRGBA bestfc = start;
		ColorRGBA bestbc = start;
		//Point bestfpx = new Point(px,py);
		//Point bestbpx = new Point(px,py);
		double ahigh = Math.PI;
		double alow = 0;

		for(int tries=1; tries <= Options.TotalTries; tries++)
		{
			double dang = (ahigh - alow)/3;
			for(double a = alow; a<ahigh; a+=dang)
			{
				Point fp = FindColorAlongRay(frame,a,px,py,false,start,out ColorRGBA fc);
				Point bp = FindColorAlongRay(frame,a,px,py,true,start,out ColorRGBA bc);

				double len = Options.Measurer.Value.Measure(fp.X,fp.Y,bp.X,bp.Y);

				if (len < bestlen) {
					bestang = a;
					bestlen = len;
					bestfc = MoreTools.BetweenColor(fc,start,0.5);
					bestbc = MoreTools.BetweenColor(bc,start,0.5);
					//bestfpx = fp;
					//bestbpx = bp;
					double flen = Options.Measurer.Value.Measure(px,py,fp.X,fp.Y);
					bestratio = flen/len;
					// Log.Debug("bestratio="+bestratio+" bestfc = "+bestfc+" bestbc="+bestbc);
				}
			}

			alow = bestang - Math.PI/3/tries;
			ahigh = bestang + Math.PI/3/tries;
		}

		ColorRGBA final;
		// Log.Debug("bestfc = "+bestfc+" bestbc="+bestbc);
		if (bestfc.Equals(start) && bestbc.Equals(start)) {
			final = start;
		}
		else if (bestratio > 0.5) {
			final = MoreTools.BetweenColor(start,bestbc,(bestratio-0.5)*2);
		}
		else {
			final = MoreTools.BetweenColor(bestfc,start,bestratio*2);
		}
		return final;
	}

	Point FindColorAlongRay(ICanvas lb, double a, int px, int py, bool back, ColorRGBA start, out ColorRGBA c)
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
				ColorRGBA f = Options.Sampler.Value.GetSample(lb,(int)fx,(int)fy);
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