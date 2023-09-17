namespace ImageFunctions.Core;

public readonly struct ImageFormat
{
	public ImageFormat(string name, string desc, bool canread, bool canwrite)
	{
		CanRead = canread;
		CanWrite = canwrite;
		Name = name;
		Description = desc;
	}

	public readonly bool CanRead;
	public readonly bool CanWrite;
	public readonly string Name;
	public readonly string Description;
}