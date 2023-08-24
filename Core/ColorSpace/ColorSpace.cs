using ImageFunctions.Core;

namespace ImageFunctions.Plugin.AllColors;

//TODO look at https://easyrgb.com/en/convert.php
// and http://www.brucelindbloom.com/
// and http://members.chello.at/~easyfilter/colorspace.c
// https://imagej.net/plugins/color-space-converter.html

public static class ColorSpace
{

}

/*
// https://en.wikipedia.org/wiki/CIE_1931_color_space
public class ColorSpaceCie1931
{
	public ColorFun ToSpace(in ColorFun o)
	{
		double x = (o.C1 * 0.49000 + o.C2 * 0.3100 + o.C3 * 0.20000) / 0.17697;
		double y = (o.C1 * 0.17697 + o.C2 * 0.8124 + o.C3 * 0.01063) / 0.17697;
		double z = (o.C1 * 0.00000 + o.C2 * 0.0100 + o.C3 * 0.99000) / 0.17697;
		return new ColorFun(x,y,z,o.C4);
	}

	public ColorFun ToRGBA(in ColorFun o)
	{
		//Note: manually calculated from ToSpace matrix
		double r = o.X *  0.4184657124218946000 + o.Y * -0.158660784803799100 + o.Z * -0.08283492761809548;
		double g = o.X * -0.0911689639090227500 + o.Y *  0.252431442139465200 + o.Z *  0.01570752176955761;
		double b = o.X *  0.0009208986253436641 + o.Y * -0.002549812546863284 + o.Z *  0.17859891392151960;
		return new ColorFun(r,g,b,o.α);
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
*/