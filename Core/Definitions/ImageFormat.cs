namespace ImageFunctions.Core;

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

	public readonly bool CanRead;
	public readonly bool CanWrite;
	public readonly bool MultiFrame;
	public readonly string Name;
	public readonly string Description;
}