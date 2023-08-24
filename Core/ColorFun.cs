using System.Text;

namespace ImageFunctions.Core;

public readonly struct ColorFun : IEquatable<ColorFun>, IColor3
{
	public ColorFun(double c1, double c2, double c3, double a)
	{
		//not clamping here to support HDR. clamp on display instead
		_1 = c1; _2 = c2; _3 = c3; _a = a;
	}

	public readonly double C1 { get { return _1; }}
	public readonly double C2 { get { return _2; }}
	public readonly double C3 { get { return _3; }}
	public readonly double A  { get { return _a; }}

	readonly double _1,_2,_3,_a;

	public override string ToString()
	{
		return $"{nameof(ColorFun)} [{C1},{C2},{C3},{A}]";
	}

	public bool Equals(ColorFun other)
	{
		return other.C1 == C1 && other.C2 == C2 &&
			other.C3 == C3 && other.A == A;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(C1,C2,C3,A);
	}
}

#if false
public readonly ref struct ColorFun
{
	public ColorFun(ReadOnlySpan<double> components)
	{
		Components = components;
	}

	public readonly ReadOnlySpan<double> Components;

	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.Append(nameof(ColorFun));
		sb.Append(' ').Append('[');
		bool isFirst = true;

		foreach(var c in Components) {
			if (!isFirst) {
				sb.Append(',');
			}
			else {
				isFirst = false;
			}
			sb.Append(c.ToString());
		}
		sb.Append(']');
		return sb.ToString();
	}

	public bool Equals(ColorFun other)
	{
		if (other.Components.Length != Components.Length) {
			return false;
		}

		var yourIter = other.Components.GetEnumerator();
		var mineIter = Components.GetEnumerator();

		while(yourIter.MoveNext() && mineIter.MoveNext()) {
			if (yourIter.Current != mineIter.Current) {
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		var hash = new HashCode();
		foreach(var c in Components) {
			hash.Add(c);
		}
		return hash.ToHashCode();
	}

	public static ColorFun FromRGBA255(int r, int g, int b, int a)
	{
		return FromRGBADouble(
			r / 255.0, g / 255.0, g / 255.0, a / 255.0
		);
	}

	/// <summary>
	/// Color with components ranged from 0.0 - 1.0
	/// </summary>
	public static ColorFun FromRGBADouble(double r, double g, double b, double a)
	{
		var list = new ReadOnlySpan<double>(new double[] {
			Math.Clamp(r,0.0,1.0),
			Math.Clamp(g,0.0,1.0),
			Math.Clamp(b,0.0,1.0),
			Math.Clamp(a,0.0,1.0),
		});

		return new ColorFun(list);
	}
}
#endif