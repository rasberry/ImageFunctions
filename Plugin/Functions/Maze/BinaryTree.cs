using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Maze;

public class BinaryTree : IMaze
{
	public BinaryTree(Options o) {
		Rnd = o.RndSeed.HasValue ? new Random(o.RndSeed.Value) : new Random();
	}

	public Action<int,int,PickWall> DrawCell { get; set; }
	public Func<int,int,PickWall,bool> IsBlocked { get; set; }
	public int CellsWide { get; set; }
	public int CellsHigh { get; set; }

	readonly Random Rnd;

	public void DrawMaze(ProgressBar prog)
	{
		//left edge
		prog.Prefix = "Left Edge ";
		for(int y = 0; y < CellsHigh; y++) {
			DrawPick(0,y,PickWall.N,PickWall.N);
			prog.Report((double)y / CellsHigh);
		}
		//top edge
		prog.Prefix = "Top Edge ";
		for(int x = 0; x < CellsWide; x++) {
			DrawPick(x,0,PickWall.W,PickWall.W);
			prog.Report((double)x / CellsWide);
		}
		//everything else
		prog.Prefix = "Maze ";
		double total = CellsWide * CellsHigh;
		for(int y = 0; y < CellsHigh; y++) {
			for(int x = 0; x < CellsWide; x++ ) {
				DrawPick(x,y,PickWall.W,PickWall.N);
				prog.Report((y * CellsWide + x) / total);
			}
		}
	}

	void DrawPick(int x, int y, PickWall u, PickWall v)
	{
		bool which = Rnd.Next(0,2) == 0;
		PickWall w = which ? u : v;
		DrawCell(x,y,w);
	}
}
