﻿using System;
using System.IO;
using System.Text;
using System.Drawing;

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
			#if DEBUG
			if (args[0] == "test") {
				RunTest();
				return;
			}
			#endif

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

		#if DEBUG
		static void RunTest()
		{
		}
		#endif
	}
}
