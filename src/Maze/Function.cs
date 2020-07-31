using System;
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
			sb.WL();
			sb.WL(1,"Available Mazes:");
			sb.PrintEnum<PickMaze>(1,MazeDesc);
		}

		static string MazeDesc(PickMaze maze)
		{
			switch(maze) {
			case PickMaze.Eller: return "Eller's Algorithm";
			case PickMaze.Prims: return "Prim's (Jarník's) Algorithm";
			case PickMaze.Kruskal: return "Kruskal's algorithm";
			}
			return "";
		}

		protected override AbstractProcessor CreateProcessor()
		{
			return new Processor { O = O };
		}

		Options O = new Options();
	}
}