using System;
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
		static Random Rnd = new Random(513);
		static bool RndChoice() {
			return Rnd.Next(0,2) == 0;
		}

		static void RunTest()
		{
			// http://tromp.github.io/maze.html
			char[] M = new char[2];
			int H = 20;
			int C;
			int E;
			int[] L = new int[40];
			int[] R = new int[40];

			L[0] = 1;
			for (E = 40; --E >= 1; L[E] = R[E] = E) {
				Console.Write("._");
			}
			Console.Write("\n|");

			while(--H >= 0) {
				for (C = 40; --C >= 1; Console.Write(M)) {
					if (C != (E=L[C-1]) && RndChoice()) {
						R[E] = R[C];
						L[R[C]] = E;
						R[C] = C-1;
						L[C-1] = C;
						M[1] = '.';
					}
					else {
						M[1] = '|';
					}
					if (C != (E=L[C]) && RndChoice()) {
						R[E] = R[C];
						L[R[C]] = E;
						L[C] = C;
						R[C] = C;
						M[0] = '_';
					}
					else {
						M[0] = ' ';
					}
				}
				Console.Write("\n|");
			}

			M[0] = '_';
			for (C = 40; --C >=1; Console.Write(M)) {
				if (C != (E=L[C-1]) && (C == R[C] || 6<<27<Rnd.Next())) {
					L[R[E]=R[C]]=E;
					L[R[C]=C-1]=C;
					M[1] = '.';
				}
				else {
					M[1] = '|';
				}
				E = L[C];
				R[E] = R[C];
				L[R[C]] = E;
				L[C] = C;
				R[C] = C;
			}
			Console.WriteLine();


#if false
char M[3];		/* holds the 2 characters printed for each cell */
int H,			/* height of the maze */
    C,			/* current cell */
    E,			/* temporary pointer used in the updating */
    L[40],R[40];        /* left and right pointers */

main()
{
  L[0] = scanf("%d",&H);		/* reads height and sets L[0] to 1 */
  for (E = 40; --E; L[E] = R[E] = E)
    printf("._");			/* close top of maze */
  printf("\n|");
  while (--H)                           /* more rows to do */
  { for (C = 40; --C; printf(M))	/* visit cells from left to right */
    { if (C != (E=L[C-1]) && 6<<27<rand())	/* make right-connection ? */
      { R[E] = R[C];			/* link E */
        L[R[C]] = E;			/* to R[C] */
        R[C] = C-1;			/* link C */
        L[C-1] = C;			/* to C-1 */
        M[1] = '.';			/* no wall to the right */
      }
      else M[1] = '|';			/* wall to the right */
      if (C != (E=L[C]) && 6<<27<rand()) 	/* omit down-connection ? */
      { R[E] = R[C];			/* link E */
        L[R[C]] = E;			/* to R[C] */
        L[C] = C;			/* link C */
        R[C] = C;			/* to C */
        M[0] = '_';			/* wall downward */
      }
      else M[0] = ' ';			/* no wall downward */
    }
    printf("\n|");
  }
  M[0] = '_';				/* close bottom of maze */
  for (C = 40; --C; printf(M))		/* bottom row */
  { if (C != (E=L[C-1]) && (C == R[C] || 6<<27<rand()))
    { L[R[E]=R[C]]=E;
      L[R[C]=C-1]=C;
      M[1] = '.';
    }
    else M[1] = '|';
    E = L[C];
    R[E] = R[C];
    L[R[C]] = E;
    L[C] = C;
    R[C] = C;
  }
  printf("\n");
}
#endif
		}
		#endif
	}
}
