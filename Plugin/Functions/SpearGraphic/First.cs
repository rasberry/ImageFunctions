using System;
using System.Drawing;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.SpearGraphic;

public static class First
{
	//-1mm - max multiple
	//-1dw - divisor for width
	//-1dh - divisor for height
	public static void Twist1(ICanvas image,int w,int h)
	{
		double maxmult = 6;
		double vdiv = 10.0;
		double wdiv = 8.0;
		double xdiv = 10.0;
		double ydiv = 20.0;
		double xyr = 2.0;

		double max = w * maxmult;
		using var progress = new ProgressBar();
		for(int v=0; v<max; v++)
		{
			double add = (v/vdiv)+(w/wdiv);
			double x = (w/xdiv)*Math.Cos(v*Math.PI/w)+add;
			double y = (w/ydiv)*Math.Sin(v*Math.PI/(w/xyr))+add;
			int c = GreenFade(v,max);
			DrawDot(image,(int)x,(int)y,c);
			progress.Report((double)v/max);
		}
	}

	public static void Twist2(ICanvas image,int w,int h)
	{
		double maxmult = 10;
		double wdiv = 32.0;
		double vdiv = 10.0;

		double max = w * maxmult;
		using (var progress = new ProgressBar())
		{
			for(int v=1; v<max; v++)
			{
				double a = (max - v)*Math.PI/(max/(max-v/wdiv))+Math.PI;
				double x = (w/wdiv)*Math.Cos(a)+(v/vdiv);
				double y = (w/wdiv)*Math.Sin(a)+(v/vdiv);
				int c = GreenFade(v,max);
				DrawDot(image,(int)x,(int)y,c);
				progress.Report(v / max);
			}
		}
	}

	public static void Twist3(ICanvas image, int w, int h)
	{
		double maxmult = 9.5;  //9,10,11 do different things
		double wdiv = 32.0;
		double vdiv = 10.0;
		double tnum = 12.0;

		double max = w * maxmult;
		double s = w/wdiv;
		double o = w/vdiv;

		using (var progress = new ProgressBar())
		{
			for(int v=1; v<max; v++)
			{
				double add = (v/tnum)+o;
				double a = (max - v)*Math.PI/(max/(max - v/wdiv))+Math.PI;
				double x = s*Math.Tan(a)+add;
				double y = s*Math.Sin(a)+add;
				int c = GreenFade(v,max);
				DrawDot(image,(int)x,(int)y,c);

				x = s*Math.Sin(a)+add;
				y = s*Math.Tan(a)+add;
				DrawDot(image,(int)x,(int)y,c);

				progress.Report((double)v/max);
			}
		}
	}

	static (double,double,double) Hsv2Rgb(double h,double s,double v)
	{
		s /= 256.0;
		if (s == 0.0) { return (v,v,v); }
		h /= (256.0 / 6.0);
		int i = (int)Math.Floor(h);
		double f = h - i;
		double p = Math.Floor(v * (1.0 - s));
		double q = Math.Floor(v * (1.0 - s * f));
		double t = Math.Floor(v * (1.0 - s * (1.0 - f)));
		switch(i) {
		case 0: return (v,t,p);
		case 1: return (q,v,p);
		case 2: return (p,v,t);
		case 3: return (p,q,v);
		case 4: return (t,p,v);
		default: return (v,p,q);
		}
	}

	static (int,int,int,int) Orgb(int color)
	{
		int r = (color >> 16) & 255;
		int g = (color >> 8) & 255;
		int b = color & 255;
		int a = (color >> 24) & 127;
		return (r,g,b,a);
	}

	static int Irgb(int r,int g,int b,int a=127)
	{
		return ((a & 127) << 24)
			+ ((r & 255) << 16)
			+ ((g & 255) << 8)
			+ (b & 255);
	}

	static int GreenFade(double i, double max)
	{
		double g = i < 1.0 * max/2.0
			? 255.0
			: 255.0 - (i - max/2.0) * (255.0 / max * 2.0)
		;
		double b = 1.0 * i < max/2 ? 255.0 - i * (255.0 / max * 2.0) : 0.0;
		double r = b;

		return Irgb((int)r,(int)g,(int)b);
	}

	static int MoveAlpha(int color, double alphaChangePercent)
	{
		var (r,g,b,a) = Orgb(color);
		a = (int)(a * (1.0 - alphaChangePercent));
		return Irgb(r,g,b,a);
	}

	static void DrawDot(ICanvas image, int x, int y, int c)
	{
		int c40 = MoveAlpha(c,0.6);
		SetPixel(image,x+1,y,c40);
		SetPixel(image,x-1,y,c40);
		SetPixel(image,x,y+1,c40);
		SetPixel(image,x,y-1,c40);

		int c20 = MoveAlpha(c,0.4);
		SetPixel(image,x+1,y+1,c20);
		SetPixel(image,x+1,y-1,c20);
		SetPixel(image,x-1,y+1,c20);
		SetPixel(image,x-1,y-1,c20);

		SetPixel(image,x,y,MoveAlpha(c,1.0));
	}

	static void SetPixel(ICanvas image, int x, int y, int c)
	{
		if (x < 0 || y < 0 || x >= image.Width || y >= image.Height) {
			return;
		}
		var (r,g,b,a) = Orgb(c);
		var nc = ColorRGBA.FromRGBA255((byte)r,(byte)g,(byte)b,(byte)(255 - a));
		image[x,y] = nc;
	}
}
