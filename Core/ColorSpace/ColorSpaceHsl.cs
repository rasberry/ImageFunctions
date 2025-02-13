namespace ImageFunctions.Core.ColorSpace;

// https://en.wikipedia.org/wiki/HSL_and_HSV
public class ColorSpaceHsl : ColorSpaceHSBase, IColor3Space<ColorSpaceHsl.HSL>, ILumaColorSpace
{
	public ColorRGBA ToNative(in HSL o)
	{
		double c = (1.0 - Math.Abs(2 * o.L - 1.0)) * o.S;
		double m = o.L - c / 2.0;
		var (r, g, b) = HueToRgb(o.H, c, m);
		return new ColorRGBA(r, g, b, o.A);
	}

	public HSL ToSpace(in ColorRGBA o)
	{
		double max = Math.Max(Math.Max(o.R, o.G), o.B);
		double min = Math.Min(Math.Min(o.R, o.G), o.B);
		double c = max - min;

		double l = (max + min) / 2.0;
		double h = RgbToHue(o.R, o.G, o.B, c, max);
		double s = l >= 1.0 || l < double.Epsilon ? 0.0 : c / (1.0 - Math.Abs(2 * l - 1.0));

		return new HSL(h, s, l, o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o)
	{
		return o is HSL n ? ToNative(n) : ToNative(new HSL(o.C1, o.C2, o.C3, o.A));
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o)
	{
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		return ToSpace(o);
	}

	public ColorSpaceInfo Info {
		get {
			return new ColorSpaceInfo {
				Description = "Hue, Saturation, Lightness",
				ComponentNames = new[] { "H", "S", "L", "A" }
			};
		}
	}

	public readonly record struct HSL : IColor3, ILuma
	{
		public HSL(double h, double s, double l, double a = 1.0)
		{
			H = h; S = s; L = l; A = a;
		}

		public double H { get; }
		public double S { get; }
		public double L { get; }
		public double A { get; }

		double IColor3.C1 { get { return H; } }
		double IColor3.C2 { get { return S; } }
		double IColor3.C3 { get { return L; } }
		double IColor3.A { get { return A; } }
		public double Luma { get { return L; } }

		public ComponentOrdinal GetOrdinal(string name)
		{
			return name?.ToUpperInvariant() switch {
				"H" => ComponentOrdinal.C1,
				"S" => ComponentOrdinal.C2,
				"L" => ComponentOrdinal.C3,
				"A" => ComponentOrdinal.A,
				_ => throw Squeal.InvalidArgument(nameof(name))
			};
		}
	}
}
