namespace ImageFunctions.Core.ColorSpace;

// https://www.geeksforgeeks.org/difference-between-rgb-cmyk-hsv-and-yiq-color-models/
public class ColorSpaceCmy : IColor3Space<ColorSpaceCmy.CMY>, ILumaColorSpace
{
	public ILuma GetLuma(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ColorRGBA ToNative(in CMY o) {
		return new ColorRGBA(1.0 - o.C,1.0 - o.M,1.0 - o.Y,o.A);
	}

	public ColorRGBA ToNative(in IColor3 o) {
		return ToNative(o);
	}

	public CMY ToSpace(in ColorRGBA o) {
		return new CMY(1.0 - o.R, 1.0 - o.G, 1.0 - o.B, o.A);
	}

	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public readonly struct CMY : IColor3, ILuma
	{
		public CMY(double c, double m, double y, double a = 1.0) {
			C = c; M = m; Y = y; A = a;
		}
		public readonly double C,M,Y,A;

		double IColor3.C1 { get { return C; }}
		double IColor3.C2 { get { return M; }}
		double IColor3.C3 { get { return Y; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Y; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"C" => C, "M" => M, "Y" => Y, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}

		public IEnumerable<string> ComponentNames { get {
			return new[] { "C", "M", "Y", "A" };
		}}
	}
}