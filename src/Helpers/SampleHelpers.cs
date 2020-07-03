using System;
using System.Collections.Concurrent;

namespace ImageFunctions.Helpers
{
	//https://github.com/ImageMagick/ImageMagick/blob/f775a5cf27a95c42bb6d19b50f4869db265fdaa9/MagickCore/resize.c

	public static class SampleHelpers
	{
		//Don't use this Map function, use Registry.Map instead
		internal static IFResampler Map(Sampler s)
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

			return new SamplerRaft(s,c);
		}

		delegate double SamplerCalc(double x);

		class SamplerRaft : IFResampler
		{
			public SamplerRaft(Sampler s, SamplerCalc calc) {
				Kind = s;
				Calc = calc;
			}

			public double Radius { get {
				return GetRadius(Kind);
			}}

			public double GetAmount(double x) {
				return Calc(x);
			}

			public Sampler Kind { get; private set; }
			SamplerCalc Calc;
		}

		public static IFColor GetSample(this IFImage img, IFResampler sampler, int x, int y, double scale = 1.0)
		{
			double o = sampler.Radius;
			if (o < 1.0) {
				return ImageHelpers.GetPixelSafe(img,x,y); //shortcut for NearestNeighbor
			}

			double[] kern = GetKernel(sampler.Kind,scale);

			int rad = (int)Math.Ceiling(o);
			int wmax = img.Width - 1;
			int hmax = img.Height - 1;
			x = Math.Clamp(x,0,wmax);
			y = Math.Clamp(y,0,hmax);

			int t = Math.Clamp(y - rad,0,hmax);
			int b = Math.Clamp(y + rad,0,hmax);
			int l = Math.Clamp(x - rad,0,wmax);
			int r = Math.Clamp(x + rad,0,wmax);

			double vr = 0.0,vg = 0.0,vb = 0.0,va = 0.0;
			for(int j = 0,v = t; v <= b; v++, j++) {
				double ur = 0.0,ug = 0.0,ub = 0.0,ua = 0.0;
				for(int i = 0, u = l; u <= r; u++, i++) {
					//multiply sample by kernel and add
					var c = img[u,v];
					ur += kern[i] * c.R;
					ug += kern[i] * c.G;
					ub += kern[i] * c.B;
					ua += kern[i] * c.A;
				}
				//multiply row by kernel and add
				vr += ur * kern[j];
				vg += ug * kern[j];
				vb += ub * kern[j];
				va += ua * kern[j];
			}

			return new IFColor(vr,vg,vb,va);
		}

		static double[] GetKernel(Sampler sampler, double scale)
		{
			//try to grab from cache first
			double[] kern;
			int hash = HashCode.Combine(sampler,scale);
			if (KernelCache.TryGetValue(hash,out kern)) {
				return kern;
			}
			var s = Map(sampler);

			double o = s.Radius;
			int rad = (int)Math.Ceiling(o);

			//create the 1D kernel and normalization factor
			kern = new double[2 * rad + 1];
			double norm = 0.0;

			// fill the kernel
			for(int v = 0; v < kern.Length; v++) {
				double w = s.GetAmount((v - rad) / scale);
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

		/*
		public static IFColor GetSample1(this IFImage img, IFResampler sampler, int x, int y, double? radius = null)
		{
			//TODO this function has not been tested
			double o = radius.HasValue
				? radius.Value
				: sampler.Radius
			;
			int wmax = img.Width - 1;
			int hmax = img.Height - 1;

			x = Math.Clamp(x,0,wmax);
			y = Math.Clamp(y,0,hmax);
			IFColor orig = img[x,y];
			int l = Math.Clamp((int)(x - o),0,wmax);
			int r = Math.Clamp((int)(x + o),0,wmax);
			int t = Math.Clamp((int)(y - o),0,hmax);
			int b = Math.Clamp((int)(y + o),0,hmax);
			if (l == r || t == b) { return orig; }

			var xW = new double[r - l];
			var yW = new double[b - t];
			FillWeights(xW,sampler,l,r,o);
			FillWeights(yW,sampler,t,b,o);

			double sumr = 0,sumg = 0,sumb = 0;
			for(int k = 0, v = t; v < b; v++, k++) {
				double wy = yW[k];
				for(int j = 0, u = l; u < r; u++, j++) {
					double wx = xW[j];
					IFColor pix = img[u,v];
					sumr += pix.R * wx * wy;
					sumg += pix.G * wx * wy;
					sumb += pix.B * wx * wy;
				}
			}

			var sum = new IFColor(sumr,sumg,sumb,orig.A);
			return sum;
		}

		static void FillWeights(double[] weights, IFResampler s,int min, int max, double o)
		{
			double cen = (max - min) / 2.0;
			for(int x=0, i = min; i< max; i++, x++) {
				double w = s.GetAmount(i - o);
				weights[x] = w;
			}
		}
		*/

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
			if (x <= 1.0) {
				// ((a + 2) * (x ^ 3)) - ((a + 3) * (x ^ 2)) + 1;
				return (1.5 * x - 2.5) * x * x + 1.0;
			}
			else if (x <= 2.0) {
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
		static double Cubic(double x, double b, double c)
		{
			if (x < 0.0) { x = -x; }
			double x2 = x * x;
			double x3 = x2 * x;

			if (x < 1.0) {
				double v = (18 * c + 36 * b - 54) * x2 - (18 * c +27 * b - 36) * x3 - b + 3.0;
				return v / 3.0;
			}
			else if (x < 2.0) {
				double v = (30 * c + 6 * b) * x2 - (6 * c + b) * x3 - (48 * c + 12 * b) * x + (24 * c) + (8 * b);
				return v / 6.0;
			}

			return 0.0;
		}

		const double OneThird = 1.0 / 3.0;
		const double Sqrt2 = 1.4142135623730950488016887242097;
		// https://www.imagemagick.org/discourse-server/viewtopic.php?p=78213&sid=1dc8c81c20fa3c2b4a2a12da630a53dc#p78213
		const double RB1 = (228.0 - 108.0 * Sqrt2) / 199.0;
		const double RB2 = (1.0 - RB1) / 2.0;
		const double RS1 = (78.0 - 42.0 * Sqrt2) / 71.0;
		const double RS2 = (42.0 * Sqrt2 - 7.0) / 142.0;

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