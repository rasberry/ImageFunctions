using System;
using System.Numerics;

namespace ImageFunctions.Helpers
{
	//TODO look at https://easyrgb.com/en/convert.php
	// and http://www.brucelindbloom.com/
	// and http://members.chello.at/~easyfilter/colorspace.c

	public interface I3Color
	{
		double _1 { get; }
		double _2 { get; }
		double _3 { get; }
		double α { get; }
	}

	public interface I4Color : I3Color
	{
		double _4 { get; }
	}

	public interface IHasLuma
	{
		double 光 { get; }
	}

	public interface ILumaColorSpace
	{
		IHasLuma GetLuma(in IColor o);
	}

	public interface I3ColorSpace
	{
		I3Color ToSpace(in IColor o);
		IColor ToNative(in I3Color o);
	}

	public interface I4ColorSpace
	{
		I4Color ToSpace(in IColor o);
		IColor ToNative(in I4Color o);
	}

	public interface I3ColorSpace<T> : I3ColorSpace where T : I3Color
	{
		new T ToSpace(in IColor o);
		IColor ToNative(in T o);
	}
	public interface I4ColorSpace<T> : I4ColorSpace where T : I4Color
	{
		new T ToSpace(in IColor o);
		IColor ToNative(in T o);
	}

	// https://www.rapidtables.com/convert/color/
	public class ColorSpaceCmyk : I4ColorSpace<ColorSpaceCmyk.CMYK>
	{
		public IColor ToNative(in CMYK o)
		{
			double m = 1.0 - o.K;
			double r = (1.0 - o.C) * m;
			double g = (1.0 - o.M) * m;
			double b = (1.0 - o.Y) * m;

			return new IColor(r,g,b,o.α);
		}

		public CMYK ToSpace(in IColor o)
		{
			double max = Math.Max(Math.Max(o.R,o.G),o.B);
			double k = 1.0 - max;
			double den = 1.0 - k;
			double c,m,y;
			if (den < double.Epsilon) {
				c = 0.0; m = 0.0; y = 0.0;
			}
			else {
				c = (den - o.R) / den;
				m = (den - o.G) / den;
				y = (den - o.B) / den;
			}
			return new CMYK(c,m,y,k,o.A);
		}

		IColor I4ColorSpace.ToNative(in I4Color o) {
			return ToNative((CMYK)o);
		}
		I4Color I4ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public readonly struct CMYK : I4Color
		{
			public CMYK(double c, double m, double y, double k, double α = 1.0) {
				_C = c; _M = m; _Y = y; _K = k; _A = α;
			}
			readonly double _C,_M,_Y,_K,_A;

			public double _1 { get { return _C; }}
			public double _2 { get { return _M; }}
			public double _3 { get { return _Y; }}
			public double _4 { get { return _K; }}
			public double  C { get { return _C; }}
			public double  M { get { return _M; }}
			public double  Y { get { return _Y; }}
			public double  K { get { return _K; }}
			public double  α { get { return _A; }}
		}
	}

	public class ColorSpaceHSBase
	{
		protected double RgbToHue(double r,double g,double b,double c, double max)
		{
			if (c < double.Epsilon) { return 0.0; }

			double h = 0.0;
			if (max == r) {
				h = ((g - b) / c) % 6.0;
			}
			else if (max == g) {
				h = (b - r) / c + 2.0;
			}
			else if (max == b) {
				h = (r - g) / c + 4.0;
			}
			h /= 6.0; //normalize to [0.0,1.0]
			return h;
		}

		protected (double,double,double) HueToRgb(double h, double c, double m, double? z = null)
		{
			double x = z.HasValue
				? c * z.Value
				: c * (1.0 - Math.Abs((h * 6.0) % 2.0 - 1.0))
			;
			int d = (int)Math.Ceiling(h);

			double r,g,b;
			switch(d) {
			 case 0:
			 case 1: r = c; g = x; b = 0; break;
			 case 2: r = x; g = c; b = 0; break;
			 case 3: r = 0; g = c; b = x; break;
			 case 4: r = 0; g = x; b = c; break;
			 case 5: r = x; g = 0; b = c; break;
			 case 6: r = c; g = 0; b = x; break;
			default: r = 0; g = 0; b = 0; break;
			}

			r += m; g += m; b += m;
			return (r,g,b);
		}
	}

