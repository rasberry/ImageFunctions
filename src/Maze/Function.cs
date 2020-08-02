using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.Maze
{
	public class Function : AbstractFunction, IGenerator
	{
		public Size StartingSize { get {
			return new Size(1024,1024);
		}}

		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.Default("-cc",out O.CellColor,ColorHelpers.Black).IsInvalid()) {
				return false;
			}
			if (p.Default("-wc",out O.WallColor,ColorHelpers.White).IsInvalid()) {
				return false;
			}
			if (p.Default("-rs",out O.RndSeed, null).IsInvalid()) {
				return false;
			}
			if (p.Default("-sq",out O.Sequence, DefaultSeq(), SeqParser).IsInvalid()) {
				return false;
			}
			if (p.Has("-sr").IsGood()) {
				O.SequenceRandomPick = true;
			}

			if (p.Expect(out O.Which,"maze").IsBad()) {
				return false;
			}
			if (p.DefaultFile(out OutImage,nameof(Maze)).IsBad()) {
				return false;
			}
			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.Maze);
			sb.WL();
			sb.WL(0,name + "(maze) [options] [output image]");
			sb.WL(1,"Draw one of several mazes");
			sb.WL(1,"-cc (color)"    ,"Change cell color (default black)");
			sb.WL(1,"-wc (color)"    ,"Change wall color (default white)");
			sb.WL(1,"-rs (number)"   ,"Random Int32 seed value (defaults to system picked)");
			sb.WL(1,"-sq (s,s,...)"  ,"Growing Tree cell picking sequence (default 'N')");
			sb.WL(1,"-sr"            ,"Randomly pick between sequence options");
			sb.WL();
			sb.WL(1,"Available Mazes:");
			sb.PrintEnum<PickMaze>(1,MazeDesc);
			sb.WL();
			sb.WL(1,"Available Sequence Options: (Only for Growing Tree)");
			sb.PrintEnum<PickNext>(1,SeqDesc,SeqName);
		}

		static string MazeDesc(PickMaze maze)
		{
			switch(maze) {
			case PickMaze.Eller: return "Eller's Algorithm";
			case PickMaze.Prims: return "Prim's (Jarník's) Algorithm";
			case PickMaze.Kruskal: return "Kruskal's algorithm";
			case PickMaze.BinaryTree: return "Binary Tree maze algorithm";
			case PickMaze.GrowingTree: return "Growing Tree maze algorithm";
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

		protected override AbstractProcessor CreateProcessor()
		{
			return new Processor { O = O };
		}

		static IReadOnlyList<PickNext> DefaultSeq()
		{
			return new List<PickNext> { PickNext.Newest };
		}

		static bool SeqParser(string arg, out IReadOnlyList<PickNext> seq)
		{
			//need to provide delimiters
			return OptionsHelpers.TryParseSequence(arg,new char[] {','},
				out seq,OptionsHelpers.TryParseEnumFirstLetter);
		}

		Options O = new Options();

	}
}