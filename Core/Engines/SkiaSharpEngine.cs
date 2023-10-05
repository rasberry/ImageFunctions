#if false
namespace ImageFunctions.Core.Engines;

public class SkiaSharpEngine : IImageEngine, IDrawEngine
{
	public IEnumerable<ImageFormat> Formats()
	{
		SkiaSharp.SKEncodedImageFormat
		return null;
	}

	public void LoadImage(ILayers layers, string path)
	{

	}

	public ICanvas NewCanvas(int width, int height)
	{
		return null;
	}

	public void SaveImage(ILayers layers, string path, string format = null)
	{

	}

	public void DrawLine(ICanvas image, ColorRGBA color, PointD p0, PointD p1, double width = 1)
	{

	}
}
#endif