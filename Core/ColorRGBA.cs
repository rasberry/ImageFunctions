using System.Drawing;

namespace ImageFunctions.Core;

/// <summary>
/// Wrapper for ColorFun that exposes the components as R,G,B,A
/// </summary>
public readonly struct ColorRGBA : IEquatable<ColorRGBA>
{
	public ColorRGBA(double r, double g, double b, double a)
	{
		TheColor = new ColorFun(r,g,b,a);
	}

	public ColorRGBA(ColorFun c)
	{
		TheColor = c;
	}

	public double R { get { return TheColor.C1; }}
	public double G { get { return TheColor.C2; }}
	public double B { get { return TheColor.C3; }}
	public double A { get { return TheColor.A; }}

	public override string ToString()
	{
		return TheColor.ToString();
	}

	public bool Equals(ColorRGBA other)
	{
		return other.TheColor.Equals(other.TheColor);
	}

	public override int GetHashCode()
	{
		return TheColor.GetHashCode();
	}

	public static explicit operator ColorFun(ColorRGBA c)
	{
		return new ColorFun(c.R,c.G,c.B,c.A);
	}
	public static explicit operator ColorRGBA(ColorFun c)
	{
		return new ColorRGBA(c);
	}

	readonly ColorFun TheColor;

	public static ColorRGBA FromRGBA255(byte r, byte g, byte b, byte a)
	{
		return new ColorRGBA(r / 255.0, g / 255.0, b / 255.0, a / 255.0);
	}
}