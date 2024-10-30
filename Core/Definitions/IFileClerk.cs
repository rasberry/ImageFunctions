namespace ImageFunctions.Core;

public interface IFileClerk : IDisposable
{
	/// <summary>
	/// aquires a stream for reading the file
	/// </summary>
	/// <param name="ext">override the extension</param>
	/// <param name="tag">extra for the filename used when reading multiple files</param>
	/// <returns>A <c cref="System.IO.Stream">Stream</c> of data</returns>
	Stream ReadStream(string ext = null, string tag = null);

	/// <summary>
	/// aquires a stream for writing a file
	/// </summary>
	/// <param name="ext">override the extension</param>
	/// <param name="tag">extra for the filename used when writing multiple files</param>
	/// <returns>A <c cref="System.IO.Stream">Stream</c> of data</returns>
	Stream WriteStream(string ext = null, string tag = null);

	/// <summary>
	/// optional file-system location of file. must be set before any streams are used
	/// </summary>
	string Location { get; set; }

	/// <summary>
	/// optional direct source of data as alternative to using a file.
	/// </summary>
	Stream Source { get; set; }

	/// <summary>
	/// optional progress tracker
	/// </summary>
	IProgress<double> Progress { get; set; }
}