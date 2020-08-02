using System;
using System.Collections.Generic;
using System.Drawing;
using ImageFunctions.Helpers;

namespace ImageFunctions.Maze
{
	public class GrowingTree : IMaze
	{
		public Options O { get; set; }
		public Action<int,int,PickWall> DrawCell { get; set; }
		public Func<int,int,PickWall,bool> IsBlocked { get; set; }
		public int CellsWide { get; set; }
		public int CellsHigh { get; set; }

		Random Rnd = null;
		List<Point> CellList = new List<Point>();

		public void DrawMaze(ProgressBar prog)
		{
			Rnd = O.RndSeed.HasValue ? new Random(O.RndSeed.Value) : new Random();
			var have = new List<PickWall>();
			int W = CellsWide - 1;
			int H = CellsHigh - 1;
			
			var first = new Point(Rnd.Next(0,CellsWide),Rnd.Next(0,CellsHigh));
			CellList.Add(first);
			DrawCell(first.X,first.Y,PickWall.None);
			//Log.Debug($"Draw {first}");

			while(CellList.Count > 0) {
				//Log.Debug($"List={CellList.Count} [{String.Join(',',CellList)}]");
				int last = CellList.Count - 1;
				Point p = CellList[last];
				//Log.Debug($"next P = {p}");

				have.Clear();
				if (p.Y > 0 && IsBlocked(p.X,p.Y-1,PickWall.None)) { have.Add(PickWall.N); }
				if (p.X < W && IsBlocked(p.X+1,p.Y,PickWall.None)) { have.Add(PickWall.E); }
				if (p.Y < H && IsBlocked(p.X,p.Y+1,PickWall.None)) { have.Add(PickWall.S); }
				if (p.X > 0 && IsBlocked(p.X-1,p.Y,PickWall.None)) { have.Add(PickWall.W); }
				//Log.Debug($"Have [{String.Join(',',have)}]");

				if (have.Count < 1) {
					//Log.Debug($"remove {p}");
					CellList.RemoveAt(last);
					continue;
				}

				int index = Rnd.Next(0,have.Count);
				PickWall pick = have[index];

				Point n = Point.Empty;
				switch(pick) {
				case PickWall.N: n = new Point(p.X,p.Y-1); break;
				case PickWall.E: n = new Point(p.X+1,p.Y); break;
				case PickWall.S: n = new Point(p.X,p.Y+1); break;
				case PickWall.W: n = new Point(p.X-1,p.Y); break;
				}

				//Log.Debug($"Draw {n} {Aids.Opposite(pick)}");
				DrawCell(n.X,n.Y,Aids.Opposite(pick));
				CellList.Add(n);
				//Log.Debug("\n"+Aids.MazeToString(this));
			}
		}

	}
}
