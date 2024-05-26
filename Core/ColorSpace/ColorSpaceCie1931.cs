namespace ImageFunctions.Core.ColorSpace;

// https://en.wikipedia.org/wiki/CIE_1931_color_space
public class ColorSpaceCie1931 : IColor3Space<ColorSpaceCie1931.XYZ>, ILumaColorSpace
{
	public XYZ ToSpace(in ColorRGBA o)
	{
		double x = (o.R * 0.49000 + o.G * 0.3100 + o.B * 0.20000) / 0.17697;
		double y = (o.R * 0.17697 + o.G * 0.8124 + o.B * 0.01063) / 0.17697;
		double z = (o.R * 0.00000 + o.G * 0.0100 + o.B * 0.99000) / 0.17697;
		return new XYZ(x, y, z, o.A);
	}

	public ColorRGBA ToNative(in XYZ o)
	{
		//Note: manually calculated from ToSpace matrix
		double r = o.X * 0.4184657124218946000 + o.Y * -0.158660784803799100 + o.Z * -0.08283492761809548;
		double g = o.X * -0.0911689639090227500 + o.Y * 0.252431442139465200 + o.Z * 0.01570752176955761;
		double b = o.X * 0.0009208986253436641 + o.Y * -0.002549812546863284 + o.Z * 0.17859891392151960;
		return new ColorRGBA(r, g, b, o.A);
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o)
	{
		return o is XYZ n ? ToNative(n) : ToNative(new XYZ(o.C1, o.C2, o.C3, o.A));
	}
	IColor3 IColor3Space.ToSpace(in ColorRGBA o)
	{
		return ToSpace(o);
	}

	public ILuma GetLuma(in ColorRGBA o)
	{
		var c = ToSpace(o);
		return c;
	}

	public ColorSpaceInfo Info {
		get {
			return new ColorSpaceInfo {
				Description = "International Commission on Illumination 1931",
				ComponentNames = new[] { "X", "Y", "Z", "A" }
			};
		}
	}

	public readonly record struct XYZ : IColor3, ILuma
	{
		public XYZ(double x, double y, double z, double a = 1.0)
		{
			X = x; Y = y; Z = z; A = a;
		}

		public double X { get; }
		public double Y { get; }
		public double Z { get; }
		public double A { get; }

		double IColor3.C1 { get { return X; } }
		double IColor3.C2 { get { return Y; } }
		double IColor3.C3 { get { return Z; } }
		double IColor3.A { get { return A; } }
		public double Luma { get { return Z; } }

		public ComponentOrdinal GetOrdinal(string name)
		{
			return name?.ToUpperInvariant() switch {
				"X" => ComponentOrdinal.C1,
				"Y" => ComponentOrdinal.C2,
				"Z" => ComponentOrdinal.C3,
				"A" => ComponentOrdinal.A,
				_ => throw Squeal.InvalidArgument(nameof(name))
			};
		}
	}
}
