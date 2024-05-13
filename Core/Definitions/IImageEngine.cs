namespace ImageFunctions.Core;

/// <summary>
/// Generic image engine interface.
/// </summary>
public interface IImageEngine
{
	/// <summary>
	/// Save the stack of layers as one or more files
	/// </summary>
	/// <param name="layers">ILayers object containing zero or more ICanvas layers</param>
	/// <param name="path">name of file to save</param>
	/// <param name="format">format to use for saving (if null the engine selects a format)</param>
	void SaveImage(ILayers layers, string path, string format = null);

	/// <summary>
	/// Loads a file as one or more layers. Pushes the image(s) on top of the stack.
	/// </summary>
	/// <param name="layers">ILayers object in which to add one or more ICanvas layers</param>
	/// <param name="path">name of the file to load</param>
	/// <param name="name">optional name to assign to the layer(s)</param>
	void LoadImage(ILayers layers, string path, string name = null);

	/// <summary>
	/// Creates a new ICanvas object which is not added to the ILayers stack
	/// </summary>
	/// <param name="width">Width of the canvas (Note: this should match the width of all other layers)</param>
	/// <param name="height">Height of the canvas (Note: this should match the height of all other layers)</param>
	/// <returns>A new ICanvas object</returns>
	ICanvas NewCanvas(int width, int height);

	/// <summary>
	/// Lists the formats the engine supports
	/// </summary>
	/// <returns>An enumeration of the supported image formats</returns>
	IEnumerable<ImageFormat> Formats();

}