using System;
using ImageFunctions.Helpers;

namespace ImageFunctions.Maze
{
	public interface IBasicMaze {
		Options O { get; set; }
		void DrawMaze(ProgressBar prog);
	}

	public interface IMaze : IBasicMaze {
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
			switch(O.Which) {
			case PickMaze.Eller: Maze = new Ellers(); break;
			case PickMaze.Prims: Maze = new Prims(); break;
			case PickMaze.Kruskal: Maze = new Kruskal(); break;
			case PickMaze.BinaryTree: Maze = new BinaryTree(); break;
			case PickMaze.GrowingTree: Maze = new GrowingTree() { Sequence = O.Sequence }; break;
			case PickMaze.Automata: BasicMaze = new Automata { PixelGrid = Source }; break;
			}
			//Log.Debug("maze :"+O.Which);

			if (Maze != null) {
				Maze.CellsWide = Source.Width / 2;
				Maze.CellsHigh = Source.Width / 2;
				Maze.DrawCell = DrawCell;
				Maze.IsBlocked = IsBlocked;
				BasicMaze = Maze;
			}
			BasicMaze.O = O;

			ImageHelpers.FillWithColor(Source,O.WallColor);
			using (var progress = new ProgressBar()) {
				BasicMaze.DrawMaze(progress);
			}
		}

		public override void Dispose() {}

		#if false
		void DrawCell(int cx,int cy, PickWall removeWalls)
		{
			var img = Source;
			var (x,y) = CellToImage(cx,cy);
			
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
		#endif

		void DrawCell(int cx,int cy, PickWall removeWalls)
		{
			var img = Source;
			var (x,y) = CellToImage(cx,cy);
			int mw = Maze.CellsWide;
			int mh = Maze.CellsHigh;
			if (cx < 0 || cx >= mw || cy < 0 || cy >= mh) {
				return;
			}
			img[x,y] = O.CellColor;
			//Log.Debug($"Draw ({cx},{cy}) {removeWalls} [{x},{y}]");
			if (removeWalls.HasFlag(PickWall.N) && cy > 0) {
				img[x,y-1] = O.CellColor;
				//Log.Debug($"Draw [{x},{y-1}]");
			}
			if (removeWalls.HasFlag(PickWall.S) && cy < mh - 1) {
				img[x,y+1] = O.CellColor;
				//Log.Debug($"Draw [{x},{y+1}]");
			}
			if (removeWalls.HasFlag(PickWall.W) && cx > 0) {
				img[x-1,y] = O.CellColor;
				//Log.Debug($"Draw [{x-1},{y}]");
			}
			if (removeWalls.HasFlag(PickWall.E) && cx < mw - 1) {
				img[x+1,y] = O.CellColor;
				//Log.Debug($"Draw [{x+1},{y}]");
			}
		}


		bool IsBlocked(int cx,int cy, PickWall which)
		{
			var img = Source;
			var (x,y) = CellToImage(cx,cy);
			int mw = Maze.CellsWide;
			int mh = Maze.CellsHigh;
			if (cx < 0 || cy < 0 || cx >= mw || cy >= mh) {
				return false;
			}

			switch(which) {
				case PickWall.None: return img[x,y].Equals(O.WallColor);
				case PickWall.N: return cy > 0      && img[x,y-1].Equals(O.WallColor);
				case PickWall.E: return cx < mw - 1 && img[x+1,y].Equals(O.WallColor);
				case PickWall.S: return cy < mh - 1 && img[x,y+1].Equals(O.WallColor);
				case PickWall.W: return cx > 0      && img[x-1,y].Equals(O.WallColor);
			}

			return false;
		}

		(int,int) CellToImage(int cx,int cy)
		{
			return (2 * cx, 2 * cy);
		}
		
		IBasicMaze BasicMaze = null;
		IMaze Maze = null;
	}
}
