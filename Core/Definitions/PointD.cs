namespace ImageFunctions.Core;

public readonly record struct PointD
{
	public PointD(double x, double y)
	{
		X = x; Y = y;
	}
	public double X { get; }
	public double Y { get; }

	public static readonly PointD Empty;
	public readonly bool IsEmpty => X == 0.0 && Y == 0.0;

	public override readonly string ToString() => $"{{X={X}, Y={Y}}}";
}
