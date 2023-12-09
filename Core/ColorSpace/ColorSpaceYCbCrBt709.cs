namespace ImageFunctions.Core.ColorSpace;

// https://en.wikipedia.org/wiki/YCbCr
public class ColorSpaceYCbCrBt709 : ColorSpaceYCbCrBase,
	IColor3Space<ColorSpaceYCbCrBt709.YBR>, ILumaColorSpace
{
	public YBR ToSpace(in ColorRGBA o)
	{
		var (y,b,r) = BaseToSpace(o);
		return new YBR(y,b,r,o.A);
	}

	public ColorRGBA ToNative(in YBR o)
	{
		return BaseToNative(o);
	}

	static ColorSpaceYCbCrBt709()
	{
		Kr = 0.2126; Kb = 0.0722; Kg = 1.0 - Kb - Kr;
		InitMatrixValues();
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
			Description = "Luma, Blue-Red difference chroma : BT. 709",
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