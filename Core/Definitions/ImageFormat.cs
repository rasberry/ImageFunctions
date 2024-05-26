namespace ImageFunctions.Core;

/// <summary>
/// ImageFormat represents the formats supported by an engine
/// </summary>
public readonly record struct ImageFormat
{
	public ImageFormat(
		string name, string desc, bool canRead,
		bool canWrite, bool frames, string extension,
		string mimetype
	)
	{
		CanRead = canRead;
		CanWrite = canWrite;
		Name = name;
		Description = desc;
		MultiFrame = frames;
		BestExtension = extension;
		MimeType = mimetype;
	}

	/// <summary>The engine can read files of this format</summary>
	public bool CanRead { get; }

	/// <summary>The engine can write files of this format</summary>
	public bool CanWrite { get; }

	/// <summary>Whether this format supports multiple layers</summary>
	public bool MultiFrame { get; }

	/// <summary>Name of the format (also usually the file extension)</summary>
	public string Name { get; }

	/// <summary>Description of the format</summary>
	public string Description { get; }

	/// <summary>File format extension (prefixed with a dot)</summary>
	public string BestExtension { get; }

	/// <summary>Mime type of the format</summary>
	public string MimeType { get; }
}
