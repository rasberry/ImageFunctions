using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ImageFunctions.SpearGraphic
{
	public static class Second<TPixel> where TPixel : struct, IPixel<TPixel>
	{
		public static void Twist3(Image<TPixel> image, int w, int h, int which = 2)
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

		static void Twist3Params(Image<TPixel> image, int w, int h, double max, double s, double o, double t, double aa, double am, double an)
		{
			double a,x,y,ox,oy;
			var gop = new GraphicsOptions {
				Antialias = true
			};
			for(double v=1; v<max; v++)
			{
				ox = o+(an+(am*(max-v)/max))*Math.Cos(v*Math.PI/aa);
				oy = o+(an+(am*(max-v)/max))*Math.Sin(v*Math.PI/aa);
				
				a = (max - v)*Math.PI/(max / (max - v/32)) + Math.PI;
				x = s * Math.Tan(a) + (v/t) + ox;
				y = s * Math.Sin(a) + (v/t) + oy;
				Rgba32 p = ColorFade(v,max,FadeComp.G);
				if (x < w && y < h && x > 0 && y > 0) {
					image.Mutate(op => {
						DrawRectangle(op,gop,p,x,y);
						DrawRectangle(op,gop,p,y,x);
					});
				}
			}
		}

		static void DrawRectangle(IImageProcessingContext op, GraphicsOptions gop, Rgba32 p,double x,double y)
		{
			PointF p0 = new PointF((float)x + 0.0f,(float)y + 0.0f);
			PointF p1 = new PointF((float)x + 0.5f,(float)y + 0.0f);
			PointF p2 = new PointF((float)x + 0.5f,(float)y + 0.5f);
			PointF p3 = new PointF((float)x + 0.0f,(float)y + 0.5f);
			op.DrawPolygon(gop,(Color)p,1.0f,p0,p1,p2,p3);
		}

		public static void Twist4(Image<TPixel> image, int w, int h)
		{
			var gop = new GraphicsOptions {
				Antialias = true,
			};

			double max = w*9;
			double s = w/32.0; //stretch x
			double o = w/4.0; //offset
			double t = 20; //stretch y
			double a,x,y;
			double lx=0,ly=0;
			double oos = w * 0.088; //offset size
			
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
						Rgba32 p = v > m2 ? ColorFade(v-m2,m2,FadeComp.B) : ColorFade(m2-v,m2,FadeComp.R);
						
						image.Mutate(op => {
							DrawLine(op,gop,p,lx,ly,x-oo,y-oo);
							DrawLine(op,gop,p,ly,lx,y+oo,x+oo);
						});
					}
					lx = x;
					ly = y;
				}
			}
		}

		static void DrawLine(IImageProcessingContext op, GraphicsOptions gop,Rgba32 p, double x0,double y0,double x1,double y1)
		{
			var p0 = new PointF((float)x0,(float)y0);
			var p1 = new PointF((float)x1,(float)y1);
			op.DrawLines(gop,(Color)p,1.0f,p0,p1);
		}
		
		enum FadeComp { R, G, B }
		static Rgba32 ColorFade(double i, double max, FadeComp f)
		{
			double p,s;
			p = i < 1.0 * max / 2 ? 255
				: 255 - (i - max/2) * (255 / max * 2);
			s = 1.0 * i < max/2 ? 255 - i * (255 / max * 2) : 0;
			
			Color c;
			if (f == FadeComp.R) {
				c = new Rgba32((byte)p,(byte)s,(byte)s,32);
			} else if (f == FadeComp.G) {
				c = new Rgba32((byte)s,(byte)p,(byte)s,32);
			} else { //FadeComp.B
				c = new Rgba32((byte)s,(byte)s,(byte)p,32);
			}
			return c;
		}
	}
}