namespace ImageFunctions.Core;

public interface ICanvas
{
	int Width { get; }
	int Height { get; }

	ColorFun this[int x, int y] { get; set; }
	string Name { get; }
}