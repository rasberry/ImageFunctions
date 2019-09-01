using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageFunctions.Encrypt
{
	public class Encryptor
	{
		public const int BlockSizeInBytes = 16;
		public const int MinSaltBytes = 8;
		public const int DefaultIterations = 3119;

		public static bool TryStringToBytes(string s,out byte[] bytes)
		{
			bytes = null;
			try {
				string ue = System.Text.RegularExpressions.Regex.Unescape(s ?? "");
				bytes = Encoding.UTF8.GetBytes(ue);
			}
			catch(ArgumentException) {
				return false;
			}
			return bytes != null;
		}

		public byte[] SaltBytes { get; set; } = null;
		public byte[] IVBytes { get; set; } = null;
		public int KeySize { get; set; } = 256;
		public int PasswordIterations { get; set; } = DefaultIterations;

		public void TransformStream(bool decrypt, Stream inData, Stream outData,
			byte[] password, IProgress<double> progress = null)
		{
			if (SaltBytes == null) { SaltBytes = DefaultSalt; }
			if (IVBytes == null) { IVBytes = DefaultIV; }

			using (var derived = new Rfc2898DeriveBytes(password, SaltBytes, PasswordIterations))
			{
				var keyBytes = derived.GetBytes(KeySize / 8);
				using (var symmetricKey = new RijndaelManaged())
				{
					symmetricKey.BlockSize = BlockSizeInBytes * 8;
					symmetricKey.Mode = CipherMode.CBC;
					symmetricKey.Padding = PaddingMode.Zeros;

					var encryptor = decrypt
						? symmetricKey.CreateDecryptor(keyBytes, IVBytes)
						: symmetricKey.CreateEncryptor(keyBytes, IVBytes)
					;
					var cryptoStream = new CryptoStream(outData, encryptor, CryptoStreamMode.Write);

					using (encryptor) using (cryptoStream) {
						CopyToWithProgress(inData,cryptoStream,progress);
					}
				}
			}
		}

		// https://referencesource.microsoft.com/#mscorlib/system/io/stream.cs,2a0f078c2e0c0aa8
		const int _DefaultCopyBufferSize = 81920;
		void CopyToWithProgress(Stream source, Stream destination,IProgress<double> progress = null)
		{
			byte[] buffer = new byte[_DefaultCopyBufferSize];
			long len = source.Length;
			long count = 0;
			int read;
			while ((read = source.Read(buffer, 0, buffer.Length)) != 0) {
				destination.Write(buffer, 0, read);
				count += read;
				progress?.Report((double)count / len);
			}
		}

		//salt must be at least 8 bytes
		public static readonly byte[] DefaultSalt = new byte[] {
			0xB, 0xE, 0xD, 0xF, 0xA, 0xC, 0xE, 0xD
		};
		//IV must be same size as block (128 bits)
		public static readonly byte[] DefaultIV = new byte[] {
			0xA, 0xB, 0xB, 0xA, 0xD, 0xE, 0xF, 0xA,
			0xC, 0xE, 0xD, 0xF, 0xA, 0xD, 0xE, 0xD
		};
	}

	public class PixelStream<TPixel> : Stream
		where TPixel : struct, IPixel<TPixel>
	{
		public PixelStream(Image<TPixel> image) : this(image.Frames.RootFrame)
		{
		}

		public PixelStream(ImageFrame<TPixel> image)
		{
			Image = image;
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

			Rgba32 c = Image.GetPixelSpan()[pos].ToColor();
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

			Rgba32 c = span[pos].ToColor();
			switch(elem) {
				default:
				case 0: c.R = value; break;
				case 1: c.G = value; break;
				case 2: c.B = value; break;
				case 3: c.A = value; break;
			}
			span[pos] = c.FromColor<TPixel>();
			Position++;
		}
	}
}
