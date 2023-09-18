namespace ImageFunctions.Core;

public interface IImageEngine
{
	ILayers NewImage(int width, int height);
	ILayers LoadImage(string path);
	void SaveImage(ILayers layers, string path, string format = null);

	IEnumerable<ImageFormat> Formats();
}