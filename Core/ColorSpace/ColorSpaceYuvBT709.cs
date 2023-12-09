namespace ImageFunctions.Core.ColorSpace;

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

	public ILuma GetLuma(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ColorSpaceInfo Info { get {
		return new ColorSpaceInfo {
			Description = "Luma, U-axis, V-axis : BT. 709",
			ComponentNames = new[] { "Y", "U", "V", "A" }
		};
	}}

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
	}
}