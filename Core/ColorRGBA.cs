using ImageFunctions.Core.ColorSpace;

namespace ImageFunctions.Core;

/// <summary>
/// Color with Components R,G,B,A
/// </summary>
public readonly record struct ColorRGBA : IColor3, ILuma
{
	public ColorRGBA(double r, double g, double b, double a)
	{
		//not clamping here to support HDR. clamp on display instead
		R = r; G = g; B = b; A = a;
	}

	public readonly double R,G,B,A;

	double IColor3.C1 { get { return R; }}
	double IColor3.C2 { get { return G; }}
	double IColor3.C3 { get { return B; }}
	double IColor3.A  { get { return A; }}

	public static ColorRGBA FromRGBA255(byte r, byte g, byte b, byte a)
	{
		return new ColorRGBA(r / 255.0, g / 255.0, b / 255.0, a / 255.0);
	}

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

	public double Luma { get {
		//BT. 2020
		double Kr = 0.2627, Kb = 0.0593, Kg = 1.0 - Kb - Kr;
		return  Kr * R + Kb * G + Kg * B;

	}}
}
