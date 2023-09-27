using System.Collections;
using ImageFunctions.Core;
using Rasberry.Cli;
using O = ImageFunctions.Plugin.Functions.Maze.Options;

namespace ImageFunctions.Plugin.Functions.Maze;

public class Automata : IBasicMaze
{
	public ICanvas PixelGrid { get; set; }
	Random Rnd = null;
	// need to update next layer without changing current layer
	BitArray Curr;
	BitArray Next;

	public void DrawMaze(ProgressBar prog)
	{
		Rnd = O.RndSeed.HasValue ? new Random(O.RndSeed.Value) : new Random();

		// B3/S12345

		int bitLen = PixelGrid.Width * PixelGrid.Height;
		Curr = new BitArray(bitLen);
		Next = new BitArray(bitLen);
		for(int b=0; b<Curr.Length; b++) {
			bool set = Rnd.Next(0,2) == 0;
			Curr.Set(b,set); Next.Set(b,set);
		}
		bool done = false;
		int iters = 0;

		while(!done && iters < 100) {
			done = true;

			prog.Prefix = $"Iteration {iters} ";
			prog.Report(0.0);
			iters++;

			for(int y=0; y<PixelGrid.Height; y++) {
				for(int x=0; x<PixelGrid.Width; x++) {
					bool c = GetXYBit(x,y);
					int n = CountNeighbors(x,y);
					//if dead and has 3 neighbors, make alive
					if (!c && n == 3) { //B3
						SetXYBit(x,y,true);
						done = false;
					}
					//if alive and has 0 or more then 5 neighbors, kill
					else if (c && (n < 1 || n > 5)) { //S12345
						SetXYBit(x,y,false);
						done = false;
					}
				}
			}

			// Log.Debug("\n" + BitArrayToString());

			// copy updated layer to the current layer
			for(int a=0; a<bitLen; a++) { Curr[a] = Next[a]; }
		}

		for(int y=0; y<PixelGrid.Height; y++) {
			for(int x=0; x<PixelGrid.Width; x++) {
				PixelGrid[x,y] = GetXYBit(x,y) ? O.CellColor : O.WallColor;
			}
		}
	}

	void SetXYBit(int x, int y, bool v)
	{
		(x,y) = GetDonutCoords(x,y);
		int offset = y * PixelGrid.Width + x;
		Next.Set(offset,v);
	}

	bool GetXYBit(int x, int y)
	{
		(x,y) = GetDonutCoords(x,y);
		int offset = y * PixelGrid.Width + x;
		return Curr.Get(offset);
	}

	(int,int) GetDonutCoords(int x,int y)
	{
		int W = PixelGrid.Width;
		int H = PixelGrid.Height;
		if (x < 0)  { x += W; }
		if (x >= W) { x -= W; }
		if (y < 0)  { y += H; }
		if (y >= H) { y -= H; }
		return (x,y);
	}

	int CountNeighbors(int x,int y)
	{
		int total =
			  (GetXYBit(x - 1, y - 1) ? 1 : 0)
			+ (GetXYBit(x + 0, y - 1) ? 1 : 0)
			+ (GetXYBit(x + 1, y - 1) ? 1 : 0)
			+ (GetXYBit(x - 1, y + 0) ? 1 : 0)
			// + (GetXYBit(x + 0, y + 0) ? 1 : 0)
			+ (GetXYBit(x + 1, y + 0) ? 1 : 0)
			+ (GetXYBit(x - 1, y + 1) ? 1 : 0)
			+ (GetXYBit(x + 0, y + 1) ? 1 : 0)
			+ (GetXYBit(x + 1, y + 1) ? 1 : 0)
		;
		return total;
	}

	string BitArrayToString()
	{
		var sb = new StringBuilder();
		for(int y=0; y<PixelGrid.Height; y++) {
			for(int x=0; x<PixelGrid.Width; x++) {
				bool c = GetXYBit(x,y);
				int n = CountNeighbors(x,y);
				//if dead and has 3 neighbors, make alive
				if (!c && n == 3) { //B3
					sb.Append('B');
				}
				//if alive and has 0 or more then 5 neighbors, kill
				else if (c && (n < 1 || n > 5)) { //S12345
					sb.Append('K');
				}
				else {
					sb.Append(c ? 'C' : ' ');
				}
			}
			sb.AppendLine();
		}
		return sb.ToString();
	}
}
