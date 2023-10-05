using ImageFunctions.Core;
using Rasberry.Cli;
using O = ImageFunctions.Plugin.Functions.Maze.Options;

namespace ImageFunctions.Plugin.Functions.Maze;

[InternalRegisterFunction(nameof(Maze))]
public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		O.Usage(sb);
	}

	public bool Run(IRegister register, ILayers layers, string[] args)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if (!O.ParseArgs(args, register)) {
			return false;
		}

		canvas = layers.NewCanvasFromLayersOrDefault(O.DefaultWidth, O.DefaultHeight);
		layers.Push(canvas);

		switch(O.Which) {
		case PickMaze.Eller:         Maze = new Ellers(); break;
		case PickMaze.Prims:         Maze = new Prims(); break;
		case PickMaze.Kruskal:       Maze = new Kruskal(); break;
		case PickMaze.BinaryTree:    Maze = new BinaryTree(); break;
		case PickMaze.GrowingTree:   Maze = new GrowingTree() { Sequence = O.Sequence }; break;
		case PickMaze.Spiral:        Maze = new Spiral(); break;
		case PickMaze.ReverseDelete: Maze = new ReverseDelete(); break;
		case PickMaze.SideWinder:    Maze = new SideWinder(); break;
		case PickMaze.Division:      Maze = new Division(); break;
		case PickMaze.Automata: BasicMaze = new Automata { PixelGrid = canvas }; break;
		}
		//Log.Debug("maze :"+O.Which);

		if (Maze != null) {
			Maze.CellsWide = canvas.Width / 2;
			Maze.CellsHigh = canvas.Width / 2;
			Maze.DrawCell = DrawCell;
			Maze.IsBlocked = IsBlocked;
			BasicMaze = Maze;
		}

		PlugTools.FillWithColor(canvas, O.WallColor);
		using var progress = new ProgressBar();
		BasicMaze.DrawMaze(progress);

		return true;
	}

	void DrawCell(int cx,int cy, PickWall removeWalls)
	{
		var (x,y) = CellToImage(cx,cy);
		int mw = Maze.CellsWide;
		int mh = Maze.CellsHigh;
		if (cx < 0 || cx >= mw || cy < 0 || cy >= mh) {
			return;
		}
		canvas[x,y] = O.CellColor;
		//Log.Debug($"Draw ({cx},{cy}) {removeWalls} [{x},{y}]");
		if (removeWalls.HasFlag(PickWall.N) && cy > 0) {
			canvas[x,y-1] = O.CellColor;
			//Log.Debug($"Draw [{x},{y-1}]");
		}
		if (removeWalls.HasFlag(PickWall.S) && cy < mh - 1) {
			canvas[x,y+1] = O.CellColor;
			//Log.Debug($"Draw [{x},{y+1}]");
		}
		if (removeWalls.HasFlag(PickWall.W) && cx > 0) {
			canvas[x-1,y] = O.CellColor;
			//Log.Debug($"Draw [{x-1},{y}]");
		}
		if (removeWalls.HasFlag(PickWall.E) && cx < mw - 1) {
			canvas[x+1,y] = O.CellColor;
			//Log.Debug($"Draw [{x+1},{y}]");
		}
	}

	bool IsBlocked(int cx,int cy, PickWall which)
	{
		var (x,y) = CellToImage(cx,cy);
		int mw = Maze.CellsWide;
		int mh = Maze.CellsHigh;
		if (cx < 0 || cy < 0 || cx >= mw || cy >= mh) {
			return false;
		}

		switch(which) {
			case PickWall.None: return canvas[x,y].Equals(O.WallColor);
			case PickWall.N: return cy > 0      && canvas[x,y-1].Equals(O.WallColor);
			case PickWall.E: return cx < mw - 1 && canvas[x+1,y].Equals(O.WallColor);
			case PickWall.S: return cy < mh - 1 && canvas[x,y+1].Equals(O.WallColor);
			case PickWall.W: return cx > 0      && canvas[x-1,y].Equals(O.WallColor);
		}

		return false;
	}

	(int,int) CellToImage(int cx,int cy)
	{
		return (2 * cx, 2 * cy);
	}

	ICanvas canvas;
	IBasicMaze BasicMaze = null;
	IMaze Maze = null;
}