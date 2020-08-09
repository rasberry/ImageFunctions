using System;
using ImageFunctions.Helpers;

namespace ImageFunctions.Maze
{
	//http://www.neocomputer.org/projects/eller.html
	//http://weblog.jamisbuck.org/2010/12/29/maze-generation-eller-s-algorithm
	//TODO resulting image doesn't look right

	public class Ellers : IMaze
	{
		public Options O { get; set; }
		public Action<int,int,PickWall> DrawCell { get; set; }
		public Func<int,int,PickWall,bool> IsBlocked { get; set; }
		public int CellsWide { get; set; }
		public int CellsHigh { get; set; }

		Random Rnd = null;

		public void DrawMaze(ProgressBar prog)
		{
			Rnd = O.RndSeed.HasValue ? new Random(O.RndSeed.Value) : new Random();

			int width = CellsWide;
			int lastRow = CellsHigh - 1;
			int[] row0 = new int[width];
			int[] row1 = new int[width];
			int[] cRow = row0; //current row
			int[] nRow = row1; //next row
			int nextSet = width+1; //set to the value after Step 1
			int rowNum = 0; //row #

			//Step 1: Initialize the cells of the first row to each exist in their own set.
			for(int c=0; c<width; c++) {
				cRow[c] = c + 1; //zero means not in a set
			}

			while(rowNum < lastRow)
			{
				prog.Report((double)rowNum / lastRow);

				//Step 2: Now, randomly join adjacent cells, but only if they are not in the same set.
				// When joining adjacent cells, merge the cells of both sets into a single set, indicating
				// that all cells in both sets are now connected (there is a path that connects any two
				// cells in the set).
				for (int c = 1; c < width; c++) {
					bool merge = Rnd.RandomChoice();
					if (merge) {
						cRow[c] = cRow[c - 1];
					}
				}

				//Step 3: For each set, randomly create vertical connections downward to the next row.
				// Each remaining set must have at least one vertical connection. The cells in the next
				// row thus connected must share the set of the cell above them.
				int currSet = cRow[0]; //current set #
				bool hasConnection = false;
				for (int c = 0; c <= width; c++) {
					bool wasLast = c == width || cRow[c] != currSet;
					if (wasLast) {
						if (!hasConnection) {
							nRow[c - 1] = cRow[c - 1];
						}
						hasConnection = false;
						if (c == width) { break; }
						currSet = cRow[c];
					}

					bool connect = Rnd.RandomChoice();
					if (connect) {
						nRow[c] = cRow[c];
						hasConnection = true;
					}
				}

				//Step 4: Flesh out the next row by putting any remaining cells into their own sets.
				for (int c = 0; c < width; c++) {
					if (nRow[c] == 0) { //zero means not in a set
						nRow[c] = nextSet;
						nextSet++;
						if (nextSet >= int.MaxValue) { nextSet = 1; }
					}
				}

				//draw Here
				
				EllersDrawRow(cRow, nRow, rowNum);

				// Step 5: Repeat until the last row is reached.
				rowNum++;
				(cRow, nRow) = (nRow, cRow); //swap rows
				for (int c = 0; c < width; c++) { nRow[c] = 0; } //clear next row
			}

			//Step 6: For the last row, join all adjacent cells that do not share a set,
			// and omit the vertical connections, and you’re done!
			for (int c=1; c<width; c++) {
				if (cRow[c] != cRow[c-1]) {
					cRow[c] = cRow[c-1];
				}
			}

			//draw last row
			EllersDrawRow(cRow, null, rowNum);
		}

		void EllersDrawRow(int[] cRow, int[] nRow, int rowNum)
		{
			PickWall w = PickWall.None;
			int width = CellsWide;

			for (int c = 0; c < width; c++) {
				int nc = c == 0 ? c + 1 : c - 1;
				if (cRow[nc] == cRow[c]) {
					w |= PickWall.W;
					if (nRow != null && cRow[c] == nRow[c]) {
						w |= PickWall.S;
					}
					DrawCell(c,rowNum,w);
				}
			}
		}

	}
}