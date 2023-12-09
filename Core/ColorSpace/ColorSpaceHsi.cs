namespace ImageFunctions.Core.ColorSpace;

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