using System;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
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
					UserPassword = args[a];
				}
				else if (curr == "-pi") {
					if (!TryPromptForPassword(out UserPassword)) {
						Log.Error("invalid password");
						return false;
					}
				}
				else if (curr == "-d") {
					DoDecryption = true;
				}
				else if (String.IsNullOrEmpty(InImage)) {
					InImage = curr;
				}
				else if (String.IsNullOrEmpty(OutImage)) {
					OutImage = curr;
				}
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
			if (String.IsNullOrEmpty(UserPassword) || !Encryptor.TryStringToBytes(UserPassword,out Password)) {
				Log.Error("missing or invalid password");
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
			return true;
		}

		protected override void Process(IImageProcessingContext<Rgba32> ctx)
		{
			var proc = new Processor<Rgba32>();
			proc.DoDecryption = DoDecryption;
			proc.Password = Password;
			if (Rect.IsEmpty) {
				Log.Debug("rect is empty "+Rect);
				ctx.ApplyProcessor(proc);
			} else {
				Log.Debug("rect is full "+Rect);
				ctx.ApplyProcessor(proc,Rect);
			}
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Action.PixelateDetails);
			sb.AppendLine();
			sb.AppendLine(name + " [options] (input image) [output image]");
			sb.AppendLine(" Encrypt or Decrypts all or parts of an image");
			sb.AppendLine(" -d                          Enable decryption (default is to encrypt)");
			sb.AppendLine(" -p (password)               Password used to encrypt / decrypt image");
			sb.AppendLine(" -pi                         Ask for the password on the command prompt (instead of -p)");
		}

		bool DoDecryption = false;
		byte[] Password = null;
		string UserPassword = null;

	}
}