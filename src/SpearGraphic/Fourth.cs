using System;
using System.Drawing;
using ImageFunctions.Helpers;

namespace ImageFunctions.SpearGraphic
{
	public static class Fourth
	{
		public static void Draw(IFImage image,int w,int h, int? seed = null)
		{
			InitRandom(seed);

			var p1 = new Twist1Params {
				RadRateMax = 0.5
				,RadRatemin = 0.01
				,RotRate = 0.05
				,MaxRevs = 200
				,PenStart = Color.FromArgb(0,255,0,0)
				,PenEnd = Color.FromArgb(127,255,0,0)
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
				,PenStart = Color.FromArgb(0,127,192,0)
				,PenEnd = Color.FromArgb(127,192,127,0)
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
				,PenStart = Color.FromArgb(0,192,127,0)
				,PenEnd = Color.FromArgb(127,192,127,0)
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
			public Color PenEnd { get; set; }
			public Color PenStart { get; set; }
		}

		static void Twist1(IFImage image, int w, int h, Twist1Params p)
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

			//var gop = new GraphicsOptions { Antialias = true };

			using (var progress = new ProgressBar())
			{
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

					Color c = TweenColor(p.PenEnd,p.PenStart,maxrad,0,rad);
					DrawLine(image,c,lx,ly,x,y,penw);
					//Rgba32 pen = new Pen(c,(float)penw);
					//g.DrawLine(pen,(float)lx,(float)ly,(float)x,(float)y);
					/* //TODO need replacement for line drawing
					image.Mutate(op => {
						var p0 = new PointF((float)lx,(float)ly);
						var p1 = new PointF((float)x,(float)y);
						op.DrawLines(gop,c,(float)penw,p0,p1);
					});
					*/

					double dist = Dist(cen,new DPoint(x,y));
					if (dist < 1.0) {
						break;
					}
					progress.Report((maxrad - dist) / maxrad);

					lx = x; ly = y;
					ang += p.RotRate;
					if (ang > pi2d) {
						ang -= pi2d;
						if (--rev <= 0) { break; }
						//Console.WriteLine(rev);
					}
					double radrate = Random(p.RadRatemin,p.RadRateMax);
					rad = Math.Max(rad - radrate,0);
				}
			}
		}

		static void DrawLine(IFImage img,Color c,double x0, double y0, double x1, double y1, double w)
		{
			ImageHelpers.DrawLine(img,c,new PointD(x0,y0),new PointD(x1,y1),w);
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
			} else {
				c = Color.FromArgb(32,(int)s,(int)s,(int)p);
			}
			return c;
		}

		static Color ColorMix(Color one, Color two)
		{
			int r = one.R + two.R / 2;
			int g = one.G + two.G / 2;
			int b = one.R + two.R / 2;
			int a = one.A + two.A / 2;

			Color f = Color.FromArgb(a,r,g,b);
			return f;
		}

		static Color TweenColor(Color start, Color end, double max, double min, double curr)
		{
			double p = (curr - min) / (max - min);

			double a = (end.A - start.A) * p + start.A;
			double r = (end.R - start.R) * p + start.R;
			double g = (end.G - start.G) * p + start.G;
			double b = (end.B - start.B) * p + start.B;

			Color f = Color.FromArgb((int)a,(int)r,(int)g,(int)b);
			return f;
		}

		static Color RandomColorFade(double dist, double max, bool invert, FadeComp component = FadeComp.None)
		{
			double start = invert
				? PumpAt(max-dist,0)
				: CapAt(dist,max)
			;

			if (component == FadeComp.None) {
				component = RandEnum<FadeComp>();
			}

			Color p = ColorFade(start,max,component);
			return p;
		}

		static E RandEnum<E>(bool SkipZero = true) where E : struct
		{
			var list = Enum.GetValues(typeof(E));
			int i = Random(SkipZero ? 1 : 0,list.Length + 1);
			return (E)((object)i);
		}

		static Color RandomColor(int? alpha = null)
		{
			int a = alpha == null ? Random(0,255) : alpha.Value;

			Color color = Color.FromArgb(
				Random(0,255),Random(0,255),Random(0,255),a);
			return color;
		}

		static Point GetRandomPoint(int w,int h)
		{
			return new Point(Random(0,w),Random(0,h));
		}

		static void InitRandom(int? seed = null)
		{
			if (rnd == null) {
				if (!seed.HasValue) {
					seed = (int)(DateTime.Now.Ticks - DateTime.Today.Ticks);
				}
				rnd = new Random(seed.Value);
			}
		}

		static Random rnd = null;
		static int Random(int low, int high)
		{
			InitRandom();
			return rnd.Next(low,high);
		}

		static double Random(double low, double high)
		{
			InitRandom();
			return rnd.NextDouble() * (high-low) + low;
		}
	}
}