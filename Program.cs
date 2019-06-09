using System;
using System.IO;

namespace ImageFunctions
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1) {
				Options.Usage();
				return;
			}
			if (!Options.Parse(args, out var pruned)) {
				return;
			}
			IFunction func = Registry.Map(Options.Which);
			func.Rect = Options.Rect;
			if (!func.ParseArgs(pruned)) {
				return;
			}

			try {
				func.Main();
			}
			catch(Exception e) {
				#if DEBUG
				Log.Error(e.ToString());
				#else
				Log.Error(e.Message);
				#endif
			}
		}
	}
}
