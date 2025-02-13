using ImageFunctions.Core;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.SpearGraphic;

public static class Third
{
	public static void Twist1(ICanvas image, DrawLineFunc dlf, int w, int h, int? seed = null)
	{
		InitRandom(seed);

		int reps = 100;
		int margin = 10;
		DPoint curr = new DPoint(Random(margin, w - margin), Random(margin, h - margin));
		DPoint last = curr;
		int maxiter = 100;
		double mindist = 10.0;
		double angadd = 0.02;
		double speed = 2.0;
		double dir = 0;
		int nextRepDist = 400;
		int colorDistMax = 500;

		//var gop = new GraphicsOptions { Antialias = true };

		//DPoint[] ways = new DPoint[] {
		//	new DPoint(0
		//}
		DPoint dest = default(DPoint);
		using var progress = new ProgressBar();

		for(int r = 0; r < reps; r++) {
			int iter = 0;
			//random near current point
			dest = new DPoint(
				Random(PumpAt(margin, curr.X - nextRepDist), CapAt(curr.X + nextRepDist, w - margin))
				, Random(PumpAt(margin, curr.Y - nextRepDist), CapAt(curr.Y + nextRepDist, h - margin))
			);

			//random on a grid
			//DPoint ndest;
			//do {
			//	ndest = new DPoint(
			//		Decimate(Random(margin,w-margin),-2)
			//		,Decimate(Random(margin,h-margin),-2)
			//	);
			//} while(Dist(dest,ndest) < 10.0);
			//dest = ndest;

			//g.DrawArc(Pens.Red,(int)(dest.X-2),(int)(dest.Y-2),4,4,0,(int)(2*Math.PI));
			angadd = Random(0.01, 0.1);
			speed = Random(1.0, 5.0);
			mindist = speed / Math.Tan(angadd);
			FadeComp fcolor = RandEnum<FadeComp>();

			while(true) {
				if(++iter >= maxiter) { break; }
				double dist = Dist(dest, curr);
				//Console.WriteLine(dist);
				if(dist < mindist) { break; }
				//Pen p1 = RandomColorFade(dist,colorDistMax,false);
				//Pen p2 = RandomColorFade(dist,colorDistMax,true);
				Color p1 = ColorFade(PumpAt(colorDistMax - dist, 0), colorDistMax, fcolor);
				Color p2 = ColorFade(CapAt(dist, colorDistMax), colorDistMax, fcolor);

				var np1 = ColorRGBA.FromRGBA255(p1.R, p1.G, p1.B, p1.A);
				var np2 = ColorRGBA.FromRGBA255(p2.R, p2.G, p2.B, p2.A);

				dlf(image, np2, new(last.X, last.Y), new(curr.X, curr.Y));
				dlf(image, np2, new(dest.X, dest.Y), new(curr.X, curr.Y));
				DPoint ext = Move(curr, 2 * Math.PI - dir, 100);
				dlf(image, np1, new(ext.X, ext.Y), new(curr.X, curr.Y));

				//if (dist < 100 && angadd > 0.01) {
				//	angadd -= 0.01;
				//} else if (dist > 500 && angadd < 0.1) {
				//	angadd += 0.01;
				//}

				double newdirP = dir + angadd;
				double newdirN = dir - angadd;
				DPoint mP = Move(curr, newdirP, speed);
				DPoint mN = Move(curr, newdirN, speed);
				double distP = Dist(dest, mP);
				double distN = Dist(dest, mN);
				last = curr;
				if(distP < distN) {
					curr = mP;
					dir = newdirP;
				}
				else {
					curr = mN;
					dir = newdirN;
				}

				progress.Report((double)r / reps);
			}
		}
	}

	static double Dist(DPoint one, DPoint two)
	{
		double dist = Math.Sqrt(Math.Pow(one.X - two.X, 2) + Math.Pow(one.Y - two.Y, 2));
		return dist;
	}
	static DPoint Move(DPoint start, double ang, double dist)
	{
		double nx = dist * Math.Sin(ang) + start.X;
		double ny = dist * Math.Cos(ang) + start.Y;
		return new DPoint(nx, ny);
	}

	static double Decimate(double num, int precision)
	{
		if(precision >= 0) {
			return Math.Round(num, precision);
		}
		else {
			return Math.Floor(num * Math.Pow(10, precision)) / Math.Pow(10, precision);
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

	enum FadeComp { None = 0, R = 1, G = 2, B = 3 }

	static Color ColorFade(double i, double max, FadeComp f)
	{
		double p, s;
		p = i < 1.0 * max / 2 ? 255
			: 255 - (i - max / 2) * (255 / max * 2);
		s = 1.0 * i < max / 2 ? 255 - i * (255 / max * 2) : 0;

		Color c;
		if(f == FadeComp.R) {
			c = Color.FromArgb(32, (int)p, (int)s, (int)s);
		}
		else if(f == FadeComp.G) {
			c = Color.FromArgb(32, (int)s, (int)p, (int)s);
		}
		else {
			c = Color.FromArgb(32, (int)s, (int)s, (int)p);
		}
		return c;
	}

	static Color ColorMix(Color one, Color two)
	{
		int r = one.R + two.R / 2;
		int g = one.G + two.G / 2;
		int b = one.R + two.R / 2;
		int a = one.A + two.A / 2;

		Color f = Color.FromArgb(a, r, g, b);
		return f;
	}

	static Color RandomColorFade(double dist, double max, bool invert, FadeComp component = FadeComp.None)
	{
		double start = invert
			? PumpAt(max - dist, 0)
			: CapAt(dist, max)
		;

		if(component == FadeComp.None) {
			component = RandEnum<FadeComp>();
		}

		Color p = ColorFade(start, max, component);
		return p;
	}

	static E RandEnum<E>(bool SkipZero = true) where E : struct
	{
		var list = Enum.GetValues(typeof(E));
		int i = Random(SkipZero ? 1 : 0, list.Length + 1);
		return (E)((object)i);
	}

	static void InitRandom(int? seed = null)
	{
		if(rnd == null) {
			if(!seed.HasValue) {
				seed = (int)(DateTime.Now.Ticks - DateTime.Today.Ticks);
			}
			rnd = new Random(seed.Value);
		}
	}

	static Random rnd = null;
	static int Random(int low, int high)
	{
		InitRandom();
		return rnd.Next(low, high);
	}

	static double Random(double low, double high)
	{
		InitRandom();
		return rnd.NextDouble() * (high - low) + low;
	}
}

struct DPoint : IEquatable<DPoint>
{
	public DPoint(double x, double y)
	{
		X = x;
		Y = y;
	}

	public double X { get; private set; }
	public double Y { get; private set; }

	public bool Equals(DPoint other)
	{
		return this.X == other.X && this.Y == other.Y;
	}

	public override string ToString()
	{
		return "{X=" + X + ",Y=" + Y + "}";
	}
}
