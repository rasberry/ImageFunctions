using System;

namespace ImageFunctions.Maze
{
	public enum PickMaze
	{
		None = 0,
		Eller = 1
	}

	public class Options
	{
		public IColor CellColor;
		public IColor WallColor;
	}
}