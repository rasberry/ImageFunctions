using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Maze;

public sealed class Options : IOptions
{
	public ColorRGBA CellColor;
	public ColorRGBA WallColor;
	public int? RndSeed;
	public PickMaze Which;
	public IReadOnlyList<PickNext> Sequence;
	public bool SequenceRandomPick;

	public const int DefaultWidth = 1024;
	public const int DefaultHeight = 1024;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1, "Draws one of several mazes");
		sb.ND(1, "-m  (maze)", "Choose a maze (default prims)");
		sb.ND(1, "-cc (color)", "Change cell color (default black)");
		sb.ND(1, "-wc (color)", "Change wall color (default white)");
		sb.ND(1, "-rs (number)", "Random Int32 seed value (defaults to system picked)");
		sb.ND(1, "-sq (s,s,...)", "Growing Tree cell picking sequence (default 'N')");
		sb.ND(1, "-sr", "Randomly pick between sequence options");
		sb.WT();
		sb.ND(1, "Available Mazes:");
		sb.PrintEnum<PickMaze>(1, MazeDesc, excludeZero: true);
		sb.WT();
		sb.ND(1, "Available Sequence Options: (Only for Growing Tree)");
		sb.PrintEnum<PickNext>(1, SeqDesc, SeqName, excludeZero: true);
	}

	static string MazeDesc(PickMaze maze)
	{
		switch(maze) {
		case PickMaze.Automata: return "Cellular automata maze";
		case PickMaze.BinaryTree: return "Binary tree maze algorithm";
		case PickMaze.Division: return "Recursize division algorithm";
		case PickMaze.Eller: return "Eller's algorithm";
		case PickMaze.GrowingTree: return "Growing tree maze algorithm";
		case PickMaze.Kruskal: return "Kruskal's algorithm 🐢";
		case PickMaze.Prims: return "Prim's (Jarník's) algorithm";
		case PickMaze.ReverseDelete: return "Reverse delete algorithm 🐢";
		case PickMaze.SideWinder: return "Sidewinder maze algorithm";
		case PickMaze.Spiral: return "Experimental maze using a spiral layout";
		}
		return "";
	}

	static string SeqDesc(PickNext next)
	{
		switch(next) {
		case PickNext.Middle: return "Pick the middle cell of the current path";
		case PickNext.Newest: return "Pick the most recent visited cell (recursive backtracker)";
		case PickNext.Oldest: return "Pick the lest recent visited cell";
		case PickNext.Random: return "Pick a random cell in the current path (Prim's)";
		}
		return "";
	}

	static string SeqName(PickNext next)
	{
		switch(next) {
		case PickNext.Middle: return "(M)iddle";
		case PickNext.Newest: return "(N)ewest";
		case PickNext.Oldest: return "(O)ldest";
		case PickNext.Random: return "(R)Random";
		}
		return "";
	}

	static IReadOnlyList<PickNext> DefaultSeq()
	{
		return new List<PickNext> { PickNext.Newest };
	}

	static IReadOnlyList<PickNext> SeqParser(string arg)
	{
		var parser = new ParseParams.Parser<PickNext>((string s) => {
			return ExtraParsers.ParseEnumFirstLetter<PickNext>(s);
		});
		//need to provide delimiters
		return ExtraParsers.ParseSequence(arg, new char[] { ',' }, parser);
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var colorParser = new ParseParams.Parser<ColorRGBA>(PlugTools.ParseColor);

		if(p.Scan("-m", PickMaze.Prims)
			.WhenGoodOrMissing(r => { Which = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-cc", PlugColors.Black, colorParser)
			.WhenGoodOrMissing(r => { CellColor = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-wc", PlugColors.White, colorParser)
			.WhenGoodOrMissing(r => { WallColor = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<int>("-rs")
			.WhenGood(r => { RndSeed = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-sq", DefaultSeq(), SeqParser)
			.WhenGoodOrMissing(r => { Sequence = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Has("-sr").IsGood()) {
			SequenceRandomPick = true;
		}

		return true;
	}
}

public enum PickMaze
{
	None = 0,
	Eller = 1,
	Prims = 2,
	Kruskal = 3,
	BinaryTree = 4,
	GrowingTree = 5,
	Automata = 6,
	Spiral = 7,
	ReverseDelete = 8,
	SideWinder = 9,
	Division = 10
}

public enum PickNext
{
	None = 0,
	Newest = 1,
	Oldest = 2,
	Middle = 3,
	Random = 4
}
