namespace ImageFunctions.Core.FileIO;

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

	public string GetLabel(string name, string ext = null, string tag = null)
	{
		//give name a default
		if(String.IsNullOrWhiteSpace(name)) { name = "item"; }
		return TransformLocation(name, ext, tag);
	}

	public Stream ReadStream(string ext = null, string tag = null)
	{
		if(OneStream != null) {
			throw new InvalidOperationException("stream already created");
		}
		var loc = TransformLocation(Location, ext, tag);

		var fs = FileIOInst.OpenForReading(loc);
		OneStream = new WatchedStream(fs, Progress);
		return OneStream;
	}

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

	string TransformLocation(string name, string ext, string tag)
	{
		if(String.IsNullOrWhiteSpace(name)) {
			Squeal.ArgumentNullOrEmpty(nameof(name));
		}

		// save the path for later
		var path = Path.GetDirectoryName(name);
		//grab noext in case we have a tag
		var noext = Path.GetFileNameWithoutExtension(name);
		//if ext is not specified get it from the name
		if(String.IsNullOrWhiteSpace(ext)) { ext = Path.GetExtension(name); }
		//ensure extension has a dot
		if(!String.IsNullOrWhiteSpace(ext) && !ext.StartsWith('.')) { ext = '.' + ext; }
		//if we have a tag incorporate it
		if(!String.IsNullOrWhiteSpace(tag)) { name = noext + "-" + tag + ext; }
		//if ext has a value at this point ensure the name is set to that ext
		if(!String.IsNullOrWhiteSpace(ext)) { name = Path.ChangeExtension(name, ext); }
		//all done
		return Path.Combine(path, name);
	}

	WatchedStream OneStream;
}
