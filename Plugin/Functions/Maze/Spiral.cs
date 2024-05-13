using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Maze;

public class Spiral : IMaze
{
	public Spiral(Options o) {
		Rnd = o.RndSeed.HasValue ? new Random(o.RndSeed.Value) : new Random();
	}

	public Action<int,int,PickWall> DrawCell { get; set; }
	public Func<int,int,PickWall,bool> IsBlocked { get; set; }
	public int CellsWide { get; set; }
	public int CellsHigh { get; set; }

	readonly Random Rnd;

	public void DrawMaze(ProgressBar prog)
	{
		int len = CellsHigh * CellsWide;
		int cx = CellsWide / 2;
		int cy = CellsHigh / 2;

		for(int c=0; c<len; c++) {
			prog.Report((double)c / len);
			var (x,y) = PlugTools.SpiralSquareToXY(c,cx,cy);
			//if (!IsBlocked(x,y,PickWall.None)) { continue; }

			var pw = PickNeighbor(x,y);
			if (pw == PickWall.None) { continue; }

			int nx = x, ny = y;
			switch(pw) {
			case PickWall.N: nx = x + 0; ny = y - 1; break;
			case PickWall.E: nx = x + 1; ny = y + 0; break;
			case PickWall.S: nx = x + 0; ny = y + 1; break;
			case PickWall.W: nx = x - 1; ny = y + 0; break;
			}
			//Log.Debug($"DC [{x},{y}] {pw} [{nx},{ny}]");
			DrawCell(x,y,pw);
			DrawCell(nx,ny,PickWall.None);
		}

	}

	PickWall PickNeighbor(int x,int y)
	{
		var list = new List<PickWall>();
		if (IsBlocked(x,y-1,PickWall.None)) { list.Add(PickWall.N); }
		if (IsBlocked(x+1,y,PickWall.None)) { list.Add(PickWall.E); }
		if (IsBlocked(x,y+1,PickWall.None)) { list.Add(PickWall.S); }
		if (IsBlocked(x-1,y,PickWall.None)) { list.Add(PickWall.W); }

		if (list.Count < 1) {
			return PickWall.None;
		}
		//add perpendicular ways if there's only one choice
		else if (list.Count == 1) {
			PickWall w = list[0];
			switch(w) {
				case PickWall.N: list.Add(PickWall.E); list.Add(PickWall.W); break;
				case PickWall.E: list.Add(PickWall.N); list.Add(PickWall.S); break;
				case PickWall.S: list.Add(PickWall.W); list.Add(PickWall.E); break;
				case PickWall.W: list.Add(PickWall.S); list.Add(PickWall.N); break;
			}
		}

		int i = Rnd.Next(0,list.Count);
		return list[i];
	}
}
