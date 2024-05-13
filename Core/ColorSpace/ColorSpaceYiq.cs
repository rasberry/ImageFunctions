namespace ImageFunctions.Core.ColorSpace;

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
		double r = o.Y *  1.0 + o.I *  0.9690 + o.Q *  0.6190;
		double g = o.Y *  1.0 + o.I * -0.2720 + o.Q * -0.6470;
		double b = o.Y *  1.0 + o.I * -1.1060 + o.Q *  1.7030;
		return new ColorRGBA(r,g,b,o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return o is YIQ n ? ToNative(n) : ToNative(new YIQ(o.C1,o.C2,o.C3,o.A));
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ColorSpaceInfo Info { get {
		return new ColorSpaceInfo {
			Description = "Luma, In-phase, Quadrature : NTSC 1953",
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

		public ComponentOrdinal GetOrdinal(string name)
		{
			return name.ToUpperInvariant() switch {
				"Y" => ComponentOrdinal.C1,
				"I" => ComponentOrdinal.C2,
				"Q" => ComponentOrdinal.C3,
				"A" => ComponentOrdinal.A,
				_ => throw Squeal.InvalidArgument(nameof(name))
			};
		}
	}
}