using System.Drawing;

namespace ImageFunctions.Core;

/// <summary>
/// ICanvas represents a single layer of pixels
/// </summary>
public interface ICanvas : IDisposable
{
	/// <summary>
	/// Width of the canvas
	/// </summary>
	int Width { get; }

	/// <summary>
	/// Height of the canvas
	/// </summary>
	int Height { get; }

	/// <summary>
	/// Gets or Sets individual pixels
	/// </summary>
	/// <param name="x">The x-coordinate</param>
	/// <param name="y">The y-coordinate</param>
	/// <returns>The pixels color in RGBA format</returns>
	ColorRGBA this[int x, int y] { get; set; }

	//TODO maybe add these for faster access
	//Span<ColorRGBA> Row(int y);
	//Span2D<ColorRGBA> Block(Rectangle rect);
}