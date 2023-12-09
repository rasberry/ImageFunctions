namespace ImageFunctions.Core.ColorSpace;

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

	public ILuma GetLuma(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ColorSpaceInfo Info { get {
		return new ColorSpaceInfo {
			Description = "Luma, In-phase, Quadrature : FCC 1987",
			ComponentNames = new[] { "Y", "I", "Q", "A" }
		};
	}}

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
	}
}
