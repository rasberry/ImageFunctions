using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.Maze
{
	//http://www.neocomputer.org/projects/eller.html
	//http://weblog.jamisbuck.org/2010/12/29/maze-generation-eller-s-algorithm
	//https://tromp.github.io/maze.html
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

			// http://tromp.github.io/maze.html
			PickWall M = PickWall.None;
			int W = CellsWide + 1; //+1 to account for the ghost column
			int H = CellsHigh - 1; //-1 to leave room for the last row
			int C,E,X = 0, Y = 0;
			int[] L = new int[W];
			int[] R = new int[W];

			L[0] = 1; E = W;
			while(--E > 0) { L[E] = R[E] = E; } /* close top of maze */

			while(--H >= 0) {
				C = W; X = 0;
				while(--C > 0) { /* visit cells from left to right */
					if (C != (E = L[C-1]) && Rnd.RandomChoice()) { /* make right-connection ? */
						R[E] = R[C]; L[R[C]] = E; /* link E to R[C] */
						R[C] = C-1; L[C-1] = C; /* link C to C-1 */
						M = M.AddFlag(PickWall.E); /* no wall to the right */
					}
					else {
						M = M.CutFlag(PickWall.E); /* wall to the right */
					}
					if (C != (E=L[C]) && Rnd.RandomChoice()) { /* omit down-connection ? */
						R[E] = R[C]; L[R[C]] = E; /* link E to R[C] */
						L[C] = C; R[C] = C; /* link C to C */
						M = M.CutFlag(PickWall.S); /* wall downward */
					}
					else {
						M = M.AddFlag(PickWall.S); /* no wall downward */
					}
					DrawCell(X,Y,M);
					X++;
				}
				Y++;
			}

			M = M.CutFlag(PickWall.S);
			C = W; X = 0; Y = CellsHigh - 1;
			/* bottom row */
			while(--C > 0) {
				if (C != (E = L[C-1]) && (C == R[C] || Rnd.RandomChoice())) { /* make right-connection ? */
					R[E] = R[C]; L[R[C]] = E; /* link E to R[C] */
					R[C] = C-1; L[C-1] = C; /* link C to C-1 */
					M = M.AddFlag(PickWall.E);
				}
				else {
					M = M.CutFlag(PickWall.E);
				}
				E = L[C]; /* add downward wall */
				R[E] = R[C]; L[R[C]] = E; /* link E to R[C] */
				L[C] = C; R[C] = C; /* link C to C */
				DrawCell(X,Y,M);
				X++;
			}
		}
	}

	#if false
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
			Cell[] row = new Cell[width];
			int setNum = 0;
			int rowNum = 0;

			//Create the first row. No cells will be members of any set
			for(int r = 0; r < width; r++) {
				row[r] = new Cell();
			}

			while(rowNum < lastRow) {

				for(int c = 0; c < width; c++) {
					Cell curr = row[c];
					//Join any cells not members of a set to their own unique set
					if (curr.Set == -1) {
						curr.Set = setNum++;
					}
				}

				for(int c = 0; c < width - 1; c++)
				{
					Cell curr = row[c];
					Cell next = row[c+1];

					//If the current cell and the cell to the right are members of the same set, always create a wall between them. (This prevents loops)
					if (curr.Set == next.Set) {
						curr.E = true;
					}
					//If you decide not to add a wall, union the sets to which the current cell and the cell to the right are members.
					else if (Rnd.RandomChoice()) {
						next.Set = curr.Set;
						//UnionSets(row,curr.Set,next.Set);
					}
				}

				//count up cells in each set
				var walls = new Dictionary<int,int>();
				for(int c = 0; c < width; c++) {
					Cell curr = row[c];

					if (!walls.ContainsKey(curr.Set)) {
						walls[curr.Set] = 1;
					}
					else {
						walls[curr.Set]++;
					}
				}

				//Create bottom-walls, moving from left to right
				for(int c = 0; c < width; c++) {
					Cell curr = row[c];

					int count = walls[curr.Set];
					//If a cell is the only member of its set, do not create a bottom-wall
					//If a cell is the only member of its set without a bottom-wall, do not create a bottom-wall
					if (count > 1 && Rnd.RandomChoice()) {
						curr.S = true;
						walls[curr.Set]--;
					}
				}

				DrawRow(row,rowNum);
				rowNum++;

				// If you decide to add another row
				for(int c = 0; c < width; c++) {
					Cell curr = row[c];
					if (curr.S) { curr.Set = -1; } //Remove cells with a bottom-wall from their set
					curr.E = false; //Remove all right walls
					curr.S = false; //Remove all bottom walls
				}
			}

			// If you decide to complete the maze
			for(int c = 0; c < width - 1; c++) {
				Cell curr = row[c];
				Cell next = row[c+1];
				curr.S = true; //Add a bottom wall to every cell

				//If the current cell and the cell to the right are members of a different set
				if (curr.Set != next.Set) {
					curr.E = false; //Remove the right wall
					//UnionSets(row,curr.Set,next.Set);
					next.Set = curr.Set; //Union the sets to which the current cell and cell to the right are members.
				}
			}
			row[width-1].S = true;
			DrawRow(row,rowNum);
		}

		void UnionSets(Cell[] row, int to, int from)
		{
			for(int c = 0; c < row.Length; c++) {
				Cell curr = row[c];
				if (curr.Set == from) { curr.Set = to; }
			}
		}

		void DrawRow(Cell[] row, int rowNum)
		{
			for (int c = 0; c < row.Length; c++) {
				Cell curr = row[c];
				PickWall w = (!curr.E ? PickWall.E : PickWall.None) | (!curr.S ? PickWall.S : PickWall.None);
				DrawCell(c,rowNum,w);
			}
		}
	}

	class Cell
	{
		public bool S = true;
		public bool E = false;
		public int Set = -1;
	}
	#endif

	#if false
	// https://github.com/dlrht/EllersMaze/blob/master/Assets/Scripts/MazeController.cs
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
			Cell[] cells = new Cell[width];
			int setNum = 0;
			int len = 0;

			for(int i = 0; i < width; i++) {
				cells[i] = new Cell();
			}


			while(len < CellsHigh) {
				// if a cell has a bottom wall, remove it from its set
				for(int i = 0; i < width; i++) {
					if (cells[i].S) {
						cells[i].Set = -1;
					}
				}

				// make all independent cells their own set
				for (int i = 0; i < width; i++) {
					if (cells[i].Set == -1) {
						cells[i].Set = setNum++;
					}

					cells[i].S = false;
					cells[i].E = false;
				}

				// Randomly add or union disjoint sets in a row
				for (int i = 0; i < width - 1; i++) {
					// if two cells are members of same set, we MUST add a wall
					if (cells[i].Set == cells[i + 1].Set || Rnd.RandomChoice()) {
						cells[i].E = true;
					}
					else {
						UnionSets(cells, i, i + 1);
					}
				}

				// Create bottom walls, track the # of vertical passages.
				// If only 1 vertical passage in a set do NOT create a wall.
				var numPassages = new Dictionary<int, int>();
				for(int i = 0; i < width; i++) {
					if (!numPassages.ContainsKey(cells[i].Set)) {
						numPassages[cells[i].Set] = 1;
					}
					else {
						numPassages[cells[i].Set] = numPassages[cells[i].Set] + 1;
					}
				}
				for (int i = 0; i < width; i++) {
					if (numPassages[cells[i].Set] > 1 && Rnd.RandomChoice()) {
						cells[i].S = true;
						numPassages[cells[i].Set] = numPassages[cells[i].Set] - 1;
					}
				}
				//draw
				for (int i = 0; i < width; i++) {
					var c = cells[i];
					PickWall w = (c.E ? PickWall.E : PickWall.None)
						| (c.S ? PickWall.S : PickWall.None);
					DrawCell(i,len,w);
				}
				len++;
			}

			// last row
			for (int i = 0; i < width - 1; i++) {
				if (cells[i].Set != cells[i + 1].Set && cells[i].E) {
					// Destroy walls separating disjoint sets
					cells[i].E = false;
					UnionSets(cells, i, i + 1);
				}
			}

			// Add bottom walls to all cells
			for (int i = 0; i < width; i++) {
				cells[i].S = true;
			}

			//draw
			for (int i = 0; i < width; i++) {
				var c = cells[i];
				PickWall w = (!c.E ? PickWall.E : PickWall.None) | (!c.S ? PickWall.S : PickWall.None);
				DrawCell(i,len,w);
			}
		}

		// Merges disjoint sets of the given i,j values
		static void UnionSets(Cell[] arr, int i, int j)
		{
			int replaceNum = -1;
			int replaceWith = -1;

			if(arr[i].Set < arr[j].Set) {
				replaceWith = arr[i].Set;
				replaceNum = arr[j].Set;
			}
			else if(arr[j].Set < arr[i].Set) {
				replaceWith = arr[j].Set;
				replaceNum = arr[i].Set;
			}

			for (int k = 0; k < arr.Length; k++) {
				if (arr[k].Set == replaceNum) {
					arr[k].Set = replaceWith;
				}
			}
		}

	}

	class Cell
	{
		public bool S = true;
		public bool E = false;
		public int Set = -1;
	}
	#endif

	#if false
	public class Ellers : IMaze
	{
		public Options O { get; set; }
		public Action<int,int,PickWall> DrawCell { get; set; }
		public Func<int,int,PickWall,bool> IsBlocked { get; set; }
		public int CellsWide { get; set; }
		public int CellsHigh { get; set; }
		Random Rnd = null;

		class Cell : IEquatable<Cell>
		{
			public Cell(int x,int y,int s) {
				Y = y; X = x; S = s;
			}

			public int Y;
			public int X;
			public int S;

			public bool Equals(Cell o) {
				return o.Y == this.Y && o.X == this.X && o.S == this.S;
			}
			public override int GetHashCode() {
				return HashCode.Combine(X,Y,S);
			}
		}

		class CellSet
		{
			public void Add(int s, Cell c)
			{
				if (!Sets.TryGetValue(s,out var list)) {
					list = new HashSet<Cell>();
					Sets.Add(s,list);
				}
				list.Add(c);
			}

			Dictionary<int,HashSet<Cell>> Sets = new Dictionary<int,HashSet<Cell>>();

			public void Merge(
		}

		public void DrawMaze(ProgressBar prog)
		{
			Rnd = O.RndSeed.HasValue ? new Random(O.RndSeed.Value) : new Random();

			int width = CellsWide;
			int lastRow = CellsHigh - 1;
			Cell[] row = new Cell[CellsWide];
			//var sets = new Dictionary<int,List<Cell>>();
			int nextSet = CellsWide + 1; //set to the value after Step 1
			int rowNum = 0; //row #

			//Step 1: Initialize the cells of the first row to each exist in their own set.
			for(int c=0; c < width; c++) {
				int s = c + 1; //zero means not in a set
				var o = new Cell(c,rowNum,c + 1);

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
						row[c] = row[c - 1];
					}
				}

				//Step 3: For each set, randomly create vertical connections downward to the next row.
				// Each remaining set must have at least one vertical connection. The cells in the next
				// row thus connected must share the set of the cell above them.
				int currSet = row[0]; //current set #
				bool hasConnection = false;
				for (int c = 0; c <= width; c++) {
					bool wasLast = c == width || row[c] != currSet;
					if (wasLast) {
						if (!hasConnection) {
							down[c - 1] = row[c - 1];
						}
						hasConnection = false;
						if (c == width) { break; }
						currSet = row[c];
					}

					bool connect = Rnd.RandomChoice();
					if (connect) {
						down[c] = row[c];
						hasConnection = true;
					}
				}

				//Step 4: Flesh out the next row by putting any remaining cells into their own sets.
				for (int c = 0; c < width; c++) {
					if (down[c] == 0) { //zero means not in a set
						down[c] = nextSet;
						nextSet++;
						if (nextSet >= int.MaxValue) { nextSet = 1; }
					}
				}

				EllersDrawRow(row, down, rowNum);
				PrintRow(row);

				// Step 5: Repeat until the last row is reached.
				rowNum++;
				for (int c = 0; c < width; c++) {
					row[c] = down[c]; //copy next row
					down[c] = 0; //clear down
				}
			}

			//Step 6: For the last row, join all adjacent cells that do not share a set,
			// and omit the vertical connections, and you’re done!
			for (int c = 1; c < width; c++) {
				int lc = c - 1;
				if (row[lc] != row[c]) {
					row[lc] = row[c];
				}
			}
			//draw last row
			EllersDrawRow(row, null, rowNum);
			PrintRow(row);
		}

		void EllersDrawRow(int[] row, int[] down, int rowNum)
		{
			PickWall w = PickWall.None;
			int width = CellsWide;

			for (int c = 1; c < width; c++) {
				int nc = c - 1;
				if (row[nc] == row[c]) {
					w |= PickWall.W;
					if (down != null && row[c] == down[c]) {
						w |= PickWall.S;
					}
					DrawCell(c,rowNum,w);
				}
			}
		}

		void PrintRow(int[] row)
		{
			var sb = new StringBuilder();
			for (int c = 0; c < CellsWide; c++) {
				sb.Append(row[c].ToString("0000")).Append(' ');
			}
			Log.Debug(sb.ToString());
		}

	}
	#endif

	#if false
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
	#endif
}