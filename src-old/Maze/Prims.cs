using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.Maze
{
	public class Prims : IMaze
	{
		public Options O { get; set; }
		public Action<int,int,PickWall> DrawCell { get; set; }
		public Func<int,int,PickWall,bool> IsBlocked { get; set; }
		public int CellsWide { get; set; }
		public int CellsHigh { get; set; }

		struct Cell
		{
			public Cell(int x,int y,PickWall w) {
				P = new Point(x,y);
				W = w;
			}
			public Point P;
			public PickWall W;

			public override string ToString() {
				return $"[{P.X},{P.Y}] {W}";
			}
		}

		Random Rnd = null;
		List<Cell> Walls = new List<Cell>();

		public void DrawMaze(ProgressBar prog)
		{
			Rnd = O.RndSeed.HasValue ? new Random(O.RndSeed.Value) : new Random();

			Point first = FindCell();
			AddWallsForCell(first.X,first.Y);
			DrawCell(first.X,first.Y,PickWall.None);
			
			//Log.Debug($"size {CellsWide},{CellsHigh}");
			int TCells = CellsWide * CellsHigh;
			int CellCount = 0;

			while(Walls.Count > 0) {
				//PrintQueue();
				Cell c = RemoveRandomWall();
				//Log.Debug($"RW = {c}");
				if (MarkCellsForWall(c)) {
					CellCount++;
					// prog.Prefix = Walls.Count + " ";
					prog.Report((double)CellCount / TCells);
				}
			}
		}

		void PrintQueue()
		{
			var sb = new StringBuilder();
			int i = 0;
			foreach(var c in Walls) {
				sb.Append($" #{i++} {c}");
			}
			Log.Debug(sb.ToString());
		}

		Point FindCell()
		{
			return new Point(
				Rnd.Next(0,CellsWide),
				Rnd.Next(0,CellsHigh)
			);
		}

		void AddWallsForCell(int x,int y)
		{
			bool isWall = IsBlocked(x,y,PickWall.None);
			if (!isWall) { return; }
			int h = CellsHigh;
			int w = CellsWide;

			if (y > 0) { Walls.Add(new Cell(x,y,PickWall.N)); }
			if (y < h) { Walls.Add(new Cell(x,y,PickWall.S)); }
			if (x > 0) { Walls.Add(new Cell(x,y,PickWall.W)); }
			if (x < w) { Walls.Add(new Cell(x,y,PickWall.E)); }
		}

		Cell RemoveRandomWall()
		{
			//doing a switch and remove so we don't have to shift everything after the chosen wall O(n)
			int len = Walls.Count;
			int which = Rnd.Next(0,len);
			Cell c = Walls[which];
			Cell e = Walls[len - 1];
			Walls[which] = e; //switch
			Walls.RemoveAt(len - 1); //remove last element (cheap O(1))
			return c;
		}

		bool MarkCellsForWall(Cell c)
		{
			Cell q = default(Cell); bool wasSet = true;
			switch(c.W) {
				case PickWall.N: q = new Cell(c.P.X,c.P.Y-1,PickWall.None); break;
				case PickWall.E: q = new Cell(c.P.X+1,c.P.Y,PickWall.None); break;
				case PickWall.S: q = new Cell(c.P.X,c.P.Y+1,PickWall.None); break;
				case PickWall.W: q = new Cell(c.P.X-1,c.P.Y,PickWall.None); break;
				default: wasSet = false; break; //q was not set
			}
			//return if out of bounds or if target cell is already open
			if (!wasSet || !IsBlocked(q.P.X,q.P.Y,q.W)) {
				//Log.Debug($"Skipped {q}");
				return false;
			}

			//Log.Debug($"Hole {c} {q} [{c.W}]");
			AddWallsForCell(q.P.X,q.P.Y);
			//we're drawing only the destination cell so the wall is opposite from that perspective
			DrawCell(q.P.X, q.P.Y, Aids.Opposite(c.W));
			return true;
		}
	}
}