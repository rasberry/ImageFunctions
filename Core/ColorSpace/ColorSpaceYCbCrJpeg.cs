namespace ImageFunctions.Core.ColorSpace;

// https://en.wikipedia.org/wiki/YUV
public class ColorSpaceYCbCrJpeg : IColor3Space<ColorSpaceYCbCrJpeg.YBR>, ILumaColorSpace
{
	public YBR ToSpace(in ColorRGBA o)
	{
		double y = o.R *  0.299000 + o.G *  0.587000 + o.B *  0.114000;
		double i = o.R * -0.168736 + o.G * -0.331264 + o.B *  0.500000;
		double q = o.R *  0.500000 + o.G * -0.418688 + o.B * -0.081312;
		return new YBR(y,i,q,o.A);
	}

	public ColorRGBA ToNative(in YBR o)
	{
		double r = o.Y * 1.0 + o.B *  0.000000 + o.R *  1.402000;
		double g = o.Y * 1.0 + o.B * -0.344136 + o.R * -0.714136;
		double b = o.Y * 1.0 + o.B *  1.772000 + o.R *  0.000000;
		return new ColorRGBA(r,g,b,o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o) {
		return ToNative((YBR)o);
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ColorSpaceInfo Info { get {
		return new ColorSpaceInfo {
			Description = "Luma, Blue-Red difference chroma",
			ComponentNames = new[] { "Y", "B", "R", "A" }
		};
	}}

	public readonly struct YBR : IColor3, ILuma
	{
		public YBR(double y, double b, double r, double a = 1.0) {
			Y = y; B = b; R = r; A = a;
		}
		public readonly double Y,B,R,A;

		double IColor3.C1 { get { return Y; }}
		double IColor3.C2 { get { return B; }}
		double IColor3.C3 { get { return R; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return Y; }}

		public double GetComponent(string name)
		{
			return name.ToUpperInvariant() switch {
				"Y" => Y, "B" => B, "R" => R, "A" => A,
				_ => throw Squeal.InvalidArgument(nameof(name)),
			};
		}
	}
}