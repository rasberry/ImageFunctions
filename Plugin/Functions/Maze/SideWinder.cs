using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Maze;

// http://weblog.jamisbuck.org/2011/2/3/maze-generation-sidewinder-algorithm
public class SideWinder : IMaze
{
	public SideWinder(Options o) {
		Rnd = o.RndSeed.HasValue ? new Random(o.RndSeed.Value) : new Random();
	}

	public Action<int,int,PickWall> DrawCell { get; set; }
	public Func<int,int,PickWall,bool> IsBlocked { get; set; }
	public int CellsWide { get; set; }
	public int CellsHigh { get; set; }

    readonly Random Rnd;

	public void DrawMaze(ProgressBar prog)
	{
		int lastx = CellsWide - 1;
		for(int y = 0; y < CellsHigh; y++) {
			int start = 0;
			for(int x = 0; x < CellsWide; x++) {
				if (y > 0 && (x == lastx || Rnd.RandomChoice())) {
					int upx = Rnd.Next(start,x+1);
					DrawCell(upx,y,PickWall.N);
					DrawCell(upx,y-1,PickWall.None);
					start = x + 1;
				}
				else if (x < lastx) {
					DrawCell(x,y,PickWall.E);
					DrawCell(x+1,y,PickWall.None);
				}
			}
			prog.Report(y / (double)CellsHigh);
		}
	}
}