	// https://en.wikipedia.org/wiki/HSL_and_HSV
	public class ColorSpaceHsl : ColorSpaceHSBase, I3ColorSpace<ColorSpaceHsl.HSL>, ILumaColorSpace
	{
		public IColor ToNative(in HSL o)
		{
			double c = (1.0 - Math.Abs(2 * o.L - 1.0)) * o.S;
			double m = o.L - c / 2.0;
			var (r,g,b) = HueToRgb(o.H,c,m);
			return new IColor(r,g,b,o.α);
		}

		public HSL ToSpace(in IColor o)
		{
			double max = Math.Max(Math.Max(o.R,o.G),o.B);
			double min = Math.Min(Math.Min(o.R,o.G),o.B);
			double c = max - min;

			double l = (max + min) / 2.0;
			double h = RgbToHue(o.R,o.G,o.B,c,max);
			double s = l >= 1.0 || l < double.Epsilon ? 0.0 : c / (1.0 - Math.Abs(2 * l - 1.0));

			return new HSL(h,s,l,o.A);
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((HSL)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct HSL : I3Color, IHasLuma
		{
			public HSL(double h, double s, double l, double α = 1.0) {
				_H = h; _S = s; _L = l; _A = α;
			}
			readonly double _H,_S,_L,_A;

			public double _1 { get { return _H; }}
			public double _2 { get { return _S; }}
			public double _3 { get { return _L; }}
			public double  H { get { return _H; }}
			public double  S { get { return _S; }}
			public double  L { get { return _L; }}
			public double  α { get { return _A; }}
			public double  光 { get { return _L; }}
		}
	}

	// https://en.wikipedia.org/wiki/HSL_and_HSV
	public class ColorSpaceHsv : ColorSpaceHSBase, I3ColorSpace<ColorSpaceHsv.HSV>, ILumaColorSpace
	{
		public HSV ToSpace(in IColor o)
		{
			double max = Math.Max(Math.Max(o.R,o.G),o.B);
			double min = Math.Min(Math.Min(o.R,o.G),o.B);
			double c = max - min;

			double v = max;
			double h = RgbToHue(o.R,o.G,o.B,c,max);
			double s = max < double.Epsilon ? 0.0 : c / max;

			return new HSV(h,s,v,o.A);
		}

		public IColor ToNative(in HSV o)
		{
			double c = o.V * o.S;
			double m = o.V - c;
			var (r,g,b) = HueToRgb(o.H,c,m);
			return new IColor(r,g,b,o.α);
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((HSV)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct HSV : I3Color, IHasLuma
		{
			public HSV(double h, double s, double v, double α = 1.0) {
				_H = h; _S = s; _V = v; _A = α;
			}
			readonly double _H,_S,_V,_A;

			public double _1 { get { return _H; }}
			public double _2 { get { return _S; }}
			public double _3 { get { return _V; }}
			public double  H { get { return _H; }}
			public double  S { get { return _S; }}
			public double  V { get { return _V; }}
			public double  α { get { return _A; }}
			public double  光 { get { return _V; }}
		}
	}

	// https://en.wikipedia.org/wiki/HSL_and_HSV
	public class ColorSpaceHsi : ColorSpaceHSBase, I3ColorSpace<ColorSpaceHsi.HSI>, ILumaColorSpace
	{
		public HSI ToSpace(in IColor o)
		{
			double max = Math.Max(Math.Max(o.R,o.G),o.B);
			double min = Math.Min(Math.Min(o.R,o.G),o.B);
			double c = max - min;

			double i = (o.R + o.G + o.B) / 3.0;
			double h = RgbToHue(o.R,o.G,o.B,c,max);
			double s = i < double.Epsilon ? 0.0 : 1.0 - min / i;
			return new HSI(h,s,i,o.A);
		}

		public IColor ToNative(in HSI o)
		{
			double z = 1.0 - Math.Abs((o.H * 6.0) % 2.0 - 1.0);
			double c = 3.0 * o.I * o.S / (z + 1);
			double m = o.I * (1.0 - o.S);
			var (r,g,b) = HueToRgb(o.H,c,m,z);
			return new IColor(r,g,b,o.α);
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((HSI)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct HSI : I3Color, IHasLuma
		{
			public HSI(double h, double s, double i, double α = 1.0) {
				_H = h; _S = s; _I = i; _A = α;
			}
			readonly double _H,_S,_I,_A;

			public double _1 { get { return _H; }}
			public double _2 { get { return _S; }}
			public double _3 { get { return _I; }}
			public double  H { get { return _H; }}
			public double  S { get { return _S; }}
			public double  I { get { return _I; }}
			public double  α { get { return _A; }}
			public double  光 { get { return _I; }}
		}
	}

	// https://en.wikipedia.org/wiki/YIQ
	public class ColorSpaceYiq : I3ColorSpace<ColorSpaceYiq.YIQ>, ILumaColorSpace
	{
		public YIQ ToSpace(in IColor o)
		{
			double y = o.R *  0.2990 + o.G *  0.5870 + o.B *  0.1140;
			double i = o.R *  0.5959 + o.G * -0.2746 + o.B * -0.3213;
			double q = o.R *  0.2115 + o.G * -0.5227 + o.B *  0.3112;
			return new YIQ(y,i,q,o.A);
		}

		public IColor ToNative(in YIQ o)
		{
			double r = o.Y *  1.0 + o.I *  0.9690 + o.Q *  0.6190;
			double g = o.Y *  1.0 + o.I * -0.2720 + o.Q * -0.6470;
			double b = o.Y *  1.0 + o.I * -1.1060 + o.Q *  1.7030;
			return new IColor(r,g,b,o.α);
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((YIQ)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct YIQ : I3Color, IHasLuma
		{
			public YIQ(double y, double i, double q, double α = 1.0) {
				_Y = y; _I = i; _Q = q; _A = α;
			}
			readonly double _Y,_I,_Q,_A;

			public double _1 { get { return _Y; }}
			public double _2 { get { return _I; }}
			public double _3 { get { return _Q; }}
			public double  α { get { return _A; }}
			public double  Y { get { return _Y; }}
			public double  I { get { return _I; }}
			public double  Q { get { return _Q; }}
			public double  光 { get { return _Y; }}
		}
	}

	// https://en.wikipedia.org/wiki/YIQ
	public class ColorSpaceYiqFcc : I3ColorSpace<ColorSpaceYiqFcc.YIQ>, ILumaColorSpace
	{
		public YIQ ToSpace(in IColor o)
		{
			double y = o.R *  0.3000 + o.G *  0.5900 + o.B *  0.1100;
			double i = o.R *  0.5990 + o.G * -0.2773 + o.B * -0.3217;
			double q = o.R *  0.2130 + o.G * -0.5251 + o.B *  0.3121;
			return new YIQ(y,i,q,o.A);
		}

		public IColor ToNative(in YIQ o)
		{
			double r = o.Y *  1.0 + o.I *  0.9469 + o.Q *  0.6236;
			double g = o.Y *  1.0 + o.I * -0.2748 + o.Q * -0.6357;
			double b = o.Y *  1.0 + o.I * -1.1000 + o.Q *  1.7000;
			return new IColor(r,g,b,o.α);
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((YIQ)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct YIQ : I3Color, IHasLuma
		{
			public YIQ(double y, double i, double q, double α = 1.0) {
				_Y = y; _I = i; _Q = q; _A = α;
			}
			readonly double _Y,_I,_Q,_A;

			public double _1 { get { return _Y; }}
			public double _2 { get { return _I; }}
			public double _3 { get { return _Q; }}
			public double  α { get { return _A; }}
			public double  Y { get { return _Y; }}
			public double  I { get { return _I; }}
			public double  Q { get { return _Q; }}
			public double  光 { get { return _Y; }}
		}
	}

	// https://en.wikipedia.org/wiki/YUV
	public class ColorSpaceYuvBT601 : I3ColorSpace<ColorSpaceYuvBT601.YUV>, ILumaColorSpace
	{
		public YUV ToSpace(in IColor o)
		{
			double y = o.R *  0.29900 + o.G *  0.58700 + o.B *  0.11400;
			double i = o.R * -0.14713 + o.G * -0.28886 + o.B *  0.43600;
			double q = o.R *  0.61500 + o.G * -0.51499 + o.B * -0.10001;
			return new YUV(y,i,q,o.A);
		}

		public IColor ToNative(in YUV o)
		{
			double r = o.Y *  1.0 + o.U *  0.00000 + o.V *  1.13983;
			double g = o.Y *  1.0 + o.U * -0.39465 + o.V * -0.58060;
			double b = o.Y *  1.0 + o.U *  2.03211 + o.V *  0.00000;
			return new IColor(r,g,b,o.α);
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((YUV)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct YUV : I3Color, IHasLuma
		{
			public YUV(double y, double u, double v, double α = 1.0) {
				_Y = y; _U = u; _V = v; _A = α;
			}
			readonly double _Y,_U,_V,_A;

			public double _1 { get { return _Y; }}
			public double _2 { get { return _U; }}
			public double _3 { get { return _V; }}
			public double  α { get { return _A; }}
			public double  Y { get { return _Y; }}
			public double  U { get { return _U; }}
			public double  V { get { return _V; }}
			public double  光 { get { return _Y; }}
		}
	}

	// https://en.wikipedia.org/wiki/YUV
	public class ColorSpaceYuvBT709 : I3ColorSpace<ColorSpaceYuvBT709.YUV>, ILumaColorSpace
	{
		public YUV ToSpace(in IColor o)
		{
			double y = o.R *  0.21260 + o.G *  0.71520 + o.B *  0.07220;
			double i = o.R * -0.09991 + o.G * -0.33609 + o.B *  0.43600;
			double q = o.R *  0.61500 + o.G * -0.55861 + o.B * -0.05639;
			return new YUV(y,i,q,o.A);
		}

		public IColor ToNative(in YUV o)
		{
			double r = o.Y *  1.0 + o.U *  0.00000 + o.V *  1.28033;
			double g = o.Y *  1.0 + o.U * -0.21482 + o.V * -0.38059;
			double b = o.Y *  1.0 + o.U *  2.12789 + o.V *  0.00000;
			return new IColor(r,g,b,o.α);
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((YUV)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct YUV : I3Color, IHasLuma
		{
			public YUV(double y, double u, double v, double α = 1.0) {
				_Y = y; _U = u; _V = v; _A = α;
			}
			readonly double _Y,_U,_V,_A;

			public double _1 { get { return _Y; }}
			public double _2 { get { return _U; }}
			public double _3 { get { return _V; }}
			public double  α { get { return _A; }}
			public double  Y { get { return _Y; }}
			public double  U { get { return _U; }}
			public double  V { get { return _V; }}
			public double  光 { get { return _Y; }}
		}
	}

	// https://en.wikipedia.org/wiki/YDbDr
	public class ColorSpaceYDbDr : I3ColorSpace<ColorSpaceYDbDr.YBR>, ILumaColorSpace
	{
		public YBR ToSpace(in IColor o)
		{
			double y = o.R *  0.299 + o.G *  0.587 + o.B *  0.114;
			double b = o.R * -0.450 + o.G * -0.883 + o.B *  1.333;
			double r = o.R * -1.333 + o.G *  1.116 + o.B *  0.217;
			return new YBR(y,b,r,o.A);
		}

		public IColor ToNative(in YBR o)
		{
			double r = o.Y *  1.0 + o.B *  0.000092303716148 + o.R * -0.525912630661865;
			double g = o.Y *  1.0 + o.B * -0.129132898890509 + o.R *  0.267899328207599;
			double b = o.Y *  1.0 + o.B *  0.664679059978955 + o.R * -0.000079808543533;
			return new IColor(r,g,b,o.α);
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((YBR)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct YBR : I3Color, IHasLuma
		{
			public YBR(double y, double b, double r, double α = 1.0) {
				_Y = y; _B = b; _R = r; _A = α;
			}
			public readonly double _Y,_B,_R,_A;

			public double _1 { get { return _Y; }}
			public double _2 { get { return _B; }}
			public double _3 { get { return _R; }}
			public double  α { get { return _A; }}
			public double  Y { get { return _Y; }}
			public double  B { get { return _B; }}
			public double  R { get { return _R; }}
			public double  光 { get { return _Y; }}
		}
	}

	public abstract class ColorSpaceYCbCrBase
	{
		protected static double Kr,Kg,Kb;
		protected static double F11,F21,F31,F12,F22,F32,F13,F23,F33;
		protected static double I11,I21,I31,I12,I22,I32,I13,I23,I33;

		protected static void InitMatrixValues()
		{
			F11 = Kr;                     F21 = Kg;                     F31 = Kb;
			F12 = -0.5 * Kr / (1.0 - Kb); F22 = -0.5 * Kg / (1.0 - Kb); F32 = 0.5;
			F13 = 0.5;                    F23 = -0.5 * Kg / (1.0 - Kr); F33 = -0.5 * Kb / (1.0 - Kr);

			I11 = 1.0; I21 = 0.0;                        I31 = 2.0 - 2.0 * Kr;
			I12 = 1.0; I22 = 2.0 * Kb / Kg * (Kb - 1.0); I32 = 2.0 * Kr / Kg * (Kr - 1.0);
			I13 = 1.0; I23 = 2.0 - 2.0 * Kb;             I33 = 0.0;
		}

		protected static (double,double,double) BaseToSpace(in IColor o)
		{
			double y = o.R * F11 + o.G * F21 + o.B * F31;
			double b = o.R * F12 + o.G * F22 + o.B * F32;
			double r = o.R * F13 + o.G * F23 + o.B * F33;
			return (y,b,r);
		}

		public IColor BaseToNative(in I3Color o)
		{
			double r = o._1 * I11 + o._2 * I21 + o._3 * I31;
			double g = o._1 * I12 + o._2 * I22 + o._3 * I32;
			double b = o._1 * I13 + o._2 * I23 + o._3 * I33;
			return new IColor(r,g,b,o.α);
		}
	}

	// https://en.wikipedia.org/wiki/YCbCr
	public class ColorSpaceYCbCrBt601 : ColorSpaceYCbCrBase,
		I3ColorSpace<ColorSpaceYCbCrBt601.YBR>, ILumaColorSpace
	{
		public YBR ToSpace(in IColor o)
		{
			var (y,b,r) = BaseToSpace(o);
			return new YBR(y,b,r,o.A);
		}

		public IColor ToNative(in YBR o)
		{
			return BaseToNative(o);
		}

		static ColorSpaceYCbCrBt601()
		{
			Kr = 0.299; Kg = 0.587; Kb = 0.114;
			InitMatrixValues();
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((YBR)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct YBR : I3Color, IHasLuma
		{
			public YBR(double y, double b, double r, double α = 1.0) {
				_Y = y; _B = b; _R = r; _A = α;
			}
			public readonly double _Y,_B,_R,_A;

			public double _1 { get { return _Y; }}
			public double _2 { get { return _B; }}
			public double _3 { get { return _R; }}
			public double  α { get { return _A; }}
			public double  Y { get { return _Y; }}
			public double  B { get { return _B; }}
			public double  R { get { return _R; }}
			public double  光 { get { return _Y; }}
		}
	}

	// https://en.wikipedia.org/wiki/YCbCr
	public class ColorSpaceYCbCrBt709 : ColorSpaceYCbCrBase,
		I3ColorSpace<ColorSpaceYCbCrBt709.YBR>, ILumaColorSpace
	{
		public YBR ToSpace(in IColor o)
		{
			var (y,b,r) = BaseToSpace(o);
			return new YBR(y,b,r,o.A);
		}

		public IColor ToNative(in YBR o)
		{
			return BaseToNative(o);
		}

		static ColorSpaceYCbCrBt709()
		{
			Kr = 0.2126; Kb = 0.0722; Kg = 1.0 - Kb - Kr;
			InitMatrixValues();
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((YBR)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct YBR : I3Color, IHasLuma
		{
			public YBR(double y, double b, double r, double α = 1.0) {
				_Y = y; _B = b; _R = r; _A = α;
			}
			public readonly double _Y,_B,_R,_A;

			public double _1 { get { return _Y; }}
			public double _2 { get { return _B; }}
			public double _3 { get { return _R; }}
			public double  α { get { return _A; }}
			public double  Y { get { return _Y; }}
			public double  B { get { return _B; }}
			public double  R { get { return _R; }}
			public double  光 { get { return _Y; }}
		}
	}

	// https://en.wikipedia.org/wiki/YCbCr
	public class ColorSpaceYCbCrBt202 : ColorSpaceYCbCrBase,
		I3ColorSpace<ColorSpaceYCbCrBt202.YBR>, ILumaColorSpace
	{
		public YBR ToSpace(in IColor o)
		{
			var (y,b,r) = BaseToSpace(o);
			return new YBR(y,b,r,o.A);
		}

		public IColor ToNative(in YBR o)
		{
			return BaseToNative(o);
		}

		static ColorSpaceYCbCrBt202()
		{
			Kr = 0.2627; Kb = 0.0593; Kg = 1.0 - Kb - Kr;
			InitMatrixValues();
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((YBR)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct YBR : I3Color, IHasLuma
		{
			public YBR(double y, double b, double r, double α = 1.0) {
				_Y = y; _B = b; _R = r; _A = α;
			}
			public readonly double _Y,_B,_R,_A;

			public double _1 { get { return _Y; }}
			public double _2 { get { return _B; }}
			public double _3 { get { return _R; }}
			public double  α { get { return _A; }}
			public double  Y { get { return _Y; }}
			public double  B { get { return _B; }}
			public double  R { get { return _R; }}
			public double  光 { get { return _Y; }}
		}
	}

	// https://en.wikipedia.org/wiki/YCbCr
	public class ColorSpaceYCbCrSmpte240m : ColorSpaceYCbCrBase,
		I3ColorSpace<ColorSpaceYCbCrSmpte240m.YBR>, ILumaColorSpace
	{
		public YBR ToSpace(in IColor o)
		{
			var (y,b,r) = BaseToSpace(o);
			return new YBR(y,b,r,o.A);
		}

		public IColor ToNative(in YBR o)
		{
			return BaseToNative(o);
		}

		static ColorSpaceYCbCrSmpte240m()
		{
			Kr = 0.212; Kb = 0.087; Kg = 1.0 - Kb - Kr;
			InitMatrixValues();
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((YBR)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct YBR : I3Color, IHasLuma
		{
			public YBR(double y, double b, double r, double α = 1.0) {
				_Y = y; _B = b; _R = r; _A = α;
			}
			public readonly double _Y,_B,_R,_A;

			public double _1 { get { return _Y; }}
			public double _2 { get { return _B; }}
			public double _3 { get { return _R; }}
			public double  α { get { return _A; }}
			public double  Y { get { return _Y; }}
			public double  B { get { return _B; }}
			public double  R { get { return _R; }}
			public double  光 { get { return _Y; }}
		}
	}

	// https://en.wikipedia.org/wiki/YUV
	public class ColorSpaceYCbCrJpeg : I3ColorSpace<ColorSpaceYCbCrJpeg.YBR>, ILumaColorSpace
	{
		public YBR ToSpace(in IColor o)
		{
			double y = o.R *  0.299000 + o.G *  0.587000 + o.B *  0.114000;
			double i = o.R * -0.168736 + o.G * -0.331264 + o.B *  0.500000;
			double q = o.R *  0.500000 + o.G * -0.418688 + o.B * -0.081312;
			return new YBR(y,i,q,o.A);
		}

		public IColor ToNative(in YBR o)
		{
			double r = o.Y * 1.0 + o.B *  0.000000 + o.R *  1.402000;
			double g = o.Y * 1.0 + o.B * -0.344136 + o.R * -0.714136;
			double b = o.Y * 1.0 + o.B *  1.772000 + o.R *  0.000000;
			return new IColor(r,g,b,o.α);
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((YBR)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct YBR : I3Color, IHasLuma
		{
			public YBR(double y, double b, double r, double α = 1.0) {
				_Y = y; _B = b; _R = r; _A = α;
			}
			public readonly double _Y,_B,_R,_A;

			public double _1 { get { return _Y; }}
			public double _2 { get { return _B; }}
			public double _3 { get { return _R; }}
			public double  α { get { return _A; }}
			public double  Y { get { return _Y; }}
			public double  B { get { return _B; }}
			public double  R { get { return _R; }}
			public double  光 { get { return _Y; }}
		}
	}

	// https://en.wikipedia.org/wiki/CIE_1931_color_space
	public class ColorSpaceCie1931 : I3ColorSpace<ColorSpaceCie1931.XYZ>, ILumaColorSpace
	{
		public XYZ ToSpace(in IColor o)
		{
			double x = (o.R * 0.49000 + o.G * 0.3100 + o.B * 0.20000) / 0.17697;
			double y = (o.R * 0.17697 + o.G * 0.8124 + o.B * 0.01063) / 0.17697;
			double z = (o.R * 0.00000 + o.G * 0.0100 + o.B * 0.99000) / 0.17697;
			return new XYZ(x,y,z,o.A);
		}

		public IColor ToNative(in XYZ o)
		{
			//Note: manually calculated from ToSpace matrix
			double r = o.X *  0.4184657124218946000 + o.Y * -0.158660784803799100 + o.Z * -0.08283492761809548;
			double g = o.X * -0.0911689639090227500 + o.Y *  0.252431442139465200 + o.Z *  0.01570752176955761;
			double b = o.X *  0.0009208986253436641 + o.Y * -0.002549812546863284 + o.Z *  0.17859891392151960;
			return new IColor(r,g,b,o.α);
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((XYZ)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		public readonly struct XYZ : I3Color, IHasLuma
		{
			public XYZ(double x, double y, double z, double α = 1.0) {
				_X = x; _Y = y; _Z = z; _A = α;
			}
			public readonly double _X,_Y,_Z,_A;

			public double _1 { get { return _X; }}
			public double _2 { get { return _Y; }}
			public double _3 { get { return _Z; }}
			public double  α { get { return _A; }}
			public double  X { get { return _X; }}
			public double  Y { get { return _Y; }}
			public double  Z { get { return _Z; }}
			public double  光 { get { return _Z; }}
		}
	}

	public class ColorSpaceCie1960 : I3ColorSpace<ColorSpaceCie1960.UVW>, ILumaColorSpace
	{
		// https://en.wikipedia.org/wiki/CIE_1960_color_space
		public UVW ToSpace(in IColor o)
		{
			////Note: this was manually solved from ToNative matrix
			//double x = o.R * 0.24512733766039210 + o.G * -0.08511926135236732 + o.B *  0.11995558030586380;
			//double y = o.R * 0.08851665976487091 + o.G *  0.11112309485155040 + o.B * -0.09801865039031715;
			//double z = o.R * 0.00000000000000000 + o.G *  0.00000000000000000 + o.B *  1.00000000000000000;
			//return new UVW(x,y,z,o.A);

			var p = XyzSpace.ToSpace(o);
			double u = 2.0 * p.X / 3.0;
			double v = p.Y;
			double w = 0.5 * (p.Z + 3.0 * p.Y - p.X);
			return new UVW(u,v,w,o.A);
		}

		public IColor ToNative(in UVW o)
		{
			//double r = o.U *  3.1956 + o.V * 2.4478 + o.W * -0.1434;
			//double g = o.U * -2.5455 + o.V * 7.0492 + o.W *  0.9963;
			//double b = o.U *  0.0000 + o.V * 0.0000 + o.W *  1.0000;
			//return new IFColor(r,g,b,o.A);

			double x = 3.0 * o.U / 2.0;
			double y = o.V;
			double z = 3.0 * o.U / 2.0 - 3.0 * o.V + 2.0 * o.W;

			var xyz = new ColorSpaceCie1931.XYZ(x,y,z,o.α);
			return XyzSpace.ToNative(xyz);
		}

		IColor I3ColorSpace.ToNative(in I3Color o) {
			return ToNative((UVW)o);
		}
		I3Color I3ColorSpace.ToSpace(in IColor o) {
			return ToSpace(o);
		}

		public IHasLuma GetLuma(in IColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		static ColorSpaceCie1931 XyzSpace = new ColorSpaceCie1931();

		public readonly struct UVW : I3Color, IHasLuma
		{
			public UVW(double u, double v, double w, double α = 1.0) {
				_U = u; _V = v; _W = w; _A = α;
			}
			public readonly double _U,_V,_W,_A;

			public double _1 { get { return _U; }}
			public double _2 { get { return _V; }}
			public double _3 { get { return _W; }}
			public double  α { get { return _A; }}
			public double  U { get { return _U; }}
			public double  V { get { return _V; }}
			public double  W { get { return _W; }}
			public double  光 { get { return _W; }}
		}
	}

	#if false
	// https://en.wikipedia.org/wiki/CIELUV
	public class ColorSpaceCieLuv : I3ColorSpace<ColorSpaceCieLuv.LUV>, ILumaColorSpace<ColorSpaceCieLuv.LUV>
	{
		// https://en.wikipedia.org/wiki/YUV
		public LUV ToSpace(IFColor o)
		{
			var p = XYZSpace.ToSpace(o);
			double d = p.X + 15 * p.Y + 3 * p.Z;
			double u = 4 * p.X / d;
			double v = 9 * p.Y / d;



			return new LUV(x * s,y * s,z * s,o.A);
		}

		public IFColor ToNative(LUV o)
		{
			return new IFColor(r,g,b,o.A);
		}

		public LUV GetLuma(IFColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		static ColorSpaceCie1931 XYZSpace = new ColorSpaceCie1931();

		public readonly struct LUV : I3Color, IHasLuma
		{
			public LUV(double l, double u, double v, double a = 1.0) {
				_L = l; _U = u; _V = v; _A = a;
			}
			public readonly double _L,_U,_V,_A;

			public double _1 { get { return _L; }}
			public double _2 { get { return _U; }}
			public double _3 { get { return _V; }}
			public double  A { get { return _A; }}
			public double  L { get { return _L; }}
			public double  U { get { return _U; }}
			public double  V { get { return _V; }}
			public double  光 { get { return _L; }}
		}
	}
	#endif

	#if false
	public class ColorSpaceCieLab : I3ColorSpace<ColorSpaceCieLab.LAB>, ILumaColorSpace<ColorSpaceCieLab.LAB>
	{
		// https://en.wikipedia.org/wiki/CIE_1960_color_space
		public LAB ToSpace(IFColor o)
		{
			var p = XyzSpace.ToSpace(o);


			return new LAB(u,v,w,o.A);
		}

		public IFColor ToNative(LAB o)
		{

			var xyz = new ColorSpaceCie1931.XYZ(x,y,z,o.α);
			return XyzSpace.ToNative(xyz);
		}

		//D65
		const double Xn = 95.0489, Yn = 100.0, Zn = 108.884;

		public LAB GetLuma(IFColor o)
		{
			var c = ToSpace(o);
			return c;
		}

		static ColorSpaceCie1931 XyzSpace = new ColorSpaceCie1931();

		public readonly struct LAB : I3Color, IHasLuma
		{
			public LAB(double l, double a, double b, double α = 1.0) {
				_L = l; _a = a; _b = b; _A = α;
			}
			public readonly double _L,_a,_b,_A;

			public double _1 { get { return _L; }}
			public double _2 { get { return _a; }}
			public double _3 { get { return _b; }}
			public double  α { get { return _A; }}
			public double  L { get { return _L; }}
			public double  a { get { return _a; }}
			public double  b { get { return _b; }}
			public double  光 { get { return _L; }}
		}
	}
	#endif

}