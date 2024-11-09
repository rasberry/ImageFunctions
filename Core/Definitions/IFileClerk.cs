namespace ImageFunctions.Core;

public interface IFileClerk1 : IDisposable
{
	/// <summary>
	/// aquires a stream for reading the file
	/// </summary>
	/// <param name="ext">override the extension</param>
	/// <param name="tag">extra tag for the filename used when reading multiple files</param>
	/// <returns>A <c cref="System.IO.Stream">Stream</c> of data</returns>
	Stream ReadStream(string ext = null, string tag = null);

	/// <summary>
	/// aquires a stream for writing a file
	/// </summary>
	/// <param name="ext">override the extension</param>
	/// <param name="tag">extra tag for the filename used when writing multiple files</param>
	/// <returns>A <c cref="System.IO.Stream">Stream</c> of data</returns>
	Stream WriteStream(string ext = null, string tag = null);

	/// <summary>
	/// label used for when adding layers
	/// <param name="tag">extra tag for the filename used when writing multiple files</param>
	/// </summary>
	string GetLabel(string tag = null);

	/// <summary>
	/// optional progress tracker
	/// </summary>
	IProgress<double> Progress { get; set; }
}

public interface IFileClerk : IDisposable
{
	Stream ReadStream(string ext = null, string tag = null);
	Stream WriteStream(string ext = null, string tag = null);
	string GetLabel(string name, string ext = null, string tag = null);
	IProgress<double> Progress { get; set; }
	Func<Stream> WriteFactory(string ext = null);
}
