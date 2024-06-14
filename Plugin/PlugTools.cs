using ImageFunctions.Core;
using Rasberry.Cli;
using System.Drawing;
using System.Globalization;

namespace ImageFunctions.Plugin;

internal static class PlugTools
{
	/// <summary>
	/// Sorts a list using a multi-threaded sort. Seems to work best on machines with 4+ cores
	/// </summary>
	/// <typeparam name="T">Generic Type parameter</typeparam>
	/// <param name="array">The IList<T> of items to sort</param>
	/// <param name="comp">Comparer function for T</param>
	/// <param name="progress">optional progress object</param>
	/// <param name="MaxDegreeOfParallelism">Maximum number of threads to allow</param>
	public static void ParallelSort<T>(IList<T> array, IComparer<T> comp = null, IProgress<double> progress = null, int? MaxDegreeOfParallelism = null)
	{
		//var ps = new QuickSort<T>(array,comp,progress);
		//ps.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
		//ps.Sort();

		var ps = new BitonicSort<T>(array, comp, progress);
		ps.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
		ps.Sort();
	}

	/// <summary>
	/// Ensures the parameter is greater than zero.
	/// </summary>
	/// <typeparam name="T">Type parameter must be IComparable</typeparam>
	/// <param name="r">The result of the parameter parsing</param>
	/// <param name="includeZero">whether to include zero as a valid option or not</param>
	/// <returns>An updated result</returns>
	public static ParseResult<T> BeGreaterThanZero<T>(this ParseResult<T> r, bool includeZero = false)
		where T : IComparable
	{
		return BeGreaterThan<T>(r,default,includeZero);
	}

	/// <summary>
	/// Ensures the parameter is greater than a given number.
	/// </summary>
	/// <typeparam name="T">Type parameter must be IComparable</typeparam>
	/// <param name="r">The result of the parameter parsing</param>
	/// <param name="inclusive">whether to include the minimum as valid option or not</param>
	/// <returns>An updated result</returns>
	public static ParseResult<T> BeGreaterThan<T>(this ParseResult<T> r, T minimum, bool inclusive = false)
		where T : IComparable
	{
		if(r.IsBad()) { return r; }

		var t = typeof(T);
		var nullType = Nullable.GetUnderlyingType(t);
		if(nullType != null) { t = nullType; }
		bool isInvalid = false;

		if(r.Value is double vd) {
			double min = (minimum is IConvertible c) ? c.ToDouble(CultureInfo.InvariantCulture) : 0.0;

			if((!inclusive && (vd - min) >= double.Epsilon)
				|| (inclusive && (vd - min) >= 0.0)) {
				return r with { Result = ParseParams.Result.Good };
			}
			isInvalid = true;
		}
		else if(r.Value is IComparable vi) {
			var compare = vi.CompareTo(minimum);
			if((!inclusive && compare > 0) || (inclusive && compare >= 0)) {
				return r with { Result = ParseParams.Result.Good };
			}
			isInvalid = true;
		}

		if(isInvalid) {
			Log.Error(Note.MustBeGreaterThan(r.Name, minimum, inclusive));
			return r with { Result = ParseParams.Result.UnParsable };
		}
		else {
			throw PlugSqueal.NotSupportedTypeByFunc(t, nameof(BeGreaterThan));
		}
	}

