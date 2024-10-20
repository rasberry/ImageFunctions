using ImageFunctions.Core;
using System.Drawing;

namespace ImageFunctions.Plugin.Aides;

public static class ImageAide
{
	/// <summary>
	/// Copies the pixels from one canvas to another
	/// </summary>
	/// <param name="dstImg">The canvas that will be modified</param>
	/// <param name="srcImg">The canvas used to retrieve the pixels</param>
	/// <param name="dstRect">Constrains the copy to this rectangle in the destination image</param>
	/// <param name="srcPoint">Sets the point offset where the pixels will be copied from</param>
	public static void CopyFrom(this ICanvas dstImg, ICanvas srcImg,
		Rectangle dstRect = default,
		Point srcPoint = default)
	{
		if(dstRect.IsEmpty) {
			dstRect = dstImg.Bounds();
		}

		for(int y = dstRect.Top; y < dstRect.Bottom; y++) {
			int cy = y - dstRect.Top + srcPoint.Y;
			for(int x = dstRect.Left; x < dstRect.Right; x++) {
				int cx = x - dstRect.Left + srcPoint.X;
				dstImg[x, y] = srcImg[cx, cy];
			}
		}
	}

	/// <summary>
	/// Fills the canvas with a single color
	/// </summary>
	/// <param name="canvas">The canvas to fill</param>
	/// <param name="color">Fill color</param>
	/// <param name="rect">Optional area to fill instead of the entire canvas</param>
	public static void FillWithColor(ICanvas canvas, ColorRGBA color, Rectangle rect = default)
	{
		Rectangle bounds = new Rectangle(0, 0, canvas.Width, canvas.Height);
		if(!rect.IsEmpty) {
			bounds.Intersect(rect);
		}

		for(int y = bounds.Top; y < bounds.Bottom; y++) {
			for(int x = bounds.Left; x < bounds.Right; x++) {
				canvas[x, y] = color;
			}
		}
	}

	/// <summary>
	/// Shortcut for getting the bounds rectangle for a canvas
	/// </summary>
	/// <param name="canvas">The canvas</param>
	/// <returns>A rectangle starting at point 0,0 and width/height matching the canvas</returns>
	public static Rectangle Bounds(this ICanvas canvas)
	{
		return new Rectangle(0, 0, canvas.Width, canvas.Height);
	}
}
