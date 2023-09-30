using System;
using System.Collections.Concurrent;

namespace ImageFunctions.Core.Samplers;

//https://github.com/ImageMagick/ImageMagick/blob/f775a5cf27a95c42bb6d19b50f4869db265fdaa9/MagickCore/resize.c
abstract class AbstractSampler : ISampler
{
	public AbstractSampler()
	{
		Scale = 1.0;
		EdgeRule = PickEdgeRule.Edge;
		Kernel = CreateKernel();
	}
	public abstract double Radius { get; }
	public double Scale { get; set; }
	public PickEdgeRule EdgeRule { get; set; }
	protected abstract double GetKernelAt(double x);
	double[] Kernel { get; }

	public ColorRGBA GetSample(ICanvas img, int x, int y)
	{
		double o = Radius;
		if (o < double.Epsilon) {
			return GetPixel(img,x,y,PickEdgeRule.Edge); //shortcut for NearestNeighbor
		}

		int rad = (int)Math.Ceiling(o);
		double[] kern = Kernel;

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
				var c = GetPixel(img,u,v,EdgeRule);
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

		return new ColorRGBA(vr,vg,vb,va);
	}

	static ColorRGBA GetPixel(ICanvas image, int x, int y, PickEdgeRule rule)
	{
		switch(rule) {
			default:
			case PickEdgeRule.Edge: {
				x = Math.Clamp(x,0,image.Width - 1);
				y = Math.Clamp(y,0,image.Height - 1);
				return image[x,y];
			}
			case PickEdgeRule.Reflect: {
				if (x < 0) { x = -x; }
				if (x >= image.Width) { x = x - 2 * image.Width - 1; }
				if (y < 0) { y = -y; }
				if (y >= image.Height) { y = y - 2 * image.Height - 1; }
				x %= image.Width;
				y %= image.Height;
				return image[x,y];
			}
			case PickEdgeRule.Tile: {
				x %= image.Width;
				y %= image.Height;
				if (x < 0) { x += image.Width; }
				if (y < 0) { y += image.Height; }
				return image[x,y];
			}
			case PickEdgeRule.Transparent: {
				if (x < 0 || x >= image.Width || y < 0 || y >= image.Width) {
					return Transparent;
				}
				return image[x,y];
			}
		}
	}

	static ColorRGBA Transparent {
		get {
			return new ColorRGBA(0.0, 0.0, 0.0, 0.0);
		}
	}

	double[] CreateKernel()
	{
		double scale = Scale;
		double[] kern;
		double o = Radius;
		int rad = (int)Math.Ceiling(o);

		//create the 1D kernel and normalization factor
		kern = new double[2 * rad + 1];
		double norm = 0.0;

		// fill the kernel
		for(int v = 0; v < kern.Length; v++) {
			double w = GetKernelAt((v - rad) / scale);
			kern[v] = w;
			norm += w;
		}
		//normalize kernel values
		if (norm > 0.0) {
			for(int v = 0; v < kern.Length; v++) {
				kern[v] /= norm;
			}
		}

		return kern;
	}

	// https://en.wikipedia.org/wiki/Bicubic_interpolation#Bicubic_convolution_algorithm
	protected static double Bicubic(double x)
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
	protected static double Box(double x)
	{
		if (x > -0.5 && x <= 0.5) {
			return 1.0;
		}
		return 0.0;
	}

	// http://www.imagemagick.org/Usage/filter/#cubics
	// http://www.cs.utexas.edu/~fussell/courses/cs384g-fall2013/lectures/mitchell/Mitchell.pdf
	protected static double Cubic(double x, double b, double c)
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

	protected static double CatmullRom(double x)        { return Cubic(x, 0.0, 0.5); }
	protected static double Hermite(double x)           { return Cubic(x, 0.0, 0.0); }
	protected static double MitchellNetravali(double x) { return Cubic(x, OneThird, OneThird); }
	protected static double Robidoux(double x)          { return Cubic(x, RB1, RB2); }
	protected static double RobidouxSharp(double x)     { return Cubic(x, RS1, RS2); }
	protected static double Spline(double x)            { return Cubic(x, 1.0, 0.0); }

