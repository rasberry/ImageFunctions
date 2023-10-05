using System;
using System.Collections.Generic;
using System.Drawing;
using ImageFunctions.Helpers;

namespace ImageFunctions.Maze
{
	public class GrowingTree : IMaze
	{
		public Options O { get; set; }
		public Action<int,int,PickWall> DrawCell { get; set; }
		public Func<int,int,PickWall,bool> IsBlocked { get; set; }
		public int CellsWide { get; set; }
		public int CellsHigh { get; set; }

		public IReadOnlyList<PickNext> Sequence { get; set; }

		Random Rnd = null;
		List<Point> CellList = new List<Point>();
		//IList<Point> CellList = new HeapList<Point>();

		public void DrawMaze(ProgressBar prog)
		{
			//Log.Debug($"Seq [{String.Join(',',Sequence)}]");
			Rnd = O.RndSeed.HasValue ? new Random(O.RndSeed.Value) : new Random();
			var have = new List<PickWall>();
			int W = CellsWide - 1;
			int H = CellsHigh - 1;
			double total = (double)CellsWide * CellsHigh;
			double count = 0.0;
			int SeqNum = 0;
			
			//pick random start
			var first = new Point(Rnd.Next(0,CellsWide),Rnd.Next(0,CellsHigh));
			CellList.Add(first);
			DrawCell(first.X,first.Y,PickWall.None);

			while(CellList.Count > 0) {
				prog.Report(count / total);
				int last = CellList.Count - 1;

				//pick next
				PickNext pn;
				if (O.SequenceRandomPick) {
					int r = Rnd.Next(0,Sequence.Count);
					pn = Sequence[r];
				}
				else {
					pn = Sequence[SeqNum];
					SeqNum++; SeqNum %= Sequence.Count; //increment and roll

				}
				int pickIndex;
				switch(pn) {
					default:
					case PickNext.Newest: pickIndex = last; break;
					case PickNext.Oldest: pickIndex = 0; break;
					case PickNext.Middle: pickIndex = CellList.Count / 2; break;
					case PickNext.Random: pickIndex = Rnd.Next(0,CellList.Count); break;
				}
				Point p = CellList[pickIndex];

				//are there any unvisited neighbors ?
				have.Clear();
				if (p.Y > 0 && IsBlocked(p.X,p.Y-1,PickWall.None)) { have.Add(PickWall.N); }
				if (p.X < W && IsBlocked(p.X+1,p.Y,PickWall.None)) { have.Add(PickWall.E); }
				if (p.Y < H && IsBlocked(p.X,p.Y+1,PickWall.None)) { have.Add(PickWall.S); }
				if (p.X > 0 && IsBlocked(p.X-1,p.Y,PickWall.None)) { have.Add(PickWall.W); }

				// remove from list if all are visited
				if (have.Count < 1) {
					CellList.RemoveAt(pickIndex);
					//swap last and picked
					//(CellList[last],CellList[pickIndex]) = (CellList[pickIndex],CellList[last]);
					//so we can remove last (which is O(1) operation)
					//CellList.RemoveAt(last);
					continue;
				}

				// pick a direction
				int index = Rnd.Next(0,have.Count);
				PickWall pick = have[index];

				Point n = Point.Empty;
				switch(pick) {
				case PickWall.N: n = new Point(p.X,p.Y-1); break;
				case PickWall.E: n = new Point(p.X+1,p.Y); break;
				case PickWall.S: n = new Point(p.X,p.Y+1); break;
				case PickWall.W: n = new Point(p.X-1,p.Y); break;
				}

				//add next one to list
				DrawCell(n.X,n.Y,Aids.Opposite(pick));
				CellList.Add(n);
				count += 1.0;
			}
		}

	}
}
