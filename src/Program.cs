using System;
using System.IO;
using System.Text;
using SixLabors.Primitives;

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
			if (args[0] == "test") {
				RunTest();
				return;
			}

			//parse initial options - determines which action to do
			if (!Options.Parse(args, out var pruned)) {
				return;
			}

			//map / parse action specific arguments
			IFunction func = Registry.Map(Options.Which);
			if (!MapOptions(func)) {
				return;
			}
			if (!func.ParseArgs(pruned)) {
				return;
			}

			//kick off action
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

		static bool MapOptions(IFunction func)
		{
			IGenerator iGen = func as IGenerator;
			//generators must be given a size
			if (iGen != null) {
				if (Options.Bounds == Rectangle.Empty) {
					var size = iGen.StartingSize;
					if (size == Size.Empty) {
						Log.Error($"{Options.Which} doesn't provide an initial size so you must include the --rect option");
						return false;
					}
					func.Bounds = new Rectangle(0,0,size.Width,size.Height);
				}
				else {
					func.Bounds = Options.Bounds;
				}
			}
			else {
				func.Bounds = Options.Bounds;
			}

			func.MaxDegreeOfParallelism = Options.MaxDegreeOfParallelism;
			return true;
		}

		static void RunTest()
		{
			//for(int i=0; i<=80; i++) {
			//	var (x,y) = Helpers.MathHelpers.DiagonalToXY(i);
			//	var p = Helpers.MathHelpers.XYToDiagonal(x,y);
			//	Console.WriteLine($"i={i} x={x} y={y} p={p}");
			//}

			for(long i=0; i<100; i++) {
				var sb = new StringBuilder();
				long n = i;
				while(true) {
					long c = UlamSpiral.Primes.IsCompositeWhy(n);
					sb.Append($" {c}");
					if (c < 2) { break; }
					n /= c;
				}
				Log.Debug($"i={i} [{sb.ToString()}]");
			}
		}
	}
}
