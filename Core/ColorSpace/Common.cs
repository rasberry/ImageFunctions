using System;
using System.Numerics;

namespace ImageFunctions.Core.ColorSpace;

//TODO look at https://easyrgb.com/en/convert.php
// and http://www.brucelindbloom.com/
// and http://members.chello.at/~easyfilter/colorspace.c
// https://imagej.net/plugins/color-space-converter.html

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

// https://en.wikipedia.org/wiki/YCbCr
public abstract class ColorSpaceYCbCrBase
{
	protected static double Kr,Kg,Kb;
	static double F11,F21,F31,F12,F22,F32,F13,F23,F33;
	static double I11,I21,I31,I12,I22,I32,I13,I23,I33;

	protected static void InitMatrixValues()
	{
		F11 = Kr;
		F21 = Kg;
		F31 = Kb;

		F12 = -0.5 * Kr / (1.0 - Kb);
		F22 = -0.5 * Kg / (1.0 - Kb);
		F32 = 0.5;

		F13 = 0.5;
		F23 = -0.5 * Kg / (1.0 - Kr);
		F33 = -0.5 * Kb / (1.0 - Kr);

		I11 = 1.0;
		I21 = 0.0;
		I31 = 2.0 - 2.0 * Kr;

		I12 = 1.0;
		I22 = 2.0 * Kb / Kg * (Kb - 1.0);
		I32 = 2.0 * Kr / Kg * (Kr - 1.0);
		
		I13 = 1.0;
		I23 = 2.0 - 2.0 * Kb;
		I33 = 0.0;
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
