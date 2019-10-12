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
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-p" && ++a<len) {
					O.UserPassword = args[a];
				}
				else if (curr == "-pi") {
					if (!TryPromptForPassword(out O.UserPassword)) {
						Log.Error("invalid password");
						return false;
					}
				}
				else if (curr == "-raw") {
					O.TreatPassAsRaw = true;
				}
				else if (curr == "-d") {
					O.DoDecryption = true;
				}
				else if (curr == "-iv" && ++a<len) {
					if (!Encryptor.TryStringToBytes(args[a],out O.IVBytes)) {
						Log.Error("Invalid IV value");
						return false;
					}
					if (O.IVBytes != null && O.IVBytes.Length != Encryptor.BlockSizeInBytes) {
						Log.Error("IV must be "+Encryptor.BlockSizeInBytes+" bytes");
						return false;
					}
				}
				else if (curr == "-salt" && ++a<len) {
					if (!Encryptor.TryStringToBytes(args[a],out O.SaltBytes)) {
						Log.Error("Invalid Salt value");
						return false;
					}
					if (O.SaltBytes != null && O.SaltBytes.Length < Encryptor.MinSaltBytes) {
						Log.Error("Salt must be at least "+Encryptor.MinSaltBytes+" bytes");
						return false;
					}
				}
				else if (curr == "-iter" && ++a<len) {
					if (!OptionsHelpers.TryParse(args[a],out int iter)) {
						Log.Error("Invalid number of iterations");
						return false;
					}
					if (iter < 1) {
						Log.Error("Iterations must be greater than zero");
						return false;
					}
					O.PasswordIterations = iter;
				}
				else if (curr == "-test") {
					O.TestMode = true;
				}
				else if (String.IsNullOrEmpty(InImage)) {
					InImage = curr;
				}
				else if (String.IsNullOrEmpty(OutImage)) {
					OutImage = curr;
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
				Log.Error("missing or invalid password");
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

			if (String.IsNullOrEmpty(InImage)) {
				Log.Error("input image must be provided");
				return false;
			}
			if (!File.Exists(InImage)) {
				Log.Error("cannot find input image \""+InImage+"\"");
				return false;
			}
			if (String.IsNullOrEmpty(OutImage)) {
				OutImage = OptionsHelpers.CreateOutputFileName(InImage);
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
			proc.SourceRectangle = sourceRectangle;
			return proc;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Action.Encrypt);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" Encrypt or Decrypts all or parts of an image");
			sb.AppendLine(" Note: (text) is escaped using RegEx syntax so that passing binary data is possible. Also see -raw option");
			sb.AppendLine(" -d                          Enable decryption (default is to encrypt)");
			sb.AppendLine(" -p (text)                   Password used to encrypt / decrypt image");
			sb.AppendLine(" -pi                         Ask for the password on the command prompt (instead of -p)");
			sb.AppendLine(" -raw                        Treat password text as a raw string (shell escaping still applies)");
			sb.AppendLine(" -iv (text)                  Initialization Vector - must be exactly "+Encryptor.BlockSizeInBytes+" bytes");
			sb.AppendLine(" -salt (text)                Encryption salt parameter - must be at least "+Encryptor.MinSaltBytes+" bytes long");
			sb.AppendLine(" -iter (number)              Number of RFC-2898 rounds to use (default "+Encryptor.DefaultIterations+")");
			sb.AppendLine(" -test                       Print out any specified (text) inputs as hex and exit");
		}

		Options O = new Options();
	}
}