namespace ImageFunctions.Core.ColorSpace;

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

	public ILuma GetLuma(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ColorSpaceInfo Info { get {
		return new ColorSpaceInfo {
			Description = "Luma, U-axis, V-axis : BT. 601",
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