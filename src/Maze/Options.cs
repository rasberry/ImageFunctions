using System;
using System.Collections.Generic;

namespace ImageFunctions.Maze
{
	public enum PickMaze
	{
		None = 0,
		Eller = 1,
		Prims = 2,
		Kruskal = 3,
		BinaryTree = 4,
		GrowingTree = 5
	}

	public enum PickNext
	{
		None = 0,
		Newest = 1,
		Oldest = 2,
		Middle = 3,
		Random = 4
	}

	public class Options
	{
		public IColor CellColor;
		public IColor WallColor;
		public int? RndSeed = null;
		public PickMaze Which = PickMaze.None;
		public IReadOnlyList<PickNext> Sequence = null;
		public bool SequenceRandomPick = false;
	}
}