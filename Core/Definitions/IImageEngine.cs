namespace ImageFunctions.Core;

public interface IImageEngine
{
	ICanvas LoadImage(string path);
	ICanvas NewImage(int width, int height);
	void SaveImage(ICanvas img, string path, string format = null);
}