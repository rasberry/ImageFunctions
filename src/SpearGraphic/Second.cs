using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageFunctions.SpearGraphic
{
	public static class Second<TPixel> where TPixel : struct, IPixel<TPixel>
	{
		//private static void Draw(Options op)
		//{
		//	int w = op.Width;
		//	int h = op.Height;
		//	Bitmap bitmap = new Bitmap(w,h,PixelFormat.Format32bppArgb);
		//	Graphics g = Graphics.FromImage(bitmap);
//
		//	g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
		//	g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
		//	g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
		//	
		//	g.Clear(Color.Black);
		//	switch(op.Type)
		//	{
		//	case Twist.One: Twist3(g,w,h); break;
		//	case Twist.Two: Twist4(g,w,h); break;
		//	}
//
		//	bitmap.Save(op.Name,ImageFormat.Png);
		//}
		
		
		private static void Twist3(ImageFrame<TPixel> image, int w, int h)
		{
			int which = 2;
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

		private static void Twist3Params (ImageFrame<TPixel> image, int w, int h, double max, double s, double o, double t, double aa, double am, double an)
		{
			double a,x,y,ox,oy;
			for(double v=1; v<max; v++)
			{
				ox = o+(an+(am*(max-v)/max))*Math.Cos(v*Math.PI/aa);
				oy = o+(an+(am*(max-v)/max))*Math.Sin(v*Math.PI/aa);
				
				a = (max - v)*Math.PI/(max / (max - v/32)) + Math.PI;
				x = s * Math.Tan(a) + (v/t) + ox;
				y = s * Math.Sin(a) + (v/t) + oy;
				Pen p = ColorFade(v,max,FadeComp.G);
				if (x < w && y < h && x > 0 && y > 0) {
					g.DrawRectangle(p,(float)x,(float)y,0.5f,0.5f);
					g.DrawRectangle(p,(float)y,(float)x,0.5f,0.5f);
				}
			}
		}

		private static void Twist4(Graphics g, int w, int h)
		{
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
						Pen p = v > m2 ? ColorFade(v-m2,m2,FadeComp.B) : ColorFade(m2-v,m2,FadeComp.R);
						
						g.DrawLine(p,(float)lx,(float)ly,(float)x-oo,(float)y-oo);
						g.DrawLine(p,(float)ly,(float)lx,(float)y+oo,(float)x+oo);
					}
					lx = x;
					ly = y;
				}
			}
		}
		
		private enum FadeComp { R, G, B }
		private static Pen ColorFade(double i, double max, FadeComp f)
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
			} else {
				c = Color.FromArgb(32,(int)s,(int)s,(int)p);
			}
			return new Pen(c);
		}

		private static Pen GetRandomPen()
		{
			Color color = Color.FromArgb(
				Random(0,255),Random(0,255),Random(0,255),Random(0,255));
			return new Pen(color,5.0f);
		}
		
		private static Point GetRandomPoint(int w,int h)
		{
			return new Point(Random(0,w),Random(0,h));
		}
		
		private static Random rnd = null;
		private static int Random(int low, int high)
		{
			int seed = (int)(DateTime.Now.Ticks - DateTime.Today.Ticks);
			if (rnd == null) {
				rnd = new Random(seed);
			}
			return rnd.Next(low,high);
		}
	}
}