using System;
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

		Random Rnd;
		PickWall[] Walls = new PickWall[] { PickWall.N, PickWall.W, PickWall.S, PickWall.E };
		int[] Parent = null;
		int MaxDepth = 0;

		public void DrawMaze(ProgressBar prog)
		{
			Rnd = O.RndSeed.HasValue ? new Random(O.RndSeed.Value) : new Random();
			int len = CellsHigh * CellsWide;
			MaxDepth = len;
			Parent = new int[len];
			DrawCell(0,0,PickWall.None);

			for(int e = 0; e < len; e++) {
				//instead of connecting all cells with every edge, connect just one edge
				// then we can see if picking a random edge keeps the graph connected
				Parent[e] = e - 1;
			}

			for(int e = len - 1,f = 0; e >= 1; e--, f++) {
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
					int d = ty * CellsWide + tx;
					int p = Parent[e];
					Parent[e] = d;
					//if it's still connected we can stop
					if (IsConnected(e)) {
						DrawCell(x,y,w);
						break;
					}
					else {
						Parent[e] = p; //put back original
					}
				}
				prog.Report(f / (double)len);
			}
		}

		bool IsConnected(int a)
		{
			if (a == -1) { return true; } //are we root ?
			int depth = MaxDepth;
			int r = Parent[a]; //follow parent pointer
			while(--depth >=0 ) {
				if (r == -1) { return true; }
				if (r == a) { return false; } //we went in a loop
				r = Parent[r];
			}
			return false;
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
