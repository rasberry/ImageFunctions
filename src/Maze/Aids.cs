using System;

namespace ImageFunctions.Maze
{
	public static class Aids
	{
		public static PickWall Opposite(this PickWall w)
		{
			switch(w) {
				case PickWall.N: return PickWall.S;
				case PickWall.E: return PickWall.W;
				case PickWall.S: return PickWall.N;
				case PickWall.W: return PickWall.W;
			}
			return PickWall.None;
		}
	}
}
