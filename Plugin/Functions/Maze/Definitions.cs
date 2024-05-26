using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Maze;

public interface IBasicMaze
{
	void DrawMaze(ProgressBar prog);
}

public interface IMaze : IBasicMaze
{
	Action<int, int, PickWall> DrawCell { get; set; }
	Func<int, int, PickWall, bool> IsBlocked { get; set; }
	int CellsWide { get; set; }
	int CellsHigh { get; set; }
}

[Flags]
public enum PickWall
{
	None = 0, N = 1, E = 2, S = 4, W = 8, All = 15
}
