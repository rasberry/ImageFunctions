using System;
using System.Collections.Generic;
using System.Drawing;
using ImageFunctions.Helpers;

namespace ImageFunctions.Maze
{
	// This is similar to Kruskal
	// https://en.wikipedia.org/wiki/Reverse-delete_algorithm
	public class ReverseDelete : IMaze
	{
		public Options O { get; set; }
		public Action<int,int,PickWall> DrawCell { get; set; }
		public Func<int,int,PickWall,bool> IsBlocked { get; set; }
		public int CellsWide { get; set; }
		public int CellsHigh { get; set; }

		class Cell
		{
			public Cell(int maxDepth) {
				MaxDepth = maxDepth;
			}

			int MaxDepth = -1;
			public Cell Parent = null;

			public Cell Root { get {
				// a little bit of recursive magick
				//return Parent != null ? Parent.Root : this;
				if (Parent == null) { return this; }
				int depth = MaxDepth;
				Cell p = Parent;
				Cell r = Parent;
				while (depth >= 0) {
					if (r == null) { return p; }
					(p,r) = (r,p.Parent);
					depth--;
				}
				return null;
			}}

			public bool IsConnectedWith(Cell c) {
				return Root == c.Root;
			}

			public void ConnectWith(Cell c) {
				this.Parent = c.Root;
			}
		}

		Random Rnd;
		PickWall[] Walls = new PickWall[] { PickWall.N, PickWall.W, PickWall.S, PickWall.E };

		public void DrawMaze(ProgressBar prog)
		{
			Rnd = O.RndSeed.HasValue ? new Random(O.RndSeed.Value) : new Random();
			int len = CellsHigh * CellsWide;
			//int maxDepth = (int)Math.Ceiling(Math.Log(len,2));

			//init structures
			var Edges = new List<Cell>(len);
			//note: using pickwall as a bit field to store 4 edges per cell
			Cell root = new Cell(len);
			Edges.Add(root);
			for(int e = 1; e < len; e++) {
				var cell = new Cell(len);
				Edges.Add(cell);
				//instead of connecting all cells with every edge, connect just one
				// then we can see if picking a random edge keeps the graph connected
				Edges[e].ConnectWith(Edges[e-1]);
			}

			for(int e = len - 1,f = 0; e >= 1; e--, f++) {
				var c = Edges[e];
				int x = e % CellsWide;
				int y = e / CellsWide;

				Shuffle(Walls);
				foreach(PickWall w in Walls) {
					int tx = -1, ty = -1;
					switch(w) {
						case PickWall.N: tx = x; ty = y-1; break;
						case PickWall.E: tx = x+1; ty = y; break;
						case PickWall.S: tx = x; ty = y+1; break;
						case PickWall.W: tx = x-1; ty = y; break;
					}
					if (tx < 0 || tx >= CellsWide || ty < 0 || ty >= CellsHigh) {
						continue;
					}
					// change parent
					Cell d = Edges[ty * CellsWide + tx];
					Cell p = c.Parent;
					c.Parent = d;
					//if it's still connected we can stop
					if (c.IsConnectedWith(root)) {
						DrawCell(x,y,w);
						break;
					}
					else {
						c.Parent = p; //put back original
					}
				}
				prog.Report(f / (double)len);
			}
		}

		void Shuffle<T>(T[] array)
		{
			for(int i = array.Length - 1; i >= 0; i--) {
				int n = Rnd.Next(0,array.Length);
				(array[i],array[n]) = (array[n],array[i]); //swap
			}
		}
	}
}
