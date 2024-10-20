namespace ImageFunctions.Plugin.Aides;

public static class MathAide
{
	/// <summary>
	/// Random coin toss
	/// </summary>
	/// <param name="rnd">Instance of the Random class</param>
	/// <param name="bias">Percent chance of returning true</param>
	public static bool RandomChoice(this Random rnd, double bias = 0.5)
	{
		return rnd.NextDouble() < bias;
	}

	/// <summary>
	/// Produces a positive random Int64
	/// </summary>
	/// <param name="rnd">Instance of the Random class</param>
	/// <param name="min">Optional minimum (inclusive)</param>
	/// <param name="max">Optional maximum (exclusive)</param>
	/// <returns>The random number</returns>
	public static long RandomLong(this Random rnd, long min = 0, long max = long.MaxValue)
	{
		byte[] buf = new byte[8];
		rnd.NextBytes(buf);
		long longRand = BitConverter.ToInt64(buf, 0);
		return Math.Abs(longRand % (max - min)) + min;
	}

	//start: top left
	//run  : right to left
	//fill : top to bottom
	/// <summary>
	/// Converts an index to an x,y coordinate according to this plan:
	///   start: top left
	///   run  : right to left
	///   fill : top to bottom
	/// </summary>
	/// <param name="position">The position index</param>
	/// <param name="width">The width of the 2D space</param>
	/// <param name="cx">Optional x center offset</param>
	/// <param name="cy">Optional y center offset</param>
	/// <returns>The x,y coordinate</returns>
	public static (int, int) LinearToXY(long position, int width, int cx = 0, int cy = 0)
	{
		int y = (int)(position / width);
		int x = (int)(position % width);
		return (x + cx, y + cy);
	}

	/// <summary>
	/// Converts an x,y coordinate to an index (opposite of LinearToXY)
	/// </summary>
	/// <param name="x">The x coordinate</param>
	/// <param name="y">The y coordinate</param>
	/// <param name="width">The width of the 2D space</param>
	/// <param name="cx">Optional x center offset</param>
	/// <param name="cy">Optional y center offset</param>
	/// <returns>The index</returns>
	public static long XYToLinear(int x, int y, int width, int cx = 0, int cy = 0)
	{
		x -= cx; y -= cy;
		return (long)y * width + x;
	}


	/// <summary>
	/// Converts and index to an x,y coordinate of an expanding 2D cone.
	/// Width is not required since the cone can expand forever
	///   start: top left
	///   run  : diagonal right + up
	///   fill : top left to bottom right
	/// </summary>
	/// <param name="position">The position index</param>
	/// <param name="cx">Optional x center offset</param>
	/// <param name="cy">Optional y center offset</param>
	/// <returns>The x,y coordinate</returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public static (int, int) DiagonalToXY(long position, int cx = 0, int cy = 0)
	{
		if(position < 0) {
			throw new ArgumentOutOfRangeException("position must be positive");
		}
		//X 0,0,1,0,1,2,0,1,2,3,0,1,2,3,4,0,1,2,3,4,5
		//Y 0,1,0,2,1,0,3,2,1,0,4,3,2,1,0,5,4,3,2,1,0
		//X https://oeis.org/A002262
		//Y https://oeis.org/A025581

		long t = (long)Math.Floor((1.0 + Math.Sqrt(1.0 + 8.0 * position)) / 2.0);
		int x = (int)(position - t * (t - 1) / 2);
		int y = (int)((t * (t + 1) - 2 * position - 2) / 2);
		return (x + cx, y + cy);
	}

	/// <summary>
	/// Converts an x,y coordinate to an index of an expanding 2D cone. (opposite of DiagonalToXY)
	/// </summary>
	/// <param name="x">The x coordinate</param>
	/// <param name="y">The y coordinate</param>
	/// <param name="cx">Optional x center offset</param>
	/// <param name="cy">Optional y center offset</param>
	/// <returns>the position</returns>
	public static long XYToDiagonal(int x, int y, int cx = 0, int cy = 0)
	{
		//solve([x = p - (t * (t - 1)) / 2, y = ((t - 1)*(t / 2 + 1)) - p],[p,t]);
		//p1=(y^2+x*(2*y+3)+y+x^2)/2

		x -= cx; y -= cy;
		long pos = (y * y + x * (2 * y + 3) + y + x * x) / 2;
		return pos;
	}

