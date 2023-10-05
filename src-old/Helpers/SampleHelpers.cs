using System;
using System.Collections.Concurrent;

namespace ImageFunctions.Helpers
{
	//https://github.com/ImageMagick/ImageMagick/blob/f775a5cf27a95c42bb6d19b50f4869db265fdaa9/MagickCore/resize.c

	public static class SampleHelpers
	{
		//Don't use this Map function outside of this class, use Registry.Map instead
		internal static ISampler Map(Sampler s,double scale, PickEdgeRule edgeRule)
		{
			SamplerCalc c = null;
			switch(s) {
			case Sampler.Bicubic:           c = Bicubic; break;
			case Sampler.Box:               c = Box; break;
			case Sampler.CatmullRom:        c = CatmullRom; break;
			case Sampler.Hermite:           c = Hermite; break;
			case Sampler.Lanczos2:          c = Lanczos2; break;
			case Sampler.Lanczos3:          c = Lanczos3; break;
			case Sampler.Lanczos5:          c = Lanczos5; break;
			case Sampler.Lanczos8:          c = Lanczos8; break;
			case Sampler.MitchellNetravali: c = MitchellNetravali; break;
			case Sampler.NearestNeighbor:   c = NearestNeighbor; break;
			case Sampler.Robidoux:          c = Robidoux; break;
			case Sampler.RobidouxSharp:     c = RobidouxSharp; break;
			case Sampler.Spline:            c = Spline; break;
			case Sampler.Triangle:          c = Triangle; break;
			case Sampler.Welch:             c = Welch; break;
			}

			return new SamplerRaft(s,c,scale,edgeRule);
		}

		delegate double SamplerCalc(double x);
		delegate IColor GetPixelFunc(IImage img,int x,int y);

		//wrap everything up in a hidden class
		class SamplerRaft : ISampler
		{
			public SamplerRaft(Sampler s, SamplerCalc calc, double scale, PickEdgeRule edgeRule) {
				Kind = s;
				Calc = calc;
				Scale = scale;
				EdgeRule = edgeRule;
				GetPixelRule = MapEdgeRule(edgeRule);
			}

			public double Radius { get {
				return GetRadius(Kind);
			}}

			public double GetKernelAt(double x) {
				return Calc(x);
			}

			public IColor GetSample(IImage img, int x, int y)
			{
				return SampleHelpers.GetSample(img,this,x,y,GetPixelRule);
			}

			public Sampler Kind { get; private set; }
			public double Scale { get; private set; }
			public PickEdgeRule EdgeRule { get; private set; }
			SamplerCalc Calc;
			GetPixelFunc GetPixelRule;
		}

		static GetPixelFunc MapEdgeRule(PickEdgeRule rule)
		{
			switch(rule)
			{
			case PickEdgeRule.Transparent: return EdgeTransparent;
			case PickEdgeRule.Tile: return EdgeTile;
			case PickEdgeRule.Reflect: return EdgeReflect;
			default: return EdgeClamp;
			}
		}

		static IColor EdgeReflect(IImage img, int x,int y)
		{
			if (x < 0) { x = -x; }
			if (x >= img.Width) { x = x - 2*img.Width - 1; }
			if (y < 0) { y = -y; }
			if (y >= img.Height) { y = y - 2*img.Height - 1; }
			x %= img.Width;
			y %= img.Height;
			return img[x,y];
		}

		static IColor EdgeClamp(IImage img, int x,int y)
		{
			x = Math.Clamp(x,0,img.Width - 1);
			y = Math.Clamp(y,0,img.Height - 1);
			return img[x,y];
		}

		static IColor EdgeTransparent(IImage img, int x, int y)
		{
			if (x < 0 || x >= img.Width || y < 0 || y >= img.Width) {
				return ColorHelpers.Transparent;
			}
			return img[x,y];
		}

		static IColor EdgeTile(IImage img, int x, int y)
		{
			x %= img.Width;
			y %= img.Height;
			if (x < 0) { x += img.Width; }
			if (y < 0) { y += img.Height; }
			return img[x,y];
		}

		static IColor GetSample(IImage img, ISampler sampler, int x, int y, GetPixelFunc pixFunc)
		{
			double o = sampler.Radius;
			if (o < double.Epsilon) {
				return SampleHelpers.EdgeClamp(img,x,y); //shortcut for NearestNeighbor
			}

			int rad = (int)Math.Ceiling(o);
			double[] kern = GetKernel(sampler);

			int t = y - rad;
			int b = y + rad;
			int l = x - rad;
			int r = x + rad;

			double vr = 0.0,vg = 0.0,vb = 0.0,va = 0.0;
			//double sumj = 0.0;
			for(int j = 0,v = t; v <= b; v++, j++) {
				double ur = 0.0,ug = 0.0,ub = 0.0,ua = 0.0;
				//double sumi = 0.0;
				for(int i = 0, u = l; u <= r; u++, i++) {
					//multiply sample by kernel and add
					var c = pixFunc(img,u,v);
					ur += kern[i] * c.R;
					ug += kern[i] * c.G;
					ub += kern[i] * c.B;
					ua += kern[i] * c.A;
					//sumi += kern[i];
				}
				//multiply row by kernel and add
				vr += ur * kern[j]; // / sumi;
				vg += ug * kern[j]; // / sumi;
				vb += ub * kern[j]; // / sumi;
				va += ua * kern[j]; // / sumi;
				//sumj += kern[j];
			}
			//vr /= sumj; vg /= sumj; vg /= sumj; va /= sumj;

			return new IColor(vr,vg,vb,va);
		}

