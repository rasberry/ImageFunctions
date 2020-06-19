using System;
using System.IO;
using System.Drawing;
using ImageFunctions.Helpers;

namespace ImageFunctions.Encrypt
{
	public class PixelStream : Stream
	{
		public PixelStream(IFImage image)
		{
			Image = image;
			//4 bytes per pixel
			InternalLength = (long)BPP * (long)Image.Height * (long)Image.Width;
		}

		IFImage Image = null;
		long InternalLength = 0;
		int PaddingLength = 0;
		const int BPP = 4; //Bytes per pixel

		public override bool CanRead { get { return true; }}
		public override bool CanSeek { get { return true; }}
		public override bool CanWrite { get { return true; }}
		public override long Position { get; set; }
		public override long Length { get { return InternalLength + PaddingLength; }}

		int? BlockSize = null;
		public int? PadToBlockSize {
			get {
				return BlockSize;
			}
			set {
				BlockSize = value;
				if (BlockSize.HasValue) {
					PaddingLength = (int)(InternalLength % BlockSize.Value);
				}
			}
		}

		public override void Flush()
		{
			//no need for this
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int b=0;
			for(b=0; b<count; b++) {
				int comp = ReadByte();
				if (comp < 0) { break; }
				buffer[offset + b] = (byte)comp;
			}
			return b;
		}

		public override int ReadByte()
		{
			if (Position >= Length) {
				return -1;
			}
			if (Position >= InternalLength) {
				Position++;
				return 0; //padding
			}

			int pos = (int)(Position / BPP);
			int elem = (int)(Position % BPP);

			//load up the pixel
			if (elem == 0) {
				int y = pos / Image.Width;
				int x = pos % Image.Width;
				//only supporting 24bit(+alpha) color for now
				var c = ImageHelpers.NativeToRgba(Image[x,y]);
				tempA = c.A;
				tempR = c.R;
				tempG = c.G;
				tempB = c.B;
			}
		
			//stream out byte at a time
			byte comp;
			switch(elem) {
				default:
				case 0: comp = tempR; break;
				case 1: comp = tempG; break;
				case 2: comp = tempB; break;
				case 3: comp = tempA; break;
			}
			Position++;
			return (int)comp;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Begin) {
				Position = offset;
			}
			else if (origin == SeekOrigin.Current) {
				Position += offset;
			}
			else if (origin == SeekOrigin.End) {
				Position = Length + offset;
			}
			if (Position < 0 || Position >= Length) {
				throw new IndexOutOfRangeException();
			}
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			for(int b = 0; b < count; b++) {
				WriteByte(buffer[b + offset]);
			}
		}

		public override void WriteByte(byte value)
		{
			if (Position >= Length) {
				return;
			}
			if (Position >= InternalLength) {
				Position++;
				return; //ignore padding
			}

			int pos = (int)(Position / BPP);
			int elem = (int)(Position % BPP);
			
			//stream in bytes
			switch(elem) {
				default:
				case 0: tempR = value; break;
				case 1: tempG = value; break;
				case 2: tempB = value; break;
				case 3: tempA = value; break;
			}

			//save pixel
			if (elem + 1 == BPP) {
				int y = pos / Image.Width;
				int x = pos % Image.Width;
				var c = Color.FromArgb(tempA,tempR,tempG,tempB);
				Image[x,y] = ImageHelpers.RgbaToNative(c);
			}
			Position++;
		}

		byte tempR,tempG,tempB,tempA;

	}

	#if false
	public class PixelStream<TPixel> : Stream
		where TPixel : struct, IPixel<TPixel>
	{
		public PixelStream(Image<TPixel> image) : this(image.Frames.RootFrame)
		{
		}

		public PixelStream(ImageFrame<TPixel> image)
		{
			Image = image;
			//4 bytes per pixel
			InternalLength = 4L * Image.Height * Image.Width;
		}

		ImageFrame<TPixel> Image = null;
		long InternalLength = 0;
		int PaddingLength = 0;

		public override bool CanRead { get { return true; }}
		public override bool CanSeek { get { return true; }}
		public override bool CanWrite { get { return true; }}
		public override long Position { get; set; }
		public override long Length { get { return InternalLength + PaddingLength; }}

		int? BlockSize = null;
		public int? PadToBlockSize {
			get {
				return BlockSize;
			}
			set {
				BlockSize = value;
				if (BlockSize.HasValue) {
					PaddingLength = (int)(InternalLength % BlockSize.Value);
				}
			}
		}

		public override void Flush()
		{
			//no need for this
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int b=0;
			for(b=0; b<count; b++) {
				int comp = ReadByte();
				if (comp < 0) { break; }
				buffer[offset + b] = (byte)comp;
			}
			return b;
		}

		public override int ReadByte()
		{
			if (Position >= Length) {
				return -1;
			}
			if (Position >= InternalLength) {
				Position++;
				return 0; //padding
			}

			int pos = (int)(Position / 4);
			int elem = (int)(Position % 4);

			//only supporting 24bit(+alpha) color for now
			Rgba32 c = default(Rgba32);
			Image.GetPixelSpan()[pos].ToRgba32(ref c);
			byte comp;
			switch(elem) {
				default:
				case 0: comp = c.R; break;
				case 1: comp = c.G; break;
				case 2: comp = c.B; break;
				case 3: comp = c.A; break;
			}
			Position++;
			return (int)comp;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (origin == SeekOrigin.Begin) {
				Position = offset;
			}
			else if (origin == SeekOrigin.Current) {
				Position += offset;
			}
			else if (origin == SeekOrigin.End) {
				Position = Length + offset;
			}
			if (Position < 0 || Position >= Length) {
				throw new IndexOutOfRangeException();
			}
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			for(int b = 0; b < count; b++) {
				WriteByte(buffer[b + offset]);
			}
		}

		public override void WriteByte(byte value)
		{
			if (Position >= Length) {
				return;
			}
			if (Position >= InternalLength) {
				Position++;
				return; //ignore padding
			}

			int pos = (int)(Position / 4);
			int elem = (int)(Position % 4);
			var span = Image.GetPixelSpan();

			Rgba32 c = default(Rgba32);
			span[pos].ToRgba32(ref c);
			switch(elem) {
				default:
				case 0: c.R = value; break;
				case 1: c.G = value; break;
				case 2: c.B = value; break;
				case 3: c.A = value; break;
			}
			span[pos].FromRgba32(c);
			Position++;
		}
	}
	#endif
}