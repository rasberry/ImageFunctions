namespace ImageFunctions.Core;

public interface ICanvas
{
	int Width { get; }
	int Height { get; }

	ColorRGBA this[int x, int y] { get; set; }
}