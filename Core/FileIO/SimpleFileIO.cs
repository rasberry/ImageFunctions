
namespace ImageFunctions.Core.FileIO;

public sealed class SimpleFileIO : IFileIO
{
	public Stream OpenForReading(string filename)
	{
		var fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
		return fs;
	}

	public Stream OpenForWriting(string filename)
	{
		var fs = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.Read);
		return fs;
	}
}
