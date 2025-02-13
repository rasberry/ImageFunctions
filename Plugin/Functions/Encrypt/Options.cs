using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Encrypt;

public sealed class Options : IOptions, IUsageProvider
{
	public bool DoDecryption;
	public byte[] Password;
	public string UserPassword;
	public byte[] IVBytes;
	//public byte[] SaltBytes = Encryptor.DefaultSalt;
	public bool TreatPassAsRaw = false;
	public bool TestMode = false;
	//public int PasswordIterations = Encryptor.DefaultIterations;
	readonly ICoreLog Log;

	public Options(IFunctionContext context)
	{
		if(context == null) { throw Squeal.ArgumentNull(nameof(context)); }
		Log = context.Log;
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1,
				"Encrypt or Decrypts the pixels of an image",
				"Note: (text) is escaped using RegEx syntax so that passing binary data is possible. Also see -raw option"
			),
			Parameters = [
				new UsageOne<bool>(1, "-d", "Enable decryption (default is to encrypt"),
				new UsageOne<string>(1, "-p", "Password used to encrypt / decrypt image") { TypeText = "text" },
				new UsageOne<bool>(1, "-pi", "Ask for the password on the command prompt (instead of -p)"),
				new UsageOne<bool>(1, "-raw", "Treat password text as a raw string (shell escaping still applies)"),
				new UsageOne<string>(1, "-iv", "Initialization Vector - must be exactly " + Encryptor.IVLengthBytes + " bytes") { TypeText = "text" },
				new UsageOne<bool>(1, "-test", "Print out any specified (text) inputs as hex and exit")
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if(p.Has("-raw").IsGood()) {
			TreatPassAsRaw = true;
		}
		if(p.Has("-d").IsGood()) {
			DoDecryption = true;
		}
		if(p.Has("-test").IsGood()) {
			TestMode = true;
		}

		if(p.Scan<byte[]>("-iv", par: Encryptor.StringToBytes)
			.WhenGood(r => { IVBytes = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<string>("-p")
			.WhenGood(r => { UserPassword = r.Value; return r; })
			.WhenMissing(r => {
				if(p.Has("-pi").IsGood()) {
					if(!TryPromptForPassword(out UserPassword)) {
						Log.Error(Note.InvalidPassword());
						return r;
					}
					return r with { Result = ParseParams.Result.Good };
				}
				return r;
			})
			.WhenUnParsable(r => { Log.Error(Note.CouldNotParse(r.Name, r.Value), r.Error); return r; })
			.IsInvalid()
		) {
			return false;
		}

		bool goodPass = false;
		if(!String.IsNullOrEmpty(UserPassword)) {
			if(TreatPassAsRaw) {
				Password = Encoding.UTF8.GetBytes(UserPassword);
				goodPass = true;
			}
			else if(Encryptor.TryStringToBytes(UserPassword, out Password)) {
				goodPass = true;
			}
		}
		if(!goodPass) {
			Log.Error(Note.InvalidPassword());
			return false;
		}

		if(IVBytes != null && IVBytes.Length < Encryptor.IVLengthBytes) {
			Log.Error(Note.MustBeSizeInBytes("-iv", Encryptor.IVLengthBytes));
		}

		if(TestMode) {
			var iv = IVBytes ?? Encryptor.GetIVBytesFromPassword(Password);
			Log.Message(
				"Password As Hex: "
				+ "\n " + BytesAsHex(Password)
				+ "\nIV As Hex:"
				+ "\n " + BytesAsHex(iv)
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
			if(key.Key == ConsoleKey.Enter) { break; }
			sb.Append(key.KeyChar);
		}
		pass = sb.ToString();
		if(String.IsNullOrWhiteSpace(pass)) {
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
			if(!isFirst) {
				sb.Append(' ');
			}
			else {
				isFirst = false;
			}
			sb.Append(b.ToString("X2"));
		}
		sb.Append("] L=" + data.Length);
		return sb.ToString();
	}
}
