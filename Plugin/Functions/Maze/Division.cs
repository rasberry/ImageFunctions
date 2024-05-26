using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.Maze;

// http://weblog.jamisbuck.org/2011/1/12/maze-generation-recursive-division-algorithm.html
// https://gist.github.com/josiahcarlson/904686

public class Division : IMaze
{
	public Division(Options o)
	{
		Rnd = o.RndSeed.HasValue ? new Random(o.RndSeed.Value) : new Random();
	}

	public Action<int, int, PickWall> DrawCell { get; set; }
	public Func<int, int, PickWall, bool> IsBlocked { get; set; }
	public int CellsWide { get; set; }
	public int CellsHigh { get; set; }

	readonly Random Rnd;
	enum PickHV { H = 0, V = 1 }

	public void DrawMaze(ProgressBar prog)
	{
		var stack = new Stack<Rectangle>();
		stack.Push(new Rectangle(0, 0, CellsWide, CellsHigh));
		double total = CellsHigh * (double)CellsWide;
		int count = 0;

		while(stack.Count > 0) {
			prog.Report(count / total);
			var rect = stack.Pop();
			int dx = rect.Width - rect.X;
			int dy = rect.Height - rect.Y;

			if(dx < 2 || dy < 2) {
				// make a hallway
				if(dx > 1) {
					int y = rect.Y;
					for(int x = rect.X; x < rect.Width - 1; x++) {
						DrawCell(x, y, PickWall.E);
						DrawCell(x + 1, y, PickWall.None);
						count++;
					}
				}
				else if(dy > 1) {
					int x = rect.X;
					for(int y = rect.Y; y < rect.Height - 1; y++) {
						DrawCell(x, y, PickWall.S);
						DrawCell(x, y + 1, PickWall.None);
						count++;
					}
				}
			}
			else {
				PickHV wall;
				if(dy > dx) { wall = PickHV.H; }
				else if(dx > dy) { wall = PickHV.V; }
				else { wall = Rnd.RandomChoice() ? PickHV.H : PickHV.V; }

				int x = Rnd.Next(rect.X, rect.Width - (wall == PickHV.V ? 1 : 0));
				int y = Rnd.Next(rect.Y, rect.Height - (wall == PickHV.H ? 1 : 0));

				if(wall == PickHV.H) {
					DrawCell(x, y, PickWall.S);
					DrawCell(x, y + 1, PickWall.None);
					count++;

					stack.Push(new Rectangle(rect.X, rect.Y, rect.Width, y + 1));
					stack.Push(new Rectangle(rect.X, y + 1, rect.Width, rect.Height));
				}
				else {
					DrawCell(x, y, PickWall.E);
					DrawCell(x + 1, y, PickWall.None);
					count++;

					stack.Push(new Rectangle(rect.X, rect.Y, x + 1, rect.Height));
					stack.Push(new Rectangle(x + 1, rect.Y, rect.Width, rect.Height));
				}
			}
		}
	}
}
