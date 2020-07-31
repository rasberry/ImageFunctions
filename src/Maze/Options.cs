using System;

namespace ImageFunctions.Maze
{
	public enum PickMaze
	{
		None = 0,
		Eller = 1,
		Prims = 2,
		Kruskal = 3
	}

	public class Options
	{
		public IColor CellColor;
		public IColor WallColor;
		public int? RndSeed = null;
		public PickMaze Which = PickMaze.None;
	}
}