namespace ImageFunctions.Core.ColorSpace;

// https://www.geeksforgeeks.org/difference-between-rgb-cmyk-hsv-and-yiq-color-models/
public class ColorSpaceCmy : IColor3Space<ColorSpaceCmy.CMY>, ILumaColorSpace
{
	public ILuma GetLuma(in ColorRGBA o)
	{
		return ToSpace(o);
	}

	public ColorRGBA ToNative(in CMY o)
	{
		return new ColorRGBA(1.0 - o.C, 1.0 - o.M, 1.0 - o.Y, o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o)
	{
		return o is CMY n ? ToNative(n) : ToNative(new CMY(o.C1, o.C2, o.C3, o.A));
	}

	public CMY ToSpace(in ColorRGBA o)
	{
		return new CMY(1.0 - o.R, 1.0 - o.G, 1.0 - o.B, o.A);
	}

	IColor3 IColor3Space.ToSpace(in ColorRGBA o)
	{
		return ToSpace(o);
	}

	public ColorSpaceInfo Info {
		get {
			return new ColorSpaceInfo {
				Description = "Cyan, Magenta, Yellow",
				ComponentNames = new[] { "C", "M", "Y", "A" }
			};
		}
	}

	public readonly record struct CMY : IColor3, ILuma
	{
		public CMY(double c, double m, double y, double a = 1.0)
		{
			C = c; M = m; Y = y; A = a;
		}

		public double C { get; }
		public double M { get; }
		public double Y { get; }
		public double A { get; }

		double IColor3.C1 { get { return C; } }
		double IColor3.C2 { get { return M; } }
		double IColor3.C3 { get { return Y; } }
		double IColor3.A { get { return A; } }
		public double Luma { get { return Y; } }

		public ComponentOrdinal GetOrdinal(string name)
		{
			return name?.ToUpperInvariant() switch {
				"C" => ComponentOrdinal.C1,
				"M" => ComponentOrdinal.C2,
				"Y" => ComponentOrdinal.C3,
				"A" => ComponentOrdinal.A,
				_ => throw Squeal.InvalidArgument(nameof(name))
			};
		}
	}
}
