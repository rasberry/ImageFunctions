namespace ImageFunctions.Core.ColorSpace;

// https://www.rapidtables.com/convert/color/
public class ColorSpaceCmyk : IColor4Space<ColorSpaceCmyk.CMYK>
{
	public ColorRGBA ToNative(in CMYK o)
	{
		double m = 1.0 - o.K;
		double r = (1.0 - o.C) * m;
		double g = (1.0 - o.M) * m;
		double b = (1.0 - o.Y) * m;

		return new ColorRGBA(r,g,b,o.A);
	}

	public CMYK ToSpace(in ColorRGBA o)
	{
		double max = Math.Max(Math.Max(o.R,o.G),o.B);
		double k = 1.0 - max;
		double den = 1.0 - k;
		double c,m,y;
		if (den < double.Epsilon) {
			c = 0.0; m = 0.0; y = 0.0;
		}
		else {
			c = (den - o.R) / den;
			m = (den - o.G) / den;
			y = (den - o.B) / den;
		}
		return new CMYK(c,m,y,k,o.A);
	}

	ColorRGBA IColor4Space.ToNative(in IColor4 o) {
		return ToNative((CMYK)o);
	}
	IColor4 IColor4Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public readonly struct CMYK : IColor4
	{
		public CMYK(double c, double m, double y, double k, double a = 1.0) {
			C = c; M = m; Y = y; K = k; A = a;
		}
		public readonly double C,M,Y,K,A;

		double IColor3.C1 { get { return C; }}
		double IColor3.C2 { get { return M; }}
		double IColor3.C3 { get { return Y; }}
		double IColor4.C4 { get { return K; }}
		double IColor3.A  { get { return A; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"C" => C, "M" => M, "Y" => Y, "K" => K, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "C", "M", "Y", "K", "A" };
		}}
	}
}