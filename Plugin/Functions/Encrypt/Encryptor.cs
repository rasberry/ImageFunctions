using System.Security.Cryptography;

namespace ImageFunctions.Plugin.Functions.Encrypt;

// https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aes?view=net-7.0
public class Encryptor
{
	public static byte[] StringToBytes(string s)
	{
		string ue = System.Text.RegularExpressions.Regex.Unescape(s ?? "");
		return Encoding.UTF8.GetBytes(ue);
	}

	public static bool TryStringToBytes(string s, out byte[] bytes)
	{
		bytes = null;
		try {
			bytes = StringToBytes(s);
			return true;
		}
		catch(ArgumentException) {}
		return false;
	}

	public byte[] IVBytes { get; set; }

	public void TransformStream(bool decrypt, Stream inData, Stream outData,
		byte[] password, IProgress<double> progress = null)
	{
		if (IVBytes == null) {
			IVBytes = GetIVBytesFromPassword(password);
		}

		using var agent = Aes.Create();
		agent.Key = SHA256.HashData(password);
		agent.Mode = CipherMode.CBC;
		agent.IV = IVBytes;
		//if i use the default padding i get an error - so using zeros for now
		// System.Security.Cryptography.CryptographicException: Padding is invalid and cannot be removed.
		agent.Padding = PaddingMode.Zeros;

		using var encryptor = decrypt
			? agent.CreateDecryptor()
			: agent.CreateEncryptor()
		;
		using var cryptoStream = new CryptoStream(outData, encryptor, CryptoStreamMode.Write);

		CopyToWithProgress(inData,cryptoStream,progress);
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

	// could have used the one from OpenSSL but it looks hard to implement
	// https://github.com/openssl/openssl/blob/master/crypto/evp/evp_key.c
	public static byte[] GetIVBytesFromPassword(byte[] pBytes) {
		if (pBytes == null || pBytes.Length < 1) {
			Core.Squeal.ArgumentNullOrEmpty(nameof(pBytes));
		}

		//this is used a buffer to store the final bytes
		var ivBytes = new byte[IVLengthBytes];

		// we need IVLength bytes so if the password is shorted than that, add padding
		if (pBytes.Length < IVLengthBytes) {
			for(int s = pBytes.Length; s < IVLengthBytes; s++) {
				ivBytes[s] = DefaultIV[s];
			}
		}
		// copy the password bytes
		for(int b = 0; b < pBytes.Length; b++) {
			ivBytes[b] = pBytes[b];
		}

		//hash the password + padding
		var hashBytes = SHA256.HashData(ivBytes);
		if (hashBytes.Length != SHA256LengthBytes) {
			throw PlugSqueal.OutOfRange(nameof(hashBytes),$"SHA256.HashData returned {hashBytes.Length} instead of {SHA256LengthBytes}");
		}

		//copy the hashed bytes back to the iv
		// Note: SHA256Length must be bigger (or equal to) IVLength
		for(int i = 0; i < IVLengthBytes; i++) {
			ivBytes[i] = hashBytes[i];
		}

		//if we have extra bytes feed them back into the iv
		int diff = SHA256LengthBytes - IVLengthBytes;
		for(int i = 0; i < diff; i++) {
			byte extra = hashBytes[IVLengthBytes + i];
			ivBytes[i] ^= extra;
		}

		return ivBytes;
	}

	//IV must be same size as block (128 bits)
	static readonly byte[] DefaultIV = new byte[] {
		0xA, 0xB, 0xB, 0xA, 0xD, 0xE, 0xF, 0xA,
		0xC, 0xE, 0xD, 0xF, 0xA, 0xD, 0xE, 0xD
	};

	public const int IVLengthBytes = 16;
	const int SHA256LengthBytes = 32;
}
