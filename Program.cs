using System;
using System.IO;
using System.Linq;

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
			if (!Options.Parse(args)) {
				return;
			}
			IFunction func = Registry.Map(Options.Which);
			if (!func.ParseArgs(args.Skip(1).ToArray())) {
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
