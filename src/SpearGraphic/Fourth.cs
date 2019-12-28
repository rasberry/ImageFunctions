using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ImageFunctions.SpearGraphic
{
	public static class Fourth<TPixel> where TPixel : struct, IPixel<TPixel>
	{
		public static void Draw(Image<TPixel> image,int w,int h)
		{
			var p1 = new Twist1Params {
				RadRateMax = 0.5
				,RadRatemin = 0.01
				,RotRate = 0.05
				,MaxRevs = 200
				,PenStart = new Rgba32(255,0,0,0)
				,PenEnd = new Rgba32(255,0,0,127)
				,PenWMin = 1.0
				,PenWMax = 10.0
				,PenRateMin = 0.1
				,PenRateMax = 3
			};

			Twist1(image,w,h,p1);

			var p2 = new Twist1Params {
				RadRateMax = 0.3
				,RadRatemin = 0.01
				,RotRate = 0.05
				,MaxRevs = 200
				,PenStart = new Rgba32(127,192,0,0)
				,PenEnd = new Rgba32(192,127,0,127)
				,PenWMin = 1.0
				,PenWMax = 10.0
				,PenRateMin = 0.1
				,PenRateMax = 3
			};

			Twist1(image,w,h,p2);

			var p3 = new Twist1Params {
				RadRateMax = 0.3
				,RadRatemin = 0.01
				,RotRate = 0.05
				,MaxRevs = 200
				,PenStart = new Rgba32(192,127,0,0)
				,PenEnd = new Rgba32(192,127,0,127)
				,PenWMin = 1.0
				,PenWMax = 10.0
				,PenRateMin = 0.1
				,PenRateMax = 3
			};

			//Twist1(g,w,h,p3);

			//switch(op.Type)
			//{
			//case Twist.One: Twist1(g,w,h); break;
			////case Twist.Two: Twist4(g,w,h); break;
			//}
		}

		public class Twist1Params
		{
			public double RadRateMax { get; set; }
			public double RadRatemin { get; set; }
			public int MaxRevs { get; set; }
			public double RotRate { get; set; }
			public double PenWMax { get; set; }
			public double PenWMin { get; set; }
			public double PenRateMax { get; set; }
			public double PenRateMin { get; set; }
			public Rgba32 PenEnd { get; set; }
			public Rgba32 PenStart { get; set; }
		}

		static void Twist1(Image<TPixel> image, int w, int h, Twist1Params p)
		{
			DPoint cen = new DPoint(w / 2.0,h / 2.0);

			double ang = 0;
			double maxrad = Math.Min(cen.X,cen.Y);
			double rad = maxrad;
			double pi2d = Math.PI * 2.0f;
			float pi2f = (float)pi2d;
			double lx = cen.X + rad,ly = cen.Y;
			int rev = p.MaxRevs;
			double penwtarget = Random(p.PenWMin,p.PenWMax);
			double penrate = Random(p.PenRateMin,p.PenRateMax);
			double penw = penwtarget;

			var gop = new GraphicsOptions { Antialias = true };

			while(true)
			{
				double x = Math.Cos(ang) * rad + cen.X;
				double y = Math.Sin(ang) * rad + cen.Y;

				if (Math.Abs(penw - penwtarget) < p.PenRateMax) {
					penwtarget = Random(p.PenWMin,p.PenWMax);
					penrate = Random(p.PenRateMin,p.PenRateMax);
					if (penw > penwtarget) { penrate = -penrate; }
				}
				penw += penrate;

				Rgba32 c = TweenColor(p.PenEnd,p.PenStart,maxrad,0,rad);
				//Rgba32 pen = new Pen(c,(float)penw);
				//g.DrawLine(pen,(float)lx,(float)ly,(float)x,(float)y);
				image.Mutate(op => {
					var p0 = new PointF((float)lx,(float)ly);
					var p1 = new PointF((float)x,(float)y);
					op.DrawLines(gop,c,(float)penw,p0,p1);
				});

				if (Dist(cen,new DPoint(x,y)) < 1.0) {
					break;
				}

				lx = x; ly = y;
				ang += p.RotRate;
				if (ang > pi2d) {
					ang -= pi2d;
					if (--rev <= 0) { break; }
					Console.WriteLine(rev);
				}
				double radrate = Random(p.RadRatemin,p.RadRateMax);
				rad = Math.Max(rad - radrate,0);
			}
		}

		static double Dist(DPoint one,DPoint two)
		{
			double dist = Math.Sqrt(Math.Pow(one.X - two.X,2) + Math.Pow(one.Y - two.Y,2));
			return dist;
		}
		static DPoint Move(DPoint start, double ang, double dist)
		{
			double nx = dist * Math.Sin(ang) + start.X;
			double ny = dist * Math.Cos(ang) + start.Y;
			return new DPoint(nx,ny);
		}

		static double Decimate(double num, int precision)
		{
			if (precision >= 0) {
				return Math.Round(num,precision);
			} else {
				return Math.Floor(num * Math.Pow(10,precision)) / Math.Pow(10,precision);
			}
		}

		static double CapAt(double num, double max)
		{
			return num < max ? num : max;
		}
		static double CapAt(int num, int max)
		{
			return num < max ? num : max;
		}
		static double PumpAt(double num, double min)
		{
			return num > min ? num : min;
		}
		static double PumpAt(int num, int min)
		{
			return num > min ? num : min;
		}

		enum FadeComp { None=0, R=1, G=2, B=3 }

		static Rgba32 ColorFade(double i, double max, FadeComp f)
		{
			double p,s;
			p = i < 1.0 * max / 2 ? 255
				: 255 - (i - max/2) * (255 / max * 2);
			s = 1.0 * i < max/2 ? 255 - i * (255 / max * 2) : 0;
			
			Rgba32 c;
			if (f == FadeComp.R) {
				c = new Rgba32((byte)p,(byte)s,(byte)s,32);
			} else if (f == FadeComp.G) {
				c = new Rgba32((byte)s,(byte)p,(byte)s,32);
			} else {
				c = new Rgba32((byte)s,(byte)s,(byte)p,32);
			}
			return c;
		}

		static Rgba32 ColorMix(Rgba32 one, Rgba32 two)
		{
			int r = one.R + two.R / 2;
			int g = one.G + two.G / 2;
			int b = one.R + two.R / 2;
			int a = one.A + two.A / 2;

			Rgba32 f = new Rgba32(r,g,b,a);
			return f;
		}

		static Rgba32 TweenColor(Rgba32 start, Rgba32 end, double max, double min, double curr)
		{
			double p = (curr - min) / (max - min);

			double a = (end.A - start.A) * p + start.A;
			double r = (end.R - start.R) * p + start.R;
			double g = (end.G - start.G) * p + start.G;
			double b = (end.B - start.B) * p + start.B;

			return new Rgba32((byte)r,(byte)g,(byte)b,(byte)a);
		}

		static Rgba32 RandomColorFade(double dist, double max, bool invert, FadeComp component = FadeComp.None)
		{
			double start = invert
				? PumpAt(max-dist,0)
				: CapAt(dist,max)
			;

			if (component == FadeComp.None) {
				component = RandEnum<FadeComp>();
			}

			Rgba32 p = ColorFade(start,max,component);
			return p;
		}

		static E RandEnum<E>(bool SkipZero = true) where E : struct
		{
			var list = Enum.GetValues(typeof(E));
			int i = Random(SkipZero ? 1 : 0,list.Length + 1);
			return (E)((object)i);
		}

		static Rgba32 RandomColor(int? alpha = null)
		{
			int a = alpha == null ? Random(0,255) : alpha.Value;

			Rgba32 color = new Rgba32(
				(byte)Random(0,255),(byte)Random(0,255),(byte)Random(0,255),(byte)a);
			return color;
		}
		
		static Point GetRandomPoint(int w,int h)
		{
			return new Point(Random(0,w),Random(0,h));
		}
		
		static Random rnd = null;
		static int Random(int low, int high)
		{
			int seed = (int)(DateTime.Now.Ticks - DateTime.Today.Ticks);
			if (rnd == null) {
				rnd = new Random(seed);
			}
			return rnd.Next(low,high);
		}

		static double Random(double low, double high)
		{
			int seed = (int)(DateTime.Now.Ticks - DateTime.Today.Ticks);
			if (rnd == null) {
				rnd = new Random(seed);
			}

			return rnd.NextDouble() * (high-low) + low;
		}
	}
}