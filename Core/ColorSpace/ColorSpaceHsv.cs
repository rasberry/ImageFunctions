namespace ImageFunctions.Core.ColorSpace;

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

	public ColorSpaceInfo Info { get {
		return new ColorSpaceInfo {
			Description = "Hue, Saturation, Value",
			ComponentNames = new[] { "H", "S", "V", "A" }
		};
	}}

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
	}
}