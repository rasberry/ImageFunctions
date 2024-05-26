namespace ImageFunctions.Core.ColorSpace;

// https://en.wikipedia.org/wiki/YDbDr
public class ColorSpaceYDbDr : IColor3Space<ColorSpaceYDbDr.YBR>, ILumaColorSpace
{
	public YBR ToSpace(in ColorRGBA o)
	{
		double y = o.R * 0.299 + o.G * 0.587 + o.B * 0.114;
		double b = o.R * -0.450 + o.G * -0.883 + o.B * 1.333;
		double r = o.R * -1.333 + o.G * 1.116 + o.B * 0.217;
		return new YBR(y, b, r, o.A);
	}

	public ColorRGBA ToNative(in YBR o)
	{
		double r = o.Y * 1.0 + o.B * 0.000092303716148 + o.R * -0.525912630661865;
		double g = o.Y * 1.0 + o.B * -0.129132898890509 + o.R * 0.267899328207599;
		double b = o.Y * 1.0 + o.B * 0.664679059978955 + o.R * -0.000079808543533;
		return new ColorRGBA(r, g, b, o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o)
	{
		return o is YBR n ? ToNative(n) : ToNative(new YBR(o.C1, o.C2, o.C3, o.A));
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
				Description = "Luma, Blue-Red difference chroma : SECAM",
				ComponentNames = new[] { "Y", "B", "R", "A" }
			};
		}
	}

	public readonly record struct YBR : IColor3, ILuma
	{
		public YBR(double y, double b, double r, double a = 1.0)
		{
			Y = y; B = b; R = r; A = a;
		}

		public double Y { get; }
		public double B { get; }
		public double R { get; }
		public double A { get; }

		double IColor3.C1 { get { return Y; } }
		double IColor3.C2 { get { return B; } }
		double IColor3.C3 { get { return R; } }
		double IColor3.A { get { return A; } }
		public double Luma { get { return Y; } }

		public ComponentOrdinal GetOrdinal(string name)
		{
			return name?.ToUpperInvariant() switch {
				"Y" => ComponentOrdinal.C1,
				"B" => ComponentOrdinal.C2,
				"R" => ComponentOrdinal.C3,
				"A" => ComponentOrdinal.A,
				_ => throw Squeal.InvalidArgument(nameof(name))
			};
		}
	}
}
