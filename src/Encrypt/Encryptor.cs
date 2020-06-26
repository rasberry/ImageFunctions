using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

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
}