	/// <summary>
	/// Ensures the parameter is between two numbers
	/// </summary>
	/// <typeparam name="T">Type parameter must be an IComparable</typeparam>
	/// <param name="r">The result of the parameter parsing</param>
	/// <param name="low">The smallest allowable value</param>
	/// <param name="high">The largest allowable value</param>
	/// <param name="lowInclusive">Whether to allow the value itself</param>
	/// <param name="highInclusive">Whether to allow the value itself</param>
	/// <returns>An updated result</returns>
	public static ParseResult<T> BeBetween<T>(this ParseResult<T> r, T low, T high,
		bool lowInclusive = true, bool highInclusive = true) where T : IComparable
	{
		if(r.IsBad()) { return r; }

		var t = typeof(T);
		var nullType = Nullable.GetUnderlyingType(t);
		if(nullType != null) { t = nullType; }

		var clow = r.Value.CompareTo(low);
		var chigh = r.Value.CompareTo(high);
		if((!lowInclusive && clow > 0 || lowInclusive && clow >= 0)
			&& (!highInclusive && chigh < 0 || highInclusive && chigh <= 0)) {
			return r with { Result = ParseParams.Result.Good };
		}

		Log.Error(PlugNote.MustBeInRange(r.Name, low, high, lowInclusive, highInclusive));
		return r with { Result = ParseParams.Result.UnParsable };
	}

	//ratio 0.0 = 100% a
	//ratio 1.0 = 100% b
	/// <summary>
	/// Calculates the middle color between two colors
	/// </summary>
	/// <param name="a">The first color</param>
	/// <param name="b">The second color</param>
	/// <param name="ratio">The ratio between colors
	///  ratio 0.0 = 100% color a
	///  ratio 1.0 = 100% color b
	/// </param>
	/// <returns>The new between color</returns>
	public static ColorRGBA BetweenColor(ColorRGBA a, ColorRGBA b, double ratio)
	{
		ratio = Math.Clamp(ratio, 0.0, 1.0);
		double nr = (1.0 - ratio) * a.R + ratio * b.R;
		double ng = (1.0 - ratio) * a.G + ratio * b.G;
		double nb = (1.0 - ratio) * a.B + ratio * b.B;
		double na = (1.0 - ratio) * a.A + ratio * b.A;
		var btw = new ColorRGBA(nr, ng, nb, na);
		// Log.Debug("between a="+a+" b="+b+" r="+ratio+" nr="+nr+" ng="+ng+" nb="+nb+" na="+na+" btw="+btw);
		return btw;
	}

	/// <summary>
	/// Wrapper for Parallel.For that includes progress
	/// </summary>
	/// <param name="max">The total number of iterations</param>
	/// <param name="callback">The callsback is called on each iteration</param>
	/// <param name="progress">Optional progress object</param>
	public static void ThreadRun(int max, Action<int> callback, int? maxThreads, IProgress<double> progress = null)
	{
		int done = 0;
		ParallelOptions po = new();
		if(maxThreads.HasValue) {
			po.MaxDegreeOfParallelism = maxThreads.Value < 1 ? 1 : maxThreads.Value;
		};
		Parallel.For(0, max, po, num => {
			Interlocked.Add(ref done, 1);
			progress?.Report((double)done / max);
			callback(num);
		});
	}

	/// <summary>
	/// Copies the pixels from one canvas to another
	/// </summary>
	/// <param name="dstImg">The canvas that will be modified</param>
	/// <param name="srcImg">The canvas used to retrieve the pixels</param>
	/// <param name="dstRect">Constrains the copy to this rectangle in the destination image</param>
	/// <param name="srcPoint">Sets the point offset where the pixels will be copied from</param>
	public static void CopyFrom(this ICanvas dstImg, ICanvas srcImg,
		Rectangle dstRect = default,
		Point srcPoint = default)
	{
		if(dstRect.IsEmpty) {
			dstRect = dstImg.Bounds();
		}

		for(int y = dstRect.Top; y < dstRect.Bottom; y++) {
			int cy = y - dstRect.Top + srcPoint.Y;
			for(int x = dstRect.Left; x < dstRect.Right; x++) {
				int cx = x - dstRect.Left + srcPoint.X;
				dstImg[x, y] = srcImg[cx, cy];
			}
		}
	}

