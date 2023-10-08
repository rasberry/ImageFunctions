using System;
using System.Drawing;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.SpearGraphic;

public static class Second
{
	public static void Twist3(ICanvas image, DrawLineFunc dlf, int w, int h, int which = 2)
	{
		if (which == 0)
		{
			int iter = 9;
			double max = w*iter; //9,10,11 do different things
			double s = w/32.0;
			double o = w/10.0;
			double t = 1.1*iter;
			double aa = 64, am = 0, an = 0;
			Twist3Params(image, w, h, max, s, o, t, aa, am, an);
		}
		if (which == 1)
		{
			int iter = 40;
			double max = w*iter; //9,10,11 do different things
			double s = w/256.0;
			double o = w/6.0;
			double t = 1.2*iter;
			double aa = 64, am = 1, an = 0;
			Twist3Params(image, w, h, max, s, o, t, aa, am, an);
		}
		if (which == 2)
		{
			int iter = 80;
			double max = w*iter; //9,10,11 do different things
			double s = w/1.5;
			double o =w/4.0;
			double t = 2*iter;
			double aa = 1, am = 0, an = 0;
			Twist3Params(image, w, h, max, s, o, t, aa, am, an);
		}
	}

	static void Twist3Params(ICanvas image, int w, int h, double max, double s, double o, double t, double aa, double am, double an)
	{
		double a,x,y,ox,oy;

		using var progress = new ProgressBar();

		for(double v=1; v<max; v++)
		{
			ox = o+(an+(am*(max-v)/max))*Math.Cos(v*Math.PI/aa);
			oy = o+(an+(am*(max-v)/max))*Math.Sin(v*Math.PI/aa);

			a = (max - v)*Math.PI/(max / (max - v/32)) + Math.PI;
			x = s * Math.Tan(a) + (v/t) + ox;
			y = s * Math.Sin(a) + (v/t) + oy;
			Color p = ColorFade(v,max,FadeComp.G);
			if (x < w && y < h && x > 0 && y > 0) {
				DrawPoint(image,p,x,y);
				DrawPoint(image,p,y,x);
			}
			progress.Report(v/max);
		}
	}

	const double DRo = 0.3;
	static void DrawPoint(ICanvas img, Color p, double x, double y)
	{
		DrawPixel(img,p,x - DRo, y - DRo);
		DrawPixel(img,p,x + 0.0, y - DRo);
		DrawPixel(img,p,x + DRo, y - DRo);
		DrawPixel(img,p,x - DRo, y + 0.0);
		DrawPixel(img,p,x + 0.0, y + 0.0);
		DrawPixel(img,p,x + DRo, y + 0.0);
		DrawPixel(img,p,x - DRo, y + DRo);
		DrawPixel(img,p,x + 0.0, y + DRo);
		DrawPixel(img,p,x + DRo, y + DRo);
	}

	//static void DrawRectangle(IImage op, Color p, double x, double y)
	//{
	//	PointF p0 = new PointF((float)x + 0.0f,(float)y + 0.0f);
	//	PointF p1 = new PointF((float)x + 0.5f,(float)y + 0.0f);
	//	PointF p2 = new PointF((float)x + 0.5f,(float)y + 0.5f);
	//	PointF p3 = new PointF((float)x + 0.0f,(float)y + 0.5f);
	//	op.DrawPolygon(gop,(Color)p,1.0f,p0,p1,p2,p3);
	//}

	// https://en.wikipedia.org/wiki/Spatial_anti-aliasing
	static void DrawPixel(ICanvas img, Color p, double x, double y)
	{
		var nc = ColorRGBA.FromRGBA255(p.R, p.G, p.B, p.A);
		int fx = (int)Math.Floor(x), cx = (int)Math.Ceiling(x);
		int fy = (int)Math.Floor(y), cy = (int)Math.Ceiling(y);

		for(int ry = (int)fy; ry < cy; ry++) {
			for(double rx = fx; rx < cx; rx++) {
				double px = 1.0 - Math.Abs(x - rx);
				double py = 1.0 - Math.Abs(y - ry);
				double pp = px * py;
				CompostPixel(img,nc,rx,ry,pp);
			}
		}
	}

	static void CompostPixel(ICanvas img, ColorRGBA c, double x, double y, double k)
	{
		int ix = (int)x, iy = (int)y;
		if (ix < 0 || iy < 0 || ix >= img.Width || iy >= img.Height) {
			return;
		}
		if (k >= 1.0) {
			img[ix,iy] = c; //100% opacity so replace
		}
		else if (k < double.Epsilon) {
			return; //0% opacity so keep original
		}
		else {
			//https://en.wikipedia.org/wiki/Alpha_compositing
			var o = img[ix,iy];
			double oma = (c.A * k)*(1.0 - o.A);
			double a = o.A + oma;
			double r = (o.R*o.A + c.R*oma) / a;
			double g = (o.G*o.A + c.G*oma) / a;
			double b = (o.B*o.A + c.B*oma) / a;
			img[ix,iy] = new ColorRGBA(r,g,b,a);
		}
	}

	public static void Twist4(ICanvas image, DrawLineFunc dlf, int w, int h)
	{
		double max = w*9;
		double s = w/32.0; //stretch x
		double o = w/4.0; //offset
		double t = 20; //stretch y
		double a,x,y;
		double lx=0,ly=0;
		double oos = w * 0.088; //offset size

		using (var progress = new ProgressBar())
		{
			for(double v=1; v<max; v++)
			{
				a = (max - v)*Math.PI/(max / (max - v/32)) + Math.PI;
				x = s * Math.Tan(a) + (v/t) + o;
				y = s * Math.Sin(a) + (v/t) + o;

				if (v != 1 && x < w && y < h && x > 0 && y > 0)
				{
					float oo = (float)(oos*Math.Sin(((max-v)/max)*2*Math.PI));
					double dx = Math.Abs(lx - x);
					double dy = Math.Abs(ly - y);
					if (dx <= dy)
					{
						double m2 = max/2;
						Color p = v > m2
							? ColorFade(v-m2,m2,FadeComp.B)
							: ColorFade(m2-v,m2,FadeComp.R);

						var np = ColorRGBA.FromRGBA255(p.R, p.G, p.B, p.A);
						dlf(image,np,new(lx,ly),new(x-oo,y-oo));
						dlf(image,np,new(ly,lx),new(y+oo,x+oo));
					}
					lx = x;
					ly = y;
				}
				progress.Report(v/max);
			}
		}
	}

	enum FadeComp { R, G, B }
	static Color ColorFade(double i, double max, FadeComp f)
	{
		double p,s;
		p = i < 1.0 * max / 2 ? 255
			: 255 - (i - max/2) * (255 / max * 2);
		s = 1.0 * i < max/2 ? 255 - i * (255 / max * 2) : 0;

		Color c;
		if (f == FadeComp.R) {
			c = Color.FromArgb(32,(int)p,(int)s,(int)s);
		} else if (f == FadeComp.G) {
			c = Color.FromArgb(32,(int)s,(int)p,(int)s);
		} else { //FadeComp.B
			c = Color.FromArgb(32,(int)s,(int)s,(int)p);
		}
		return c;
	}
}