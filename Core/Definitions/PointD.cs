namespace ImageFunctions.Core;

public readonly record struct PointD
{
	public PointD(double x, double y)
	{
		X = x; Y = y;
	}
	public double X { get; }
	public double Y { get; }
}
