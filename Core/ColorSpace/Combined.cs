using System;
using System.Numerics;

namespace ImageFunctions.Core.ColorSpace;

//TODO look at https://easyrgb.com/en/convert.php
// and http://www.brucelindbloom.com/
// and http://members.chello.at/~easyfilter/colorspace.c
// https://imagej.net/plugins/color-space-converter.html


// https://www.rapidtables.com/convert/color/
public class ColorSpaceCmyk : IColor4Space<ColorSpaceCmyk.CMYK>
{
	public ColorRGBA ToNative(in CMYK o)
	{
		double m = 1.0 - o.K;
		double r = (1.0 - o.C) * m;
		double g = (1.0 - o.M) * m;
		double b = (1.0 - o.Y) * m;

		return new ColorRGBA(r,g,b,o.A);
	}

	public CMYK ToSpace(in ColorRGBA o)
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

	ColorRGBA IColor4Space.ToNative(in IColor4 o) {
		return ToNative((CMYK)o);
	}
	IColor4 IColor4Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public readonly struct CMYK : IColor4
	{
		public CMYK(double c, double m, double y, double k, double a = 1.0) {
			C = c; M = m; Y = y; K = k; A = a;
		}
		public readonly double C,M,Y,K,A;

		double IColor3.C1 { get { return C; }}
		double IColor3.C2 { get { return M; }}
		double IColor3.C3 { get { return Y; }}
		double IColor4.C4 { get { return K; }}
		double IColor3.A  { get { return A; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"C" => C, "M" => M, "Y" => Y, "K" => K, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "C", "M", "Y", "K", "A" };
		}}
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
public class ColorSpaceHsl : ColorSpaceHSBase, IColor3Space<ColorSpaceHsl.HSL>, ILumaColorSpace
{
	public ColorRGBA ToNative(in HSL o)
	{
		double c = (1.0 - Math.Abs(2 * o.L - 1.0)) * o.S;
		double m = o.L - c / 2.0;
		var (r,g,b) = HueToRgb(o.H,c,m);
		return new ColorRGBA(r,g,b,o.A);
	}