	protected static double Lanczos(double x, double rad)
	{
		if (x < 0.0) { x = -x; }
		if (x < rad) {
			return Tools.SinC(x) * Tools.SinC(x / rad);
		}
		return 0.0;
	}

	protected static double Lanczos2(double x) { return Lanczos(x,2.0); }
	protected static double Lanczos3(double x) { return Lanczos(x,3.0); }
	protected static double Lanczos5(double x) { return Lanczos(x,5.0); }
	protected static double Lanczos8(double x) { return Lanczos(x,8.0); }

	protected static double Triangle(double x)
	{
		if (x < 0.0) { x = -x; }
		if (x < 1.0) {
			return 1.0 - x;
		}
		return 0.0;
	}

	protected static double Welch(double x)
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

sealed class NearestNeighbor : AbstractSampler
{
	public NearestNeighbor() : base() {}
	public override double Radius { get { return 0.0; }}
	protected override double GetKernelAt(double x) {
		return 1.0;
	}
}

sealed class Bicubic : AbstractSampler
{
	public Bicubic() : base() {}
	public override double Radius { get { return 2.0; }}
	protected override double GetKernelAt(double x) { return Bicubic(x); }
}

sealed class Box : AbstractSampler
{
	public Box() : base() {}
	public override double Radius { get { return 0.5; }}
	protected override double GetKernelAt(double x) { return Box(x); }
}

sealed class CatmullRom : AbstractSampler
{
	public CatmullRom() : base() {}
	public override double Radius { get { return 2.0; }}
	protected override double GetKernelAt(double x) { return CatmullRom(x); }
}

sealed class Hermite : AbstractSampler
{
	public Hermite() : base() {}
	public override double Radius { get { return 2.0; }}
	protected override double GetKernelAt(double x) { return Hermite(x); }
}

sealed class Lanczos2 : AbstractSampler
{
	public Lanczos2() : base() {}
	public override double Radius { get { return 2.0; }}
	protected override double GetKernelAt(double x) { return Lanczos2(x); }
}

sealed class Lanczos3 : AbstractSampler
{
	public Lanczos3() : base() {}
	public override double Radius { get { return 3.0; }}
	protected override double GetKernelAt(double x) { return Lanczos3(x); }
}

sealed class Lanczos5 : AbstractSampler
{
	public Lanczos5() : base() {}
	public override double Radius { get { return 5.0; }}
	protected override double GetKernelAt(double x) { return Lanczos5(x); }
}

sealed class Lanczos8 : AbstractSampler
{
	public Lanczos8() : base() {}
	public override double Radius { get { return 8.0; }}
	protected override double GetKernelAt(double x) { return Lanczos8(x); }
}

sealed class MitchellNetravali : AbstractSampler
{
	public MitchellNetravali() : base() {}
	public override double Radius { get { return 2.0; }}
	protected override double GetKernelAt(double x) { return MitchellNetravali(x); }
}

sealed class Robidoux : AbstractSampler
{
	public Robidoux() : base() {}
	public override double Radius { get { return 2.0; }}
	protected override double GetKernelAt(double x) { return Robidoux(x); }
}

sealed class RobidouxSharp : AbstractSampler
{
	public RobidouxSharp() : base() {}
	public override double Radius { get { return 2.0; }}
	protected override double GetKernelAt(double x) { return RobidouxSharp(x); }
}

sealed class Spline : AbstractSampler
{
	public Spline() : base() {}
	public override double Radius { get { return 2.0; }}
	protected override double GetKernelAt(double x) { return Spline(x); }
}

sealed class Triangle : AbstractSampler
{
	public Triangle() : base() {}
	public override double Radius { get { return 1.0; }}
	protected override double GetKernelAt(double x) { return Triangle(x); }
}

sealed class Welch : AbstractSampler
{
	public Welch() : base() {}
	public override double Radius { get { return 1.0; }}
	protected override double GetKernelAt(double x) { return Welch(x); }
}
