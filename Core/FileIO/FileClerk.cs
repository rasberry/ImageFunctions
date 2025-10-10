namespace ImageFunctions.Core.FileIO;

/// <inheritdoc />
public sealed class FileClerk : IFileClerk
{
	public FileClerk(IFileIO fileIOinst, string location)
	{
		Location = location;
		FileIOInst = fileIOinst;
	}

	public IProgress<double> Progress { get; set; }
	readonly string Location;
	readonly IFileIO FileIOInst;

	public void Dispose()
	{
		OneStream?.Dispose();
		foreach(var w in Watcher) { w?.Dispose(); }
		Watcher.Clear();
	}

	/// <inheritdoc />
	public string GetLabel(string name, string ext = null, string tag = null)
	{
		//give name a default
		if(String.IsNullOrWhiteSpace(name)) {
			name = Path.GetFileNameWithoutExtension(Location);
		}
		return TransformLocation(name, ext, tag);
	}

	/// <inheritdoc />
	public Stream ReadStream(string ext = null, string tag = null)
	{
		if(OneStream != null) {
			throw new InvalidOperationException("stream already created");
		}

		var loc = TransformLocation(Location, ext, tag);
		System.Diagnostics.Trace.WriteLine($"Location={Location} loc={loc}");

		var fs = FileIOInst.OpenForReading(loc);
		OneStream = new WatchedStream(fs, Progress);
		return OneStream;
	}

	/// <inheritdoc />
	public Stream WriteStream(string ext = null, string tag = null)
	{
		if(OneStream != null) {
			throw new InvalidOperationException("stream already created");
		}
		var loc = TransformLocation(Location, ext, tag);

		var fs = FileIOInst.OpenForWriting(loc);
		OneStream = new WatchedStream(fs, Progress);
		return OneStream;
	}

	/// <inheritdoc />
	public Func<Stream> WriteFactory(string ext = null)
	{
		if(FactoryCount != 0) {
			throw new InvalidOperationException("factory already created");
		}
		FactoryExt = ext;
		return FactoryWrite;
	}

	Stream FactoryWrite()
	{
		if(OneStream != null) {
			throw new InvalidOperationException("factory already created");
		}
		FactoryCount++;
		var stream = WriteStream(FactoryExt, $"{FactoryCount}");
		Watcher.Add((WatchedStream)stream);
		OneStream = null; //bypass OneStream checks
		return stream;
	}

	string FactoryExt;
	int FactoryCount;
	readonly List<WatchedStream> Watcher = new();

	string TransformLocation(string original, string ext, string tag)
	{
		if(String.IsNullOrWhiteSpace(original)) {
			Squeal.ArgumentNullOrEmpty(nameof(original));
		}

		// save the path for later
		var path = Path.GetDirectoryName(original);
		//grab noext in case we have a tag
		var noext = Path.GetFileNameWithoutExtension(original);
		//if ext is not specified get it from the name
		if(String.IsNullOrWhiteSpace(ext)) { ext = Path.GetExtension(original); }
		//ensure extension has a dot
		if(!String.IsNullOrWhiteSpace(ext) && !ext.StartsWith('.')) { ext = '.' + ext; }
		//if we have a tag incorporate it
		if(!String.IsNullOrWhiteSpace(tag)) { noext += "-" + tag; }
		//all done
		return Path.Combine(path, noext + ext);
	}

	WatchedStream OneStream;
}
