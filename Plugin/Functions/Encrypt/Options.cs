using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Encrypt;

public sealed class Options : IOptions
{
	public bool DoDecryption;
	public byte[] Password;
	public string UserPassword;
	public byte[] IVBytes;
	//public byte[] SaltBytes = Encryptor.DefaultSalt;
	public bool TreatPassAsRaw = false;
	public bool TestMode = false;
	//public int PasswordIterations = Encryptor.DefaultIterations;

	public void Usage(StringBuilder sb)
	{
		sb.ND(1,"Encrypt or Decrypts the pixels of an image");
		sb.ND(1,"Note: (text) is escaped using RegEx syntax so that passing binary data is possible. Also see -raw option");
		sb.ND(1,"-d"            ,"Enable decryption (default is to encrypt)");
		sb.ND(1,"-p (text)"     ,"Password used to encrypt / decrypt image");
		sb.ND(1,"-pi"           ,"Ask for the password on the command prompt (instead of -p)");
		sb.ND(1,"-raw"          ,"Treat password text as a raw string (shell escaping still applies)");
		sb.ND(1,"-iv (text)"    ,"Initialization Vector - must be exactly "+Encryptor.IVLengthBytes+" bytes");
		sb.ND(1,"-test"         ,"Print out any specified (text) inputs as hex and exit");
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if (p.Has("-raw").IsGood()) {
			TreatPassAsRaw = true;
		}
		if (p.Has("-d").IsGood()) {
			DoDecryption = true;
		}
		if (p.Has("-test").IsGood()) {
			TestMode = true;
		}
		if (p.Default("-iv",out IVBytes,null,Encryptor.TryStringToBytes).IsInvalid()) {
			Tell.CouldNotParse("-iv");
			return false;
		}

		var ppass = p.Default("-p",out UserPassword);
		if (ppass.IsInvalid()) {
			Tell.CouldNotParse("-p");
			return false;
		}
		else if (ppass.IsMissing() && p.Has("-pi").IsGood()) {
			if (!TryPromptForPassword(out UserPassword)) {
				Tell.InvalidPassword();
				return false;
			}
		}

		bool goodPass = false;
		if (!String.IsNullOrEmpty(UserPassword)) {
			if (TreatPassAsRaw) {
				Password = Encoding.UTF8.GetBytes(UserPassword);
				goodPass = true;
			}
			else if (Encryptor.TryStringToBytes(UserPassword,out Password)) {
				goodPass = true;
			}
		}
		if (!goodPass) {
			Tell.InvalidPassword();
			return false;
		}

		if (IVBytes != null && IVBytes.Length < Encryptor.IVLengthBytes) {
			Tell.MustBeSizeInBytes("-iv",Encryptor.IVLengthBytes);
		}

		if (TestMode) {
			var iv = IVBytes ?? Encryptor.GetIVBytesFromPassword(Password);
			Log.Message(
				"Password As Hex: "
				+"\n "+BytesAsHex(Password)
				+"\nIV As Hex:"
				+"\n "+BytesAsHex(iv)
			);
			return false; //stop the program
		}

		return true;
	}

	static bool TryPromptForPassword(out string pass)
	{
		pass = null;
		Console.Write("password: ");
		StringBuilder sb = new StringBuilder();
		while(true) {
			var key = Console.ReadKey(true);
			if (key.Key == ConsoleKey.Enter) { break; }
			sb.Append(key.KeyChar);
		}
		pass = sb.ToString();
		if (String.IsNullOrWhiteSpace(pass)) {
			return false;
		}
		return true;
	}

	static string BytesAsHex(byte[] data)
	{
		var sb = new StringBuilder();
		sb.Append('[');
		bool isFirst = true;
		foreach(byte b in data) {
			if (!isFirst) {
				sb.Append(' ');
			} else {
				isFirst = false;
			}
			sb.Append(b.ToString("X2"));
		}
		sb.Append("] L=" + data.Length);
		return sb.ToString();
	}
}
