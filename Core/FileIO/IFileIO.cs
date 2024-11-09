namespace ImageFunctions.Core.FileIO;

public interface IFileIO
{
	Stream OpenForReading(string filename);
	Stream OpenForWriting(string filename);
}
