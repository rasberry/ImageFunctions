using System.Data;

namespace ImageFunctions.Core.FileIO;


public sealed class RelayClerk : IFileClerk
{
	public RelayClerk(string label)
	{
		Label = label;
	}

	readonly string Label;
	public IProgress<double> Progress { get; set; }

	public void Dispose()
	{
		var args = new EventArgs();
		OnDispose?.Invoke(this, args);
	}

	public string GetLabel(string name, string ext = null, string tag = null)
	{
		if(String.IsNullOrWhiteSpace(name)) { name = "item"; }
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
		return name;
	}

	public Stream ReadStream(string ext = null, string tag = null)
	{
		if(OneStream != null) {
			throw new InvalidOperationException("stream already created");
		}
		var args = new StreamEventArgs(Label, ext, tag);
		AqureRead?.Invoke(this, args);
		if(args.Source == null) {
			throw new NoNullAllowedException($"{nameof(args.Source)} must not be null");
			//Squeal.SourceMustNotBeNull();
		}

		OneStream = (args.Source is WatchedStream _source)
			? _source
			: new WatchedStream(args.Source, Progress)
		;
		return OneStream;
	}

	public Stream WriteStream(string ext = null, string tag = null)
	{
		if(OneStream != null) {
			throw new InvalidOperationException("stream already created");
		}
		var args = new StreamEventArgs(Label, ext, tag);
		AqureWrite?.Invoke(this, args);
		if(args.Source == null) {
			throw new NoNullAllowedException($"{nameof(args.Source)} must not be null");
			//Squeal.SourceMustNotBeNull();
		}
		OneStream = (args.Source is WatchedStream _source)
			? _source
			: new WatchedStream(args.Source, Progress)
		;
		return OneStream;
	}

	WatchedStream OneStream;

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
		OneStream = null; //bypass OneStream checks
		return stream;
	}

	int FactoryCount;
	string FactoryExt;

	public event EventHandler<StreamEventArgs> AqureRead;
	public event EventHandler<StreamEventArgs> AqureWrite;
	public event EventHandler<EventArgs> OnDispose;

	public class StreamEventArgs : EventArgs
	{
		public StreamEventArgs(string name = null, string ext = null, string tag = null)
		{
			Name = name;
			Extension = ext;
			Tag = tag;
		}

		public string Name { get; private set; }
		public string Extension { get; private set; }
		public string Tag { get; private set; }
		public Stream Source { get; set; }
	}
}
