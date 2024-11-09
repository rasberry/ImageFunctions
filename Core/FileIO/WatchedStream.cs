namespace ImageFunctions.Core.FileIO;

public sealed class WatchedStream : Stream
{
	public WatchedStream(Stream s, IProgress<double> progress)
	{
		Progress = progress;
		InnerStream = s;
		IsDisposed = false;
	}

	readonly Stream InnerStream;
	readonly IProgress<double> Progress;

	public override bool CanRead => InnerStream.CanRead;
	public override bool CanSeek => InnerStream.CanSeek;
	public override bool CanWrite => InnerStream.CanWrite;
	public override long Length => InnerStream.Length;
	public override long Position {
		get => InnerStream.Position;
		set => InnerStream.Position = value;
	}

	public override void Flush()
	{
		InnerStream.Flush();
		UpdateProgress();
	}
	public override int Read(byte[] buffer, int offset, int count)
	{
		var r = InnerStream.Read(buffer, offset, count);
		UpdateProgress();
		return r;
	}
	public override long Seek(long offset, SeekOrigin origin)
	{
		var l = InnerStream.Seek(offset, origin);
		UpdateProgress();
		return l;
	}
	public override void SetLength(long value)
	{
		InnerStream.SetLength(value);
		UpdateProgress();
	}
	public override void Write(byte[] buffer, int offset, int count)
	{
		InnerStream.Write(buffer, offset, count);
		UpdateProgress();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if(!IsDisposed) {
			InnerStream.Dispose();
			IsDisposed = true;
		}
	}

	void UpdateProgress()
	{
		if(Progress == null || InnerStream == null) { return; }
		double pct = (double)InnerStream.Position / InnerStream.Length;
		Progress.Report(pct);
	}

	bool IsDisposed;
}
