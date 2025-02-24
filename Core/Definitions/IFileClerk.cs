namespace ImageFunctions.Core;

/// <summary>
/// File Clerk for reading / writing files consistently. Instances of implementing classes should be disposed.
/// </summary>
public interface IFileClerk : IDisposable
{
	/// <summary>
	/// Aquires a stream for reading the file
	/// </summary>
	/// <param name="ext">override the extension</param>
	/// <param name="tag">extra tag for the filename used when reading multiple files</param>
	/// <returns>A <c cref="System.IO.Stream">Stream</c> of data. Do not dispose this stream.</returns>
	Stream ReadStream(string ext = null, string tag = null);

	/// <summary>
	/// Aquires a stream for writing one file
	/// </summary>
	/// <param name="ext">override the extension</param>
	/// <param name="tag">extra tag for the filename used when writing multiple files</param>
	/// <returns>A <c cref="System.IO.Stream">Stream</c> of data. Do not dispose this stream.</returns>
	Stream WriteStream(string ext = null, string tag = null);

	/// <summary>
	/// Generates a label used for when adding layers
	/// </summary>
	/// <param name="name">file name</param>
	/// <param name="ext">override the extension</param>
	/// <param name="tag">extra tag for the filename used when writing multiple files</param>
	string GetLabel(string name, string ext = null, string tag = null);

	/// <summary>
	/// Optional progress tracker
	/// </summary>
	IProgress<double> Progress { get; set; }

	/// <summary>
	/// Writer stream factory to support writing multiple files
	/// </summary>
	/// <param name="ext">override the extension</param>
	/// <returns>A Func<Stream> which creates a new stream when called. Do not dispose this stream.</returns>
	Func<Stream> WriteFactory(string ext = null);
}
