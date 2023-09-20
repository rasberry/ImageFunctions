namespace ImageFunctions.Core;

public interface IImageEngine
{
	void SaveImage(ILayers layers, string path, string format = null);
	void LoadImage(ILayers layers, string path);
	ICanvas NewCanvas(int width, int height);

	IEnumerable<ImageFormat> Formats();
}