using System.Collections.Concurrent;

namespace ImageFunctions.Core;

public sealed class FileClerk : IFileClerk
{
	public string Location { get; set; }
	public IProgress<double> Progress { get; set; }
	public Stream Source { get; set; }

	public void Dispose()
	{
		if (Watcher == null) { return; }
		foreach(var w in Watcher) {
			if (w != null && !w.IsDisposed) {
				w.Dispose();
			}
		}
		Watcher.Clear();
		Watcher = null;
	}

	public Stream ReadStream(string ext = null, string tag = null)
	{
		WatchedStream watched;
		if (Source != null) {
			watched = new WatchedStream(Source, Progress);
		}
		else {
			var loc = GetFinal(ext,tag);
			var fs = File.Open(loc, FileMode.Open, FileAccess.Read, FileShare.Read);
			watched = new WatchedStream(fs, Progress);
		}

		Watcher.Add(watched);
		return watched;
	}

	public Stream WriteStream(string ext = null, string tag = null)
	{
		WatchedStream watched;
		if (Source != null) {
			watched = new WatchedStream(Source, Progress);
		}
		else {
			var loc = GetFinal(ext,tag);
			var fs = File.Open(loc, FileMode.Create, FileAccess.Write, FileShare.Read);
			watched = new WatchedStream(fs, Progress);
		}
		Watcher.Add(watched);
		return watched;
	}

	string GetFinal(string ext = null, string tag = null)
	{
		string loc = Location;
		if (String.IsNullOrWhiteSpace(loc)) {
			Squeal.ArgumentNullOrEmpty(nameof(Location));
		}
		if (!String.IsNullOrWhiteSpace(ext)) {
			loc = Path.ChangeExtension(loc, ext);
		}
		if (!string.IsNullOrWhiteSpace(tag)) {
			var xt = Path.GetExtension(loc);
			var name = Path.GetFileNameWithoutExtension(loc);
			loc = $"{name}{tag}{xt}";
		}
		return loc;
	}

	ConcurrentBag<WatchedStream> Watcher = new();

	sealed class WatchedStream : Stream
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

		public override void Flush() {
			InnerStream.Flush();
			UpdateProgress();
		}
		public override int Read(byte[] buffer, int offset, int count) {
			var r = InnerStream.Read(buffer, offset, count);
			UpdateProgress();
			return r;
		}
		public override long Seek(long offset, SeekOrigin origin) {
			var l = InnerStream.Seek(offset, origin);
			UpdateProgress();
			return l;
		}
		public override void SetLength(long value) {
			InnerStream.SetLength(value);
			UpdateProgress();
		}
		public override void Write(byte[] buffer, int offset, int count) {
			InnerStream.Write(buffer, offset, count);
			UpdateProgress();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!IsDisposed) {
				InnerStream.Dispose();
				IsDisposed = true;
			}
		}

		void UpdateProgress()
		{
			if (Progress == null) { return; }
			double pct = (double)InnerStream.Position / InnerStream.Length;
			Progress.Report(pct);
		}

		public bool IsDisposed;
	}
}