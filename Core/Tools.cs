using System.Drawing;
using Rasberry.Cli;

namespace ImageFunctions.Core;

/// <summary>
/// Shortcut to often used stuff
/// </summary>
public static class Tools
{
	/// <summary>
	/// Calls in parallel the given function once for each pixel (x,y) in the image.
	///  Note: ICanvas is not thread safe.
	/// </summary>
	/// <param name="image"></param>
	/// <param name="callback"></param>
	/// <param name="progress"></param>
	public static void ThreadPixels(this ICanvas image, Action<int,int> callback,
		int? maxThreads = null, IProgress<double> progress = null)
	{
		var size = new Rectangle(0, 0, image.Width, image.Height);
		ThreadPixels(size, callback, maxThreads, progress);
	}

	/// <summary>
	/// Calls in parallel the given function once for each position (x,y) in the rectangle.
	/// </summary>
	/// <param name="rect"></param>
	/// <param name="callback"></param>
	/// <param name="progress"></param>
	public static void ThreadPixels(this Rectangle rect, Action<int,int> callback,
		int? maxThreads = null, IProgress<double> progress = null)
	{
		long done = 0;
		long max = (long)rect.Width * rect.Height;

		ParallelOptions po = new();
		if (maxThreads.HasValue) {
			po.MaxDegreeOfParallelism = maxThreads.Value < 1 ? 1 : maxThreads.Value;
		}

		Parallel.For(0, max, po, num => {
			int y = (int)(num / rect.Width);
			int x = (int)(num % rect.Width);
			Interlocked.Increment(ref done);
			progress?.Report((double)done / max);
			callback(x + rect.Left,y + rect.Top);
		});
	}

	//https://en.wikipedia.org/wiki/Sinc_function
	public static double SinC(double v)
	{
		if (Math.Abs(v) < double.Epsilon) {
			return 1.0;
		}
		v *= Math.PI; //normalization factor
		double s = Math.Sin(v) / v;
		return Math.Abs(s) < double.Epsilon ? 0.0 : s;
	}

	public static bool EqualsIC(this string sub, string test)
	{
		return sub.Equals(test,StringComparison.CurrentCultureIgnoreCase);
	}

	public static bool StartsWithIC(this string sub, string test)
	{
		return sub.StartsWith(test,StringComparison.CurrentCultureIgnoreCase);
	}

	public static bool EndsWithIC(this string sub, string test)
	{
		return sub.EndsWith(test,StringComparison.CurrentCultureIgnoreCase);
	}

	/// <summary>
	/// Creates a new ICanvas from the top layers width / height or the given
	/// width / height if no layers exist
	///  Note: if you add the new canvas to the layers list, do not dispose (don't add 'using')
	/// </summary>
	/// <param name="layers">The ILayers object</param>
	/// <param name="width">default width - used if no layers exist</param>
	/// <param name="height">default height - used if no layers exist</param>
	/// <returns>The created ICanvas</returns>
	public static ICanvas NewCanvasFromLayersOrDefault(this IImageEngine engine, ILayers layers, int width, int height)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}

		ICanvas more;
		if (layers.Count < 1) {
			more = engine.NewCanvas(width, height);
		}
		else {
			var proto = layers.First();
			more = engine.NewCanvas(proto.Width, proto.Height);
		}

		return more;
	}

	/// <summary>
	/// Creates an image with the same dimensions as the first image in the layers list
	///  Note: if you add the new canvas to the layers list, do not dispose (don't add 'using')
	/// </summary>
	/// <param name="layers">The ILayers object</param>
	/// <param name="canvas">The newly created canvas</param>
	/// <returns>false if there are no layers otherwise true</returns>
	public static bool TryNewCanvasFromLayers(this IImageEngine engine, ILayers layers, out ICanvas canvas)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}

		if (layers.Count < 1) {
			canvas = default;
			return false;
		}

		var proto = layers.First();
		canvas = engine.NewCanvas(proto.Width, proto.Height);

		return true;
	}

	/// <summary>
	/// Creates an image with the same dimensions as the first image in the layers list
	///  Note: if you add the new canvas to the layers list, do not dispose (don't add 'using')
	/// </summary>
	/// <param name="layers">The ILayers object</param>
	/// <returns>The newly created canvas</returns>
	public static ICanvas NewCanvasFromLayers(this IImageEngine engine, ILayers layers)
	{
		bool worked = engine.TryNewCanvasFromLayers(layers, out var canvas);
		if (!worked) {
			throw Squeal.LayerMustHaveAtLeast();
		}
		return canvas;
	}

	//TODO this might go away
	public static void DrawLine(this IImageEngine engine, ICanvas canvas, ColorRGBA color, PointD start, PointD end, double width = 1.0)
	{
		if (!(engine is IDrawEngine artist)) {
			throw Squeal.EngineCannotDrawLines(engine.ToString());
		}

		artist.DrawLine(canvas, color, start, end, width);
	}

	/// <summary>
	/// Turns a number between 0 and 9 into the word
	/// </summary>
	/// <param name="number">A number between 0 and 9 (inclusive)</param>
	/// <returns>The word</returns>
	/// <exception cref="ArgumentOutOfRangeException">When the number is not supported</exception>
	public static string NumberToWord(int number)
	{
		switch(number) {
			case 0: return "zero";
			case 1: return "one";
			case 2: return "two";
			case 3: return "three";
			case 4: return "four";
			case 5: return "five";
			case 6: return "six";
			case 7: return "seven";
			case 8: return "eight";
			case 9: return "nine";
		}
		throw Squeal.ArgumentOutOfRange(nameof(number));
	}

	/// <summary>
	/// Shortcut for printing a message when a parameter can't be parsed
	/// </summary>
	/// <typeparam name="T">Argument Type</typeparam>
	/// <param name="result">The result of an argument parse function</param>
	/// <returns>The result</returns>
	public static ParseResult<T> WhenInvalidTellDefault<T>(this ParseResult<T> result)
	{
		if (result.IsInvalid()) {
			Tell.CouldNotParse(result.Name, result.Error);
		}
		return result;
	}
}