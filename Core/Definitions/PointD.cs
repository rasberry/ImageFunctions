namespace ImageFunctions.Core;

public readonly struct PointD
{
	public PointD(double x,double y) {
		X = x; Y = y;
	}
	public readonly double X;
	public readonly double Y;
}