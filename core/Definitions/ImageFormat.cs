namespace ImageFunctions.Core;

/// <summary>
/// ImageFormat represents the formats supported by an engine
/// </summary>
public readonly struct ImageFormat
{
	public ImageFormat(string name, string desc, bool canread, bool canwrite, bool frames)
	{
		CanRead = canread;
		CanWrite = canwrite;
		Name = name;
		Description = desc;
		MultiFrame = frames;
	}

	/// <summary>
	/// The engine can read files of this format
	/// </summary>
	public readonly bool CanRead;

	/// <summary>
	/// The engine can write files of this format
	/// </summary>
	public readonly bool CanWrite;

	/// <summary>
	/// Whether this format supports multiple layers
	/// </summary>
	public readonly bool MultiFrame;

	/// <summary>
	/// Name of the format (also usually the file extension)
	/// </summary>
	public readonly string Name;

	/// <summary>
	/// Description of the format
	/// </summary>
	public readonly string Description;
}