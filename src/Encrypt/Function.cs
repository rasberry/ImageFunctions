using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;

namespace ImageFunctions.Encrypt
{
	public class Function : AbstractFunction
	{
		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.Has("-raw").IsGood()) {
				O.TreatPassAsRaw = true;
			}
			if (p.Has("-d").IsGood()) {
				O.DoDecryption = true;
			}
			if (p.Has("-test").IsGood()) {
				O.TestMode = true;
			}
			if (p.Default("-iv",out O.IVBytes,Encryptor.DefaultIV,Encryptor.TryStringToBytes)
				.BeSizeInBytes("-iv",O.IVBytes,Encryptor.BlockSizeInBytes).IsInvalid()) {
				return false;
			}
			if(p.Default("-salt",out O.SaltBytes,Encryptor.DefaultSalt,Encryptor.TryStringToBytes)
				.BeSizeInBytes("-salt",O.SaltBytes,Encryptor.MinSaltBytes,true).IsInvalid()) {
				return false;
			}
			if(p.Default("-iter",out O.PasswordIterations,Encryptor.DefaultIterations)
				.BeGreaterThanZero("-iter",O.PasswordIterations).IsInvalid()) {
				return false;
			}

			var ppass = p.Default("-p",out O.UserPassword);
			if (ppass.IsInvalid()) {
				return false;
			}
			else if (ppass.IsMissing() && p.Has("-pi").IsGood()) {
				if (!TryPromptForPassword(out O.UserPassword)) {
					Tell.InvalidPassword();
					return false;
				}
			}

			bool goodPass = false;
			if (!String.IsNullOrEmpty(O.UserPassword)) {
				if (O.TreatPassAsRaw) {
					O.Password = Encoding.UTF8.GetBytes(O.UserPassword);
					goodPass = true;
				}
				else if (Encryptor.TryStringToBytes(O.UserPassword,out O.Password)) {
					goodPass = true;
				}
			}
			if (!goodPass) {
				Tell.InvalidPassword();
				return false;
			}

			if (O.TestMode) {
				Log.Message(
					"Password: "
					+"\n "+BytesAsHex(O.Password)
					+"\nIV:"
					+"\n "+BytesAsHex(O.IVBytes ?? Encryptor.DefaultIV)
					+"\nSalt:"
					+"\n "+BytesAsHex(O.SaltBytes ?? Encryptor.DefaultSalt)
				);
				return false; //stop the program
			}

			if (p.ExpectFile(out InImage,"input image").IsBad()) {
				return false;
			}
			if (p.DefaultFile(out OutImage,InImage).IsBad()) {
				return false;
			}

			return true;
		}

		bool TryPromptForPassword(out string pass)
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

		string BytesAsHex(byte[] data)
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

		public override IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle)
		{
			var proc = new Processor<TPixel>();
			proc.O = O;
			proc.Source = source;
			proc.Bounds = sourceRectangle;
			return proc;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.Encrypt);
			sb.WL();
			sb.WL(0,name + " [options] (input image) [output image]");
			sb.WL(1,"Encrypt or Decrypts all or parts of an image");
			sb.WL(1,"Note: (text) is escaped using RegEx syntax so that passing binary data is possible. Also see -raw option");
			sb.WL(1,"-d"            ,"Enable decryption (default is to encrypt)");
			sb.WL(1,"-p (text)"     ,"Password used to encrypt / decrypt image");
			sb.WL(1,"-pi"           ,"Ask for the password on the command prompt (instead of -p)");
			sb.WL(1,"-raw"          ,"Treat password text as a raw string (shell escaping still applies)");
			sb.WL(1,"-iv (text)"    ,"Initialization Vector - must be exactly "+Encryptor.BlockSizeInBytes+" bytes");
			sb.WL(1,"-salt (text)"  ,"Encryption salt parameter - must be at least "+Encryptor.MinSaltBytes+" bytes long");
			sb.WL(1,"-iter (number)","Number of RFC-2898 rounds to use (default "+Encryptor.DefaultIterations+")");
			sb.WL(1,"-test"         ,"Print out any specified (text) inputs as hex and exit");
		}

		public override void Main()
		{
			Main<Rgba32>();
		}

		Options O = new Options();
	}
}