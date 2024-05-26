using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Maze;

//http://www.neocomputer.org/projects/eller.html
//http://weblog.jamisbuck.org/2010/12/29/maze-generation-eller-s-algorithm
//https://tromp.github.io/maze.html

public class Ellers : IMaze
{
	public Ellers(Options o)
	{
		Rnd = o.RndSeed.HasValue ? new Random(o.RndSeed.Value) : new Random();
	}

	public Action<int, int, PickWall> DrawCell { get; set; }
	public Func<int, int, PickWall, bool> IsBlocked { get; set; }
	public int CellsWide { get; set; }
	public int CellsHigh { get; set; }
	readonly Random Rnd;

	public void DrawMaze(ProgressBar prog)
	{
		PickWall M = PickWall.None;
		int W = CellsWide + 1; //+1 to account for the ghost column
		int H = CellsHigh - 1; //-1 to leave room for the last row
		int C, E, X = 0, Y = 0;
		int[] L = new int[W];
		int[] R = new int[W];

		L[0] = 1; E = W;
		while(--E > 0) { L[E] = R[E] = E; } /* close top of maze */

		while(--H >= 0) {
			C = W; X = 0;
			while(--C > 0) { /* visit cells from left to right */
				if(C != (E = L[C - 1]) && Rnd.RandomChoice()) { /* make right-connection ? */
					R[E] = R[C]; L[R[C]] = E; /* link E to R[C] */
					R[C] = C - 1; L[C - 1] = C; /* link C to C-1 */
					M = M.AddFlag(PickWall.E); /* no wall to the right */
				}
				else {
					M = M.CutFlag(PickWall.E); /* wall to the right */
				}
				if(C != (E = L[C]) && Rnd.RandomChoice()) { /* omit down-connection ? */
					R[E] = R[C]; L[R[C]] = E; /* link E to R[C] */
					L[C] = C; R[C] = C; /* link C to C */
					M = M.CutFlag(PickWall.S); /* wall downward */
				}
				else {
					M = M.AddFlag(PickWall.S); /* no wall downward */
				}
				DrawCell(X, Y, M);
				X++;
			}
			Y++;
		}

		M = M.CutFlag(PickWall.S);
		C = W; X = 0; Y = CellsHigh - 1;
		/* bottom row */
		while(--C > 0) {
			if(C != (E = L[C - 1]) && (C == R[C] || Rnd.RandomChoice())) { /* make right-connection ? */
				R[E] = R[C]; L[R[C]] = E; /* link E to R[C] */
				R[C] = C - 1; L[C - 1] = C; /* link C to C-1 */
				M = M.AddFlag(PickWall.E);
			}
			else {
				M = M.CutFlag(PickWall.E);
			}
			E = L[C]; /* add downward wall */
			R[E] = R[C]; L[R[C]] = E; /* link E to R[C] */
			L[C] = C; R[C] = C; /* link C to C */
			DrawCell(X, Y, M);
			X++;
		}
	}
}