		static double[] GetKernel(ISampler sampler)
		{
			double scale = sampler.Scale;
			//try to grab from cache first
			double[] kern;
			int hash = HashCode.Combine(sampler.Kind,scale);
			if (KernelCache.TryGetValue(hash,out kern)) {
				return kern;
			}

			double o = sampler.Radius;
			int rad = (int)Math.Ceiling(o);

			//create the 1D kernel and normalization factor
			kern = new double[2 * rad + 1];
			double norm = 0.0;

			// fill the kernel
			for(int v = 0; v < kern.Length; v++) {
				double w = sampler.GetKernelAt((v - rad) / scale);
				kern[v] = w;
				norm += w;
			}
			//normalize kernel values
			if (norm > 0.0) {
				for(int v = 0; v < kern.Length; v++) {
					kern[v] /= norm;
				}
			}

			//cache it
			KernelCache.TryAdd(hash,kern);
			return kern;
		}

		static ConcurrentDictionary<int,double[]> KernelCache =
			new ConcurrentDictionary<int, double[]>();

		static double GetRadius(Sampler s)
		{
			switch(s) {
			case Sampler.Bicubic:           return 2.0;
			case Sampler.Box:               return 0.5;
			case Sampler.CatmullRom:        return 2.0;
			case Sampler.Hermite:           return 2.0;
			case Sampler.Lanczos2:          return 2.0;
			case Sampler.Lanczos3:          return 3.0;
			case Sampler.Lanczos5:          return 5.0;
			case Sampler.Lanczos8:          return 8.0;
			case Sampler.MitchellNetravali: return 2.0;
			case Sampler.NearestNeighbor:   return 0.0;
			case Sampler.Robidoux:          return 2.0;
			case Sampler.RobidouxSharp:     return 2.0;
			case Sampler.Spline:            return 2.0;
			case Sampler.Triangle:          return 1.0;
			case Sampler.Welch:             return 1.0;
			}

			return 0.0;
		}

		static double NearestNeighbor(double x)
		{
			return 1.0;
		}

		// https://en.wikipedia.org/wiki/Bicubic_interpolation#Bicubic_convolution_algorithm
		static double Bicubic(double x)
		{
			if (x < 0.0) { x = -x; }
			if (x < 1.0) {
				// ((a + 2) * (x ^ 3)) - ((a + 3) * (x ^ 2)) + 1;
				return (1.5 * x - 2.5) * x * x + 1.0;
			}
			else if (x < 2.0) {
				// (a * (x ^ 3)) - (5 * a * (x ^ 2)) + (8 * a * x) - (4 * a)
				return (((-0.5 * x) + 2.5) * x - 4.0) * x + 2.0;
			}
			return 0.0;
		}

		// http://www.imagemagick.org/Usage/filter/#box
		static double Box(double x)
		{
			if (x > -0.5 && x <= 0.5) {
				return 1.0;
			}
			return 0.0;
		}

		// http://www.imagemagick.org/Usage/filter/#cubics
		// http://www.cs.utexas.edu/~fussell/courses/cs384g-fall2013/lectures/mitchell/Mitchell.pdf
		static double Cubic(double x, double b, double c)
		{
			if (x < 0.0) { x = -x; }
			double x2 = x * x;
			double x3 = x2 * x;

			if (x < 1.0) {
				double v = x3*(12 - 9*b - 6*c) + x2*(-18 + 12*b + 6*c) + (6 - 2*b);
				return v / 6.0;
			}
			else if (x < 2.0) {
				double v = x3*(-b - 6*c) + x2*(6*b + 30*c) + x*(-12*b - 48*c) + (8*b + 24*c);
				return v / 6.0;
			}

			return 0.0;
		}

		// https://www.imagemagick.org/discourse-server/viewtopic.php?p=78213&sid=1dc8c81c20fa3c2b4a2a12da630a53dc#p78213
		const double RB1 = (228.0 - 108.0 * Sqrt2) / 199.0;
		const double RB2 = (1.0 - RB1) / 2.0;
		const double RS1 = (78.0 - 42.0 * Sqrt2) / 71.0;
		const double RS2 = (42.0 * Sqrt2 - 7.0) / 142.0;
		const double OneThird = 1.0 / 3.0;
		const double Sqrt2 = 1.4142135623730950488016887242097;

		static double CatmullRom(double x)        { return Cubic(x, 0.0, 0.5); }
		static double Hermite(double x)           { return Cubic(x, 0.0, 0.0); }
		static double MitchellNetravali(double x) { return Cubic(x, OneThird, OneThird); }
		static double Robidoux(double x)          { return Cubic(x, RB1, RB2); }
		static double RobidouxSharp(double x)     { return Cubic(x, RS1, RS2); }
		static double Spline(double x)            { return Cubic(x, 1.0, 0.0); }

		static double Lanczos(double x, double rad)
		{
			if (x < 0.0) { x = -x; }
			if (x < rad) {
				return MathHelpers.SinC(x) * MathHelpers.SinC(x / rad);
			}
			return 0.0;
		}

		static double Lanczos2(double x) { return Lanczos(x,2.0); }
		static double Lanczos3(double x) { return Lanczos(x,3.0); }
		static double Lanczos5(double x) { return Lanczos(x,5.0); }
		static double Lanczos8(double x) { return Lanczos(x,8.0); }

		static double Triangle(double x)
		{
			if (x < 0.0) { x = -x; }
			if (x < 1.0) {
				return 1.0 - x;
			}
			return 0.0;
		}

		static double Welch(double x)
		{
			if (x < 0.0) { x = -x; }
			if (x < 1.0) {
				return 1.0 - x * x;
			}
			//if (x < 3.0) {
			//	return MathHelpers.SinC(x) * (1.0 - (x * x / 9.0));
			//}
			return 0.0;
		}
	}
}