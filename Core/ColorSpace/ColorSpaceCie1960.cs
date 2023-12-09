namespace ImageFunctions.Core.ColorSpace;

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