	/// <summary>
	/// Convert an index to an x,y cooridnate of a square spiral
	/// </summary>
	/// <param name="position">The index</param>
	/// <param name="cx">Optional x center offset</param>
	/// <param name="cy">Optional y center offset</param>
	/// <returns>The x,y coordinate</returns>
	public static (int, int) SpiralSquareToXY(long position, int cx = 0, int cy = 0)
	{
		// https://math.stackexchange.com/questions/163080/on-a-two-dimensional-grid-is-there-a-formula-i-can-use-to-spiral-coordinates-in

		position++; //1 based spiral
		long k = (long)Math.Ceiling((Math.Sqrt(position) - 1.0) / 2.0);
		long t = 2 * k + 1;
		long m = t * t;
		t -= 1;
		/// Console.WriteLine($"p={position} k={k} t={t} m={m} (m-t)={m-t}");
		if(position >= m - t) {
			var x = k - (m - position);
			var y = k;
			return ((int)x + cx, (int)y + cy);
		}
		m -= t;
		if(position >= m - t) {
			var x = -k;
			var y = k - (m - position);
			return ((int)x + cx, (int)y + cy);
		}
		m -= t;
		if(position >= m - t) {
			var x = -k + (m - position);
			var y = -k;
			return ((int)x + cx, (int)y + cy);
		}
		else {
			var x = k;
			var y = -k + (m - position - t);
			return ((int)x + cx, (int)y + cy);
		}
	}

	/// <summary>
	///  Convert an x,y coordinate to an index of a square spiral (opposite of SpiralSquareToXY)
	/// </summary>
	/// <param name="x">The x coordinate</param>
	/// <param name="y">The y coordinate</param>
	/// <param name="cx">Optional x center offset</param>
	/// <param name="cy">Optional y center offset</param>
	/// <returns></returns>
	public static long XYToSpiralSquare(int x, int y, int cx = 0, int cy = 0)
	{
		// https://www.reddit.com/r/dailyprogrammer/comments/3ggli3/20150810_challenge_227_easy_square_spirals/
		x -= cx;
		y -= cy;

		y = -y; //original is CW i need CCW
		if(x >= y) {
			if(x > -y) {
				long m = 2 * x - 1; m *= m;
				return m + Math.Abs(x) + y - 1;
			}
			else {
				long m = 2 * y + 1; m *= m;
				return m + Math.Abs(7 * y) + x - 1;
			}
		}
		else {
			if(x > -y) {
				long m = 2 * y - 1; m *= m;
				return m + Math.Abs(3 * y) - x - 1;
			}
			else {
				long m = 2 * x + 1; m *= m;
				return m + Math.Abs(5 * x) - y - 1;
			}
		}
	}

	/// <summary>
	/// Calculates the middle number between two numbers
	/// </summary>
	/// <param name="a">The first number</param>
	/// <param name="b">The second number</param>
	/// <param name="ratio">The ratio between colors
	///  ratio 0.0 = 100% color a
	///  ratio 1.0 = 100% color b
	/// </param>
	/// <returns>The new between number</returns>
	public static double Between(double a, double b, double ratio)
	{
		ratio = Math.Clamp(ratio, 0.0, 1.0);
		double c = (1.0 - ratio) * a + ratio * b;
		return c;
	}

	/// <summary>
	/// Find the maximum value in a sequence. Wrapper for Enumerable.Max<>
	/// </summary>
	/// <typeparam name="T">The Type of the values</typeparam>
	/// <param name="values">One or more values</param>
	/// <returns>The maximum value</returns>
	public static T Max<T>(params T[] values)
	{
		return values.Max();
	}

	/// <summary>
	/// Find the minimum value in a sequence. Wrapper for Enumerable.Min<>
	/// </summary>
	/// <typeparam name="T">The Type of the values</typeparam>
	/// <param name="values">One or more values</param>
	/// <returns>The minimum value</returns>
	public static T Min<T>(params T[] values)
	{
		return values.Min();
	}
}