	public HSL ToSpace(in ColorRGBA o)
	{
		double max = Math.Max(Math.Max(o.R,o.G),o.B);
		double min = Math.Min(Math.Min(o.R,o.G),o.B);
		double c = max - min;

		double l = (max + min) / 2.0;
		double h = RgbToHue(o.R,o.G,o.B,c,max);
		double s = l >= 1.0 || l < double.Epsilon ? 0.0 : c / (1.0 - Math.Abs(2 * l - 1.0));

		return new HSL(h,s,l,o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((HSL)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct HSL : IColor3, ILuma
	{
		public HSL(double h, double s, double l, double a = 1.0) {
			H = h; S = s; L = l; A = a;
		}
		public readonly double H,S,L,A;

		double IColor3.C1 { get { return H; }}
		double IColor3.C2 { get { return S; }}
		double IColor3.C3 { get { return L; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return L; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"H" => H, "S" => S, "L" => L, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}
		public IEnumerable<string> ComponentNames { get {
			return new[] { "H", "S", "L", "A" };
		}}
	}
}

// https://en.wikipedia.org/wiki/HSL_and_HSV
public class ColorSpaceHsv : ColorSpaceHSBase, IColor3Space<ColorSpaceHsv.HSV>, ILumaColorSpace
{
	public HSV ToSpace(in ColorRGBA o)
	{
		double max = Math.Max(Math.Max(o.R,o.G),o.B);
		double min = Math.Min(Math.Min(o.R,o.G),o.B);
		double c = max - min;

		double v = max;
		double h = RgbToHue(o.R,o.G,o.B,c,max);
		double s = max < double.Epsilon ? 0.0 : c / max;

		return new HSV(h,s,v,o.A);
	}

	public ColorRGBA ToNative(in HSV o)
	{
		double c = o.V * o.S;
		double m = o.V - c;
		var (r,g,b) = HueToRgb(o.H,c,m);
		return new ColorRGBA(r,g,b,o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((HSV)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct HSV : IColor3, ILuma
	{
		public HSV(double h, double s, double v, double a = 1.0) {
			H = h; S = s; V = v; A = a;
		}
		public readonly double H,S,V,A;

		double IColor3.C1 { get { return H; }}
		double IColor3.C2 { get { return S; }}
		double IColor3.C3 { get { return V; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return V; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"H" => H, "S" => S, "V" => V, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "H", "S", "V", "A" };
		}}
	}
}

// https://en.wikipedia.org/wiki/HSL_and_HSV
public class ColorSpaceHsi : ColorSpaceHSBase, IColor3Space<ColorSpaceHsi.HSI>, ILumaColorSpace
{
	public HSI ToSpace(in ColorRGBA o)
	{
		double max = Math.Max(Math.Max(o.R,o.G),o.B);
		double min = Math.Min(Math.Min(o.R,o.G),o.B);
		double c = max - min;

		double i = (o.R + o.G + o.B) / 3.0;
		double h = RgbToHue(o.R,o.G,o.B,c,max);
		double s = i < double.Epsilon ? 0.0 : 1.0 - min / i;
		return new HSI(h,s,i,o.A);
	}

	public ColorRGBA ToNative(in HSI o)
	{
		double z = 1.0 - Math.Abs((o.H * 6.0) % 2.0 - 1.0);
		double c = 3.0 * o.I * o.S / (z + 1);
		double m = o.I * (1.0 - o.S);
		var (r,g,b) = HueToRgb(o.H,c,m,z);
		return new ColorRGBA(r,g,b,o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((HSI)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct HSI : IColor3, ILuma
	{
		public HSI(double h, double s, double i, double a = 1.0) {
			H = h; S = s; I = i; A = a;
		}
		public readonly double H,S,I,A;

		double IColor3.C1 { get { return H; }}
		double IColor3.C2 { get { return S; }}
		double IColor3.C3 { get { return I; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return I; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"H" => H, "S" => S, "I" => I, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "H", "S", "I", "A" };
		}}
	}
}

// https://en.wikipedia.org/wiki/YIQ
public class ColorSpaceYiq : IColor3Space<ColorSpaceYiq.YIQ>, ILumaColorSpace
{
	public YIQ ToSpace(in ColorRGBA o)
	{
		double y = o.R *  0.2990 + o.G *  0.5870 + o.B *  0.1140;
		double i = o.R *  0.5959 + o.G * -0.2746 + o.B * -0.3213;
		double q = o.R *  0.2115 + o.G * -0.5227 + o.B *  0.3112;
		return new YIQ(y,i,q,o.A);
	}

	public ColorRGBA ToNative(in YIQ o)
	{
		double r = o.Y *  1.0 + o.Q *  0.9690 + o.Q *  0.6190;
		double g = o.Y *  1.0 + o.Q * -0.2720 + o.Q * -0.6470;
		double b = o.Y *  1.0 + o.Q * -1.1060 + o.Q *  1.7030;
		return new ColorRGBA(r,g,b,o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((YIQ)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct YIQ : IColor3, ILuma
	{
		public YIQ(double y, double i, double q, double a = 1.0) {
			Y = y; I = i; Q = q; A = a;
		}
		public readonly double Y,I,Q,A;

		double IColor3.C1 { get { return Y; }}
		double IColor3.C2 { get { return I; }}
		double IColor3.C3 { get { return Q; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Y; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"Y" => Y, "I" => I, "Q" => Q, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "Y", "I", "Q", "A" };
		}}
	}
}

// https://en.wikipedia.org/wiki/YIQ
public class ColorSpaceYiqFcc : IColor3Space<ColorSpaceYiqFcc.YIQ>, ILumaColorSpace
{
	public YIQ ToSpace(in ColorRGBA o)
	{
		double y = o.R *  0.3000 + o.G *  0.5900 + o.B *  0.1100;
		double i = o.R *  0.5990 + o.G * -0.2773 + o.B * -0.3217;
		double q = o.R *  0.2130 + o.G * -0.5251 + o.B *  0.3121;
		return new YIQ(y,i,q,o.A);
	}

	public ColorRGBA ToNative(in YIQ o)
	{
		double r = o.Y *  1.0 + o.I *  0.9469 + o.Q *  0.6236;
		double g = o.Y *  1.0 + o.I * -0.2748 + o.Q * -0.6357;
		double b = o.Y *  1.0 + o.I * -1.1000 + o.Q *  1.7000;
		return new ColorRGBA(r,g,b,o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((YIQ)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct YIQ : IColor3, ILuma
	{
		public YIQ(double y, double i, double q, double a = 1.0) {
			Y = y; I = i; Q = q; A = a;
		}
		public readonly double Y,I,Q,A;

		double IColor3.C1 { get { return Y; }}
		double IColor3.C2 { get { return I; }}
		double IColor3.C3 { get { return Q; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Y; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"Y" => Y, "I" => I, "Q" => Q, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "Y", "I", "Q", "A" };
		}}
	}
}

// https://en.wikipedia.org/wiki/YUV
public class ColorSpaceYuvBT601 : IColor3Space<ColorSpaceYuvBT601.YUV>, ILumaColorSpace
{
	public YUV ToSpace(in ColorRGBA o)
	{
		double y = o.R *  0.29900 + o.G *  0.58700 + o.B *  0.11400;
		double i = o.R * -0.14713 + o.G * -0.28886 + o.B *  0.43600;
		double q = o.R *  0.61500 + o.G * -0.51499 + o.B * -0.10001;
		return new YUV(y,i,q,o.A);
	}

	public ColorRGBA ToNative(in YUV o)
	{
		double r = o.Y *  1.0 + o.U *  0.00000 + o.V *  1.13983;
		double g = o.Y *  1.0 + o.U * -0.39465 + o.V * -0.58060;
		double b = o.Y *  1.0 + o.U *  2.03211 + o.V *  0.00000;
		return new ColorRGBA(r,g,b,o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((YUV)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct YUV : IColor3, ILuma
	{
		public YUV(double y, double u, double v, double a = 1.0) {
			Y = y; U = u; V = v; A = a;
		}
		public readonly double Y,U,V,A;

		double IColor3.C1 { get { return Y; }}
		double IColor3.C2 { get { return U; }}
		double IColor3.C3 { get { return V; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Y; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"Y" => Y, "U" => U, "V" => V, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "Y", "U", "V", "A" };
		}}
	}
}

// https://en.wikipedia.org/wiki/YUV
public class ColorSpaceYuvBT709 : IColor3Space<ColorSpaceYuvBT709.YUV>, ILumaColorSpace
{
	public YUV ToSpace(in ColorRGBA o)
	{
		double y = o.R *  0.21260 + o.G *  0.71520 + o.B *  0.07220;
		double i = o.R * -0.09991 + o.G * -0.33609 + o.B *  0.43600;
		double q = o.R *  0.61500 + o.G * -0.55861 + o.B * -0.05639;
		return new YUV(y,i,q,o.A);
	}

	public ColorRGBA ToNative(in YUV o)
	{
		double r = o.Y *  1.0 + o.U *  0.00000 + o.V *  1.28033;
		double g = o.Y *  1.0 + o.U * -0.21482 + o.V * -0.38059;
		double b = o.Y *  1.0 + o.U *  2.12789 + o.V *  0.00000;
		return new ColorRGBA(r,g,b,o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((YUV)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct YUV : IColor3, ILuma
	{
		public YUV(double y, double u, double v, double a = 1.0) {
			Y = y; U = u; V = v; A = a;
		}
		public readonly double Y,U,V,A;

		double IColor3.C1 { get { return Y; }}
		double IColor3.C2 { get { return U; }}
		double IColor3.C3 { get { return V; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Y; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"Y" => Y, "U" => U, "V" => V, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "Y", "U", "V", "A" };
		}}
	}
}

// https://en.wikipedia.org/wiki/YDbDr
public class ColorSpaceYDbDr : IColor3Space<ColorSpaceYDbDr.YBR>, ILumaColorSpace
{
	public YBR ToSpace(in ColorRGBA o)
	{
		double y = o.R *  0.299 + o.G *  0.587 + o.B *  0.114;
		double b = o.R * -0.450 + o.G * -0.883 + o.B *  1.333;
		double r = o.R * -1.333 + o.G *  1.116 + o.B *  0.217;
		return new YBR(y,b,r,o.A);
	}

	public ColorRGBA ToNative(in YBR o)
	{
		double r = o.Y *  1.0 + o.B *  0.000092303716148 + o.R * -0.525912630661865;
		double g = o.Y *  1.0 + o.B * -0.129132898890509 + o.R *  0.267899328207599;
		double b = o.Y *  1.0 + o.B *  0.664679059978955 + o.R * -0.000079808543533;
		return new ColorRGBA(r,g,b,o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((YBR)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct YBR : IColor3, ILuma
	{
		public YBR(double y, double b, double r, double a = 1.0) {
			Y = y; B = b; R = r; A = a;
		}
		public readonly double Y,B,R,A;

		double IColor3.C1 { get { return Y; }}
		double IColor3.C2 { get { return B; }}
		double IColor3.C3 { get { return R; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Y; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"Y" => Y, "B" => B, "R" => R, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "Y", "B", "R", "A" };
		}}
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

	protected static (double,double,double) BaseToSpace(in ColorRGBA o)
	{
		double y = o.R * F11 + o.G * F21 + o.B * F31;
		double b = o.R * F12 + o.G * F22 + o.B * F32;
		double r = o.R * F13 + o.G * F23 + o.B * F33;
		return (y,b,r);
	}

	public ColorRGBA BaseToNative(in IColor3 o)
	{
		double r = o.C1 * I11 + o.C2 * I21 + o.C3 * I31;
		double g = o.C1 * I12 + o.C2 * I22 + o.C3 * I32;
		double b = o.C1 * I13 + o.C2 * I23 + o.C3 * I33;
		return new ColorRGBA(r,g,b,o.A);
	}
}

// https://en.wikipedia.org/wiki/YCbCr
public class ColorSpaceYCbCrBt601 : ColorSpaceYCbCrBase,
	IColor3Space<ColorSpaceYCbCrBt601.YBR>, ILumaColorSpace
{
	public YBR ToSpace(in ColorRGBA o)
	{
		var (y,b,r) = BaseToSpace(o);
		return new YBR(y,b,r,o.A);
	}

	public ColorRGBA ToNative(in YBR o)
	{
		return BaseToNative(o);
	}

	static ColorSpaceYCbCrBt601()
	{
		Kr = 0.299; Kg = 0.587; Kb = 0.114;
		InitMatrixValues();
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((YBR)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct YBR : IColor3, ILuma
	{
		public YBR(double y, double b, double r, double a = 1.0) {
			Y = y; B = b; R = r; A = a;
		}
		public readonly double Y,B,R,A;

		double IColor3.C1 { get { return Y; }}
		double IColor3.C2 { get { return B; }}
		double IColor3.C3 { get { return R; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Y; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"Y" => Y, "B" => B, "R" => R, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "Y", "B", "R", "A" };
		}}
	}
}

// https://en.wikipedia.org/wiki/YCbCr
public class ColorSpaceYCbCrBt709 : ColorSpaceYCbCrBase,
	IColor3Space<ColorSpaceYCbCrBt709.YBR>, ILumaColorSpace
{
	public YBR ToSpace(in ColorRGBA o)
	{
		var (y,b,r) = BaseToSpace(o);
		return new YBR(y,b,r,o.A);
	}

	public ColorRGBA ToNative(in YBR o)
	{
		return BaseToNative(o);
	}

	static ColorSpaceYCbCrBt709()
	{
		Kr = 0.2126; Kb = 0.0722; Kg = 1.0 - Kb - Kr;
		InitMatrixValues();
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((YBR)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct YBR : IColor3, ILuma
	{
		public YBR(double y, double b, double r, double a = 1.0) {
			Y = y; B = b; R = r; A = a;
		}
		public readonly double Y,B,R,A;

		double IColor3.C1 { get { return Y; }}
		double IColor3.C2 { get { return B; }}
		double IColor3.C3 { get { return R; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Y; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"Y" => Y, "B" => B, "R" => R, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "Y", "B", "R", "A" };
		}}
	}
}

// https://en.wikipedia.org/wiki/YCbCr
public class ColorSpaceYCbCrBt202 : ColorSpaceYCbCrBase,
	IColor3Space<ColorSpaceYCbCrBt202.YBR>, ILumaColorSpace
{
	public YBR ToSpace(in ColorRGBA o)
	{
		var (y,b,r) = BaseToSpace(o);
		return new YBR(y,b,r,o.A);
	}

	public ColorRGBA ToNative(in YBR o)
	{
		return BaseToNative(o);
	}

	static ColorSpaceYCbCrBt202()
	{
		Kr = 0.2627; Kb = 0.0593; Kg = 1.0 - Kb - Kr;
		InitMatrixValues();
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((YBR)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct YBR : IColor3, ILuma
	{
		public YBR(double y, double b, double r, double a = 1.0) {
			Y = y; B = b; R = r; A = a;
		}
		public readonly double Y,B,R,A;

		double IColor3.C1 { get { return Y; }}
		double IColor3.C2 { get { return B; }}
		double IColor3.C3 { get { return R; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Y; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"Y" => Y, "B" => B, "R" => R, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "Y", "B", "R", "A" };
		}}
	}
}

// https://en.wikipedia.org/wiki/YCbCr
public class ColorSpaceYCbCrSmpte240m : ColorSpaceYCbCrBase,
	IColor3Space<ColorSpaceYCbCrSmpte240m.YBR>, ILumaColorSpace
{
	public YBR ToSpace(in ColorRGBA o)
	{
		var (y,b,r) = BaseToSpace(o);
		return new YBR(y,b,r,o.A);
	}

	public ColorRGBA ToNative(in YBR o)
	{
		return BaseToNative(o);
	}

	static ColorSpaceYCbCrSmpte240m()
	{
		Kr = 0.212; Kb = 0.087; Kg = 1.0 - Kb - Kr;
		InitMatrixValues();
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((YBR)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct YBR : IColor3, ILuma
	{
		public YBR(double y, double b, double r, double a = 1.0) {
			Y = y; B = b; R = r; A = a;
		}
		public readonly double Y,B,R,A;

		double IColor3.C1 { get { return Y; }}
		double IColor3.C2 { get { return B; }}
		double IColor3.C3 { get { return R; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Y; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"Y" => Y, "B" => B, "R" => R, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "Y", "B", "R", "A" };
		}}
	}
}

// https://en.wikipedia.org/wiki/YUV
public class ColorSpaceYCbCrJpeg : IColor3Space<ColorSpaceYCbCrJpeg.YBR>, ILumaColorSpace
{
	public YBR ToSpace(in ColorRGBA o)
	{
		double y = o.R *  0.299000 + o.G *  0.587000 + o.B *  0.114000;
		double i = o.R * -0.168736 + o.G * -0.331264 + o.B *  0.500000;
		double q = o.R *  0.500000 + o.G * -0.418688 + o.B * -0.081312;
		return new YBR(y,i,q,o.A);
	}

	public ColorRGBA ToNative(in YBR o)
	{
		double r = o.Y * 1.0 + o.B *  0.000000 + o.R *  1.402000;
		double g = o.Y * 1.0 + o.B * -0.344136 + o.R * -0.714136;
		double b = o.Y * 1.0 + o.B *  1.772000 + o.R *  0.000000;
		return new ColorRGBA(r,g,b,o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((YBR)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct YBR : IColor3, ILuma
	{
		public YBR(double y, double b, double r, double a = 1.0) {
			Y = y; B = b; R = r; A = a;
		}
		public readonly double Y,B,R,A;

		double IColor3.C1 { get { return Y; }}
		double IColor3.C2 { get { return B; }}
		double IColor3.C3 { get { return R; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Y; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"Y" => Y, "B" => B, "R" => R, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "Y", "B", "R", "A" };
		}}
	}
}

// https://en.wikipedia.org/wiki/CIE_1931_color_space
public class ColorSpaceCie1931 : IColor3Space<ColorSpaceCie1931.XYZ>, ILumaColorSpace
{
	public XYZ ToSpace(in ColorRGBA o)
	{
		double x = (o.R * 0.49000 + o.G * 0.3100 + o.B * 0.20000) / 0.17697;
		double y = (o.R * 0.17697 + o.G * 0.8124 + o.B * 0.01063) / 0.17697;
		double z = (o.R * 0.00000 + o.G * 0.0100 + o.B * 0.99000) / 0.17697;
		return new XYZ(x,y,z,o.A);
	}

	public ColorRGBA ToNative(in XYZ o)
	{
		//Note: manually calculated from ToSpace matrix
		double r = o.X *  0.4184657124218946000 + o.X * -0.158660784803799100 + o.Z * -0.08283492761809548;
		double g = o.X * -0.0911689639090227500 + o.X *  0.252431442139465200 + o.Z *  0.01570752176955761;
		double b = o.X *  0.0009208986253436641 + o.X * -0.002549812546863284 + o.Z *  0.17859891392151960;
		return new ColorRGBA(r,g,b,o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((XYZ)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public readonly struct XYZ : IColor3, ILuma
	{
		public XYZ(double x, double y, double z, double a = 1.0) {
			X = x; Y = y; Z = z; A = a;
		}
		public readonly double X,Y,Z,A;

		double IColor3.C1 { get { return X; }}
		double IColor3.C2 { get { return Y; }}
		double IColor3.C3 { get { return Z; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Z; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"X" => X, "Y" => Y, "Z" => Z, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "X", "Y", "Z", "A" };
		}}
	}
}

public class ColorSpaceCie1960 : IColor3Space<ColorSpaceCie1960.UVW>, ILumaColorSpace
{
	// https://en.wikipedia.org/wiki/CIE_1960_color_space
	public UVW ToSpace(in ColorRGBA o)
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

	public ColorRGBA ToNative(in UVW o)
	{
		//double r = o.U *  3.1956 + o.V * 2.4478 + o.W * -0.1434;
		//double g = o.U * -2.5455 + o.V * 7.0492 + o.W *  0.9963;
		//double b = o.U *  0.0000 + o.V * 0.0000 + o.W *  1.0000;
		//return new IFColor(r,g,b,o.A);

		double x = 3.0 * o.U / 2.0;
		double y = o.V;
		double z = 3.0 * o.U / 2.0 - 3.0 * o.V + 2.0 * o.W;

		var xyz = new ColorSpaceCie1931.XYZ(x,y,z,o.A);
		return XyzSpace.ToNative(xyz);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((UVW)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	static ColorSpaceCie1931 XyzSpace = new ColorSpaceCie1931();

	public readonly struct UVW : IColor3, ILuma
	{
		public UVW(double u, double v, double w, double a = 1.0) {
			U = u; V = v; W = w; A = a;
		}
		public readonly double U,V,W,A;

		double IColor3.C1 { get { return U; }}
		double IColor3.C2 { get { return V; }}
		double IColor3.C3 { get { return W; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return W; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"U" => U, "V" => V, "W" => W, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "U", "V", "W", "A" };
		}}
	}
}
