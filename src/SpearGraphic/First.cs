using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageFunctions.SpearGraphic
{
	public static class First<TPixel> where TPixel : struct, IPixel<TPixel>
	{
		public static void Twist1(ImageFrame<TPixel> image,int w,int h)
		{
			double max = w * 3;
			using (var progress = new ProgressBar())
			{
				for(int v=0; v<max; v++)
				{
					double x = (w/10.0)*Math.Cos(v*Math.PI/w)+(v/10.0)+(w/8.0);
					double y = (w/20.0)*Math.Sin(v*Math.PI/(w/2.0))+(v/10.0)+(w/8.0);
					int c = GreenFade(v,max);
					DrawDot(image,(int)x,(int)y,c);
					progress.Report((double)v/max);
				}
			}
		}

		public static void Twist2(ImageFrame<TPixel> image,int w,int h)
		{
			double max = w * 10;
			using (var progress = new ProgressBar())
			{
				for(int v=1; v<max; v++)
				{
					double a = (max - v)*Math.PI/(max/(max-v/32.0))+Math.PI;
					double x = (w/32.0)*Math.Cos(a)+(v/10.0);
					double y = (w/32.0)*Math.Sin(a)+(v/10.0);
					int c = GreenFade(v,max);
					DrawDot(image,(int)x,(int)y,c);
					progress.Report((double)v/max);
				}
			}
		}

		public static void Twist3(ImageFrame<TPixel> image, int w, int h)
		{
			double max = w * 9.5; //9,10,11 do different things
			double s = w/32.0;
			double o = w/10.0;
			double t = 12;
			using (var progress = new ProgressBar())
			{
				for(int v=1; v<max; v++)
				{
					double a = (max - v)*Math.PI/(max/(max - v/32.0))+Math.PI;
					double x = s*Math.Tan(a)+(v/t)+o;
					double y = s*Math.Sin(a)+(v/t)+o;
					int c = GreenFade(v,max);
					DrawDot(image,(int)x,(int)y,c);

					x = s*Math.Sin(a)+(v/t)+o;
					y = s*Math.Tan(a)+(v/t)+o;
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

		static void DrawDot(ImageFrame<TPixel> image, int x, int y, int c)
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

		static void SetPixel(ImageFrame<TPixel> image, int x, int y, int c)
		{
			var (r,g,b,a) = Orgb(c);
			var rgb = new Rgba32((byte)r,(byte)g,(byte)b,(byte)a);
			//Log.Debug($"setpixel [{x},{y}]={rgb.ToHex()} ({r},{g},{b},{a})");
			TPixel p = default(TPixel);
			p.FromRgba32(rgb);

			var span = image.GetPixelSpan();
			int off = y * image.Width + x;
			if (off < 0 || off >= span.Length) { return; }
			span[off] = p;
		}
	}
}
