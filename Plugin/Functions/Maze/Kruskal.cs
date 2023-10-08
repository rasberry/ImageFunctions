using System.Drawing;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Maze;

// http://weblog.jamisbuck.org/2011/1/3/maze-generation-kruskal-s-algorithm

public class Kruskal : IMaze
{
	public Kruskal(Options o) {
		Rnd = o.RndSeed.HasValue ? new Random(o.RndSeed.Value) : new Random();
	}

	public Action<int,int,PickWall> DrawCell { get; set; }
	public Func<int,int,PickWall,bool> IsBlocked { get; set; }
	public int CellsWide { get; set; }
	public int CellsHigh { get; set; }

	readonly Random Rnd;

	public void DrawMaze(ProgressBar prog)
	{
		int len = CellsHigh * CellsWide;

		//init structures
		//note: using pickwall as a bit field to store 4 edges per cell
		var Edges = new List<Cell>(len);
		var Sets = new Dictionary<Point,Cell>();
		for(int c=0; c<len; c++) {
			int x = c % CellsWide;
			int y = c / CellsWide;
			var cell = new Cell(x,y,PickWall.All);
			Edges.Add(cell);
			Sets.Add(cell.P,cell);
		}

		while(Edges.Count > 0)
		{
			//Log.Debug($"Edges Left {Edges.Count}");
			var (node,pick) = RemoveRandomEdge(Edges);
			Point? next = null;
			switch(pick) {
				case PickWall.N: next = new Point(node.P.X,node.P.Y-1); break;
				case PickWall.E: next = new Point(node.P.X+1,node.P.Y); break;
				case PickWall.S: next = new Point(node.P.X,node.P.Y+1); break;
				case PickWall.W: next = new Point(node.P.X-1,node.P.Y); break;
			}
			prog.Report((len - Edges.Count) / (double)len);

			//didn't find an edge
			if (!next.HasValue) {
				//Log.Debug($"didn't find an edge {node} -- {next}");
				continue;
			}
			//skip out of range
			Point p = next.Value;
			if (p.X < 0 || p.X >= CellsWide || p.Y < 0 || p.Y >= CellsHigh) {
				//Log.Debug($"out of bounds {node} -- {next}");
				continue;
			}
			// draw and connect
			var dest = Sets[next.Value];
			if (!node.IsConnectedWith(dest)) {
				node.ConnectWith(dest);
				DrawCell(node.P.X, node.P.Y, pick);
				DrawCell(dest.P.X, dest.P.Y, PickWall.None);
				//Log.Debug($"Draw {node} + {dest} => {pick}");
			}
			else {
				//Log.Debug($"Already conneced {node} + {dest}");
			}
		}
	}

	(Cell,PickWall) RemoveRandomEdge(List<Cell> list)
	{
		//pick a random cell
		int len = list.Count;
		int val = Rnd.Next(0,len);
		(list[val],list[len-1]) = (list[len-1],list[val]); //swap
		Cell item = list[len-1];
		PickWall w = item.W;

		if (w == PickWall.None) {
			//TODO something is very wrong if we are here
			//Log.Debug("ERROR wall was none");
		}

		//pick an existing wall on that cell
		var pickList = new List<PickWall>();
		if (w.HasFlag(PickWall.N)) { pickList.Add(PickWall.N); }
		if (w.HasFlag(PickWall.E)) { pickList.Add(PickWall.E); }
		if (w.HasFlag(PickWall.S)) { pickList.Add(PickWall.S); }
		if (w.HasFlag(PickWall.W)) { pickList.Add(PickWall.W); }

		PickWall pick;
		//if there's only 1 wall left remove the cell
		//Log.Debug($"picklist [{String.Join(',',pickList)}]");
		if (pickList.Count < 2) {
			//Log.Debug($"removed {item}");
			pick = pickList[0];
			list.RemoveAt(len-1);
		}
		// pick a random wall to remove
		else {
			pick = RemoveRandom(pickList);
			item.W &= ~pick & PickWall.All;
			//Log.Debug($"cleared {item} => {pick}");
		}
		return (item,pick);
	}

	T RemoveRandom<T>(IList<T> list)
	{
		int len = list.Count;
		int val = Rnd.Next(0,len);
		(list[val],list[len-1]) = (list[len-1],list[val]); //swap
		T item = list[len-1];
		list.RemoveAt(len-1);
		return item;
	}

	class Cell
	{
		public Cell(int x,int y,PickWall w)
		{
			P = new Point(x,y);
			W = w;
		}

		public Point P;
		public PickWall W;
		Cell Parent = null;

		public Cell Root { get {
			// a little bit of recursive magick
			return Parent != null ? Parent.Root : this;
		}}

		public bool IsConnectedWith(Cell c) {
			return Root == c.Root;
		}

		public void ConnectWith(Cell c) {
			c.Root.Parent = this;
		}

		public override string ToString() {
			return $"[{P.X},{P.Y}] {W} ({(Parent == null ? "" : "P")})";
		}
	}
}
