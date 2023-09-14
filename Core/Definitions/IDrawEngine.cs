namespace ImageFunctions.Core;

public interface IDrawEngine
{
	void DrawLine(ICanvas image, ColorRGBA color, PointD p0, PointD p1, double width = 1.0);
}