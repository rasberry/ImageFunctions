using System;
using ImageFunctions.Helpers;

namespace ImageFunctions.Maze
{
	public interface IMaze {
		Options O { get; set; }
		void DrawMaze(ProgressBar prog);
		Action<int,int,PickWall> DrawCell { get; set; }
		Func<int,int,PickWall,bool> IsBlocked { get; set; }
		int CellsWide { get; set; }
		int CellsHigh { get; set; }
	}

	[Flags]
	public enum PickWall {
		None = 0, N = 1, E = 2, S = 4, W = 8, All = 15
	}

	public class Processor : AbstractProcessor
	{
		public Options O = null;

		//TODO implement using Bounds
		public override void Apply()
		{
			IMaze maze = null;
			switch(O.Which) {
			case PickMaze.Eller: maze = new Ellers(); break;
			case PickMaze.Prims: maze = new Prims(); break;
			case PickMaze.Kruskal: maze = new Kruskal(); break;
			}
			//Log.Debug("maze :"+O.Which);

			maze.CellsWide = Source.Width / 2;
			maze.CellsHigh = Source.Width / 2;
			maze.O = O;
			maze.DrawCell = DrawCell;
			maze.IsBlocked = IsBlocked;

			ImageHelpers.FillWithColor(Source,O.WallColor);
			using (var progress = new ProgressBar()) {
				maze.DrawMaze(progress);
			}
		}

		public override void Dispose() {}

		void DrawCell(int cx,int cy, PickWall removeWalls)
		{
			var img = Source;
			int x = cx * 2 + 1;
			int y = cy * 2 + 1;
			if (x < 0 || x >= img.Width || y < 0 || y >= img.Height) {
				return;
			}
			img[x,y] = O.CellColor;
			//Log.Debug($"Draw ({cx},{cy}) {removeWalls} [{x},{y}]");
			if (removeWalls.HasFlag(PickWall.N) && y > 0) {
				img[x,y-1] = O.CellColor;
				//Log.Debug($"Draw [{x},{y-1}]");
			}
			if (removeWalls.HasFlag(PickWall.S) && y < img.Height - 1) {
				img[x,y+1] = O.CellColor;
				//Log.Debug($"Draw [{x},{y+1}]");
			}
			if (removeWalls.HasFlag(PickWall.W) && x > 0) {
				img[x-1,y] = O.CellColor;
				//Log.Debug($"Draw [{x-1},{y}]");
			}
			if (removeWalls.HasFlag(PickWall.E) && x < img.Width - 1) {
				img[x+1,y] = O.CellColor;
				//Log.Debug($"Draw [{x+1},{y}]");
			}
		}

		bool IsBlocked(int cx,int cy, PickWall which)
		{
			var img = Source;
			int x = cx * 2 + 1;
			int y = cy * 2 + 1;
			if (x < 0 || x >= img.Width || y < 0 || y >= img.Height) {
				return false;
			}

			switch(which) {
				case PickWall.None: return img[x,y].Equals(O.WallColor);
				case PickWall.N: return y > 0              && img[x,y-1].Equals(O.WallColor);
				case PickWall.E: return x < img.Width - 1  && img[x+1,y].Equals(O.WallColor);
				case PickWall.S: return y < img.Height - 1 && img[x,y+1].Equals(O.WallColor);
				case PickWall.W: return x > 0              && img[x-1,y].Equals(O.WallColor);
			}

			return false;
		}

	}
}