	/// <summary>
	/// Fills the canvas with a single color
	/// </summary>
	/// <param name="canvas">The canvas to fill</param>
	/// <param name="color">Fill color</param>
	/// <param name="rect">Optional area to fill instead of the entire canvas</param>
	public static void FillWithColor(ICanvas canvas, ColorRGBA color, Rectangle rect = default)
	{
		Rectangle bounds = new Rectangle(0, 0, canvas.Width, canvas.Height);
		if(!rect.IsEmpty) {
			bounds.Intersect(rect);
		}

		for(int y = bounds.Top; y < bounds.Bottom; y++) {
			for(int x = bounds.Left; x < bounds.Right; x++) {
				canvas[x, y] = color;
			}
		}
	}

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
	/// Shortcut for getting the bounds rectangle for a canvas
	/// </summary>
	/// <param name="canvas">The canvas</param>
	/// <returns>A rectangle starting at point 0,0 and width/height matching the canvas</returns>
	public static Rectangle Bounds(this ICanvas canvas)
	{
		return new Rectangle(0, 0, canvas.Width, canvas.Height);
	}

	const int NomSize = 1024;
	/// <summary>
	/// Helpers to get the default width / height either provided by the user
	///  or provided as an input
	/// </summary>
	/// <param name="options">ICoreOptions object - usually passed to a function</param>
	/// <param name="defaultWidth">The fallback width to use</param>
	/// <param name="defaultHeight">The fallback height to use/param>
	/// <returns>A tuple with width, height</returns>
	public static (int, int) GetDefaultWidthHeight(this ICoreOptions options, int defaultWidth = NomSize, int defaultHeight = NomSize)
	{
		return (
			options.DefaultWidth.GetValueOrDefault(defaultWidth),
			options.DefaultHeight.GetValueOrDefault(defaultHeight)
		);
	}

	static char[] RectPointDelims = new char[] { ' ', ',', 'x' };

	/// <summary>
	/// Parse a sequence of numbers into a point object
	/// Sequence may be seperated by space, comma or 'x'
	/// </summary>
	/// <param name="arg">argument value</param>
	/// <returns>A Point</returns>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="OverflowException"></exception>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="FormatException"></exception>
	public static Point ParsePoint(string arg)
	{
		var parser = new ParseParams.Parser<int>(int.Parse);
		var list = ExtraParsers.ParseSequence(arg, RectPointDelims, parser);
		if(list.Count != 2) { //must be two elements x,y
			throw PlugSqueal.SequenceMustContain(2);
		}
		return new Point(list[0], list[1]);
	}

	/// <summary>
	/// Parse a sequence of numbers into a rectangle object
	/// Sequence may be seperated by space, comma or 'x'
	/// </summary>
	/// <param name="arg">argument value</param>
	/// <returns>A Rectangle</returns>
	/// <exception cref="ArgumentException"></exception>
	/// <exception cref="OverflowException"></exception>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="FormatException"></exception>
	public static Rectangle ParseRectangle(string arg)
	{
		var parser = new ParseParams.Parser<int>(int.Parse);
		var list = ExtraParsers.ParseSequence(arg, RectPointDelims, parser);
		if(list.Count != 2 && list.Count != 4) { //must be two or four elements w,h / x,y,w,h
			throw PlugSqueal.SequenceMustContainOr(2, 4);
		}
		if(list.Count == 2) {
			//assume width / height for 2 elements
			return new Rectangle(0, 0, list[0], list[1]);
		}
		else {
			//x, y, w, h
			return new Rectangle(list[0], list[1], list[2], list[3]);
		}
	}

	/// <summary>
	/// Attempts to parse a color from name for hex value. for example 'red' or '#FF0000'
	/// </summary>
	/// <param name="arg">input string</param>
	/// <returns>ColorRGBA object</returns>
	public static ColorRGBA ParseColor(string arg)
	{
		var sdc = ExtraParsers.ParseColor(arg);
		return ColorRGBA.FromRGBA255(sdc.R, sdc.G, sdc.B, sdc.A);
	}
}
