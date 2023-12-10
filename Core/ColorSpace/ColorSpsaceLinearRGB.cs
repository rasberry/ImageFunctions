namespace ImageFunctions.Core.ColorSpace;

// https://en.wikipedia.org/wiki/Grayscale
// https://en.wikipedia.org/wiki/SRGB
public class ColorSpsaceLinearRGB : IColor3Space<ColorSpsaceLinearRGB.RGB>, ILumaColorSpace
{
	public ColorRGBA ToNative(in RGB o)
	{
		double r = o.R <= 0.0031308 ? 12.92 * o.R : 1.055 * Math.Pow(o.R, 1/2.4) - 0.055;
		double g = o.R <= 0.0031308 ? 12.92 * o.R : 1.055 * Math.Pow(o.R, 1/2.4) - 0.055;
		double b = o.R <= 0.0031308 ? 12.92 * o.R : 1.055 * Math.Pow(o.R, 1/2.4) - 0.055;
		return new ColorRGBA(r,g,b,o.A);
	}

	public RGB ToSpace(in ColorRGBA o)
	{
		double r = o.R <= 0.04045 ? o.R / 12.92 : Math.Pow((o.R + 0.055) / 1.055, 2.4);
		double g = o.G <= 0.04045 ? o.G / 12.92 : Math.Pow((o.G + 0.055) / 1.055, 2.4);
		double b = o.B <= 0.04045 ? o.B / 12.92 : Math.Pow((o.B + 0.055) / 1.055, 2.4);
		return new RGB(r,g,b,o.A);
	}

	public ColorRGBA ToNative(in IColor3 o) {
		return o is RGB n ? ToNative(n) : ToNative(new RGB(o.C1,o.C2,o.C3,o.A));
	}

	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o) {
		return ToSpace(o);
	}

	public ColorSpaceInfo Info { get {
		return new ColorSpaceInfo {
			Description = "RGB with D65 gamma removed",
			ComponentNames = new[] { "R", "G", "B", "A" }
		};
	}}

	public readonly struct RGB : IColor3, ILuma
	{
		public RGB(double x, double y, double z, double a = 1.0) {
			R = x; G = y; B = z; A = a;
		}
		public readonly double R,G,B,A;

		double IColor3.C1 { get { return R; }}
		double IColor3.C2 { get { return G; }}
		double IColor3.C3 { get { return B; }}
		double IColor3.A  { get { return A; }}
		public double Luma { get { return 0.2126 * R + 0.7152 * G + 0.0722 * B; }}

		public ComponentOrdinal GetOrdinal(string name)
		{
			return name.ToUpperInvariant() switch {
				"R" => ComponentOrdinal.C1,
				"G" => ComponentOrdinal.C2,
				"B" => ComponentOrdinal.C3,
				"A" => ComponentOrdinal.A,
				_ => throw Squeal.InvalidArgument(nameof(name))
			};
		}
	}
}