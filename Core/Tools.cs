using System.Drawing;

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
		int maxThreads = 1, IProgress<double> progress = null)
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
		int maxThreads = 1, IProgress<double> progress = null)
	{
		long done = 0;
		long max = (long)rect.Width * rect.Height;
		var po = new ParallelOptions {
			MaxDegreeOfParallelism = maxThreads < 1 ? 1 : maxThreads
		};
		Parallel.For(0, max, po, num => {
			int y = (int)(num / rect.Width);
			int x = (int)(num % rect.Width);
			Interlocked.Add(ref done,1);
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
			throw Squeal.LayerMustHaveOne();
		}
		return canvas;
	}

	public static void DrawLine(this IImageEngine engine, ICanvas canvas, ColorRGBA color, PointD start, PointD end, double width = 1.0)
	{
		if (!(engine is IDrawEngine artist)) {
			throw Squeal.EngineCannotDrawLines(engine.ToString());
		}

		artist.DrawLine(canvas, color, start, end, width);
	}
}