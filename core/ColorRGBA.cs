using ImageFunctions.Core.ColorSpace;

namespace ImageFunctions.Core;

/// <summary>
/// Color with Components R,G,B,A
/// </summary>
public readonly record struct ColorRGBA : IColor3
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
}
