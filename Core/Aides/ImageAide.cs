using System.Drawing;

namespace ImageFunctions.Core.Aides;

public static class ImageAide
{
	/// <summary>
	/// Calls in parallel the given function once for each pixel (x,y) in the image.
	///  Note: ICanvas is not thread safe.
	/// </summary>
	/// <param name="image"></param>
	/// <param name="callback"></param>
	/// <param name="progress"></param>
	public static void ThreadPixels(this ICanvas image, Action<int, int> callback,
		int? maxThreads = null, IProgress<double> progress = null)
	{
		if(image == null) {
			throw Squeal.ArgumentNull(nameof(image));
		}
		var size = new Rectangle(0, 0, image.Width, image.Height);
		ThreadPixels(size, callback, maxThreads, progress);
	}

	/// <summary>
	/// Calls in parallel the given function once for each position (x,y) in the rectangle.
	/// </summary>
	/// <param name="rect"></param>
	/// <param name="callback"></param>
	/// <param name="progress"></param>
	public static void ThreadPixels(this Rectangle rect, Action<int, int> callback,
		int? maxThreads = null, IProgress<double> progress = null)
	{
		long done = 0;
		long max = (long)rect.Width * rect.Height;

		ParallelOptions po = new();
		if(maxThreads.HasValue) {
			po.MaxDegreeOfParallelism = maxThreads.Value < 1 ? 1 : maxThreads.Value;
		}

		//Trace.WriteLine($"{nameof(ThreadPixels)} 1");
		Parallel.For(0, max, po, num => {
			//Trace.WriteLine($"{nameof(ThreadPixels)} Loop {num} {done}/{max}");
			int y = (int)(num / rect.Width);
			int x = (int)(num % rect.Width);
			Interlocked.Increment(ref done);
			progress?.Report((double)done / max);
			callback(x + rect.Left, y + rect.Top);
		});
		//Trace.WriteLine($"{nameof(ThreadPixels)} 2");
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
		if(layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if(engine == null) {
			throw Squeal.ArgumentNull(nameof(engine));
		}

		ICanvas more;
		if(layers.Count < 1) {
			more = engine.NewCanvas(width, height);
		}
		else {
			var proto = layers.First().Canvas;
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
		if(layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if(engine == null) {
			throw Squeal.ArgumentNull(nameof(engine));
		}

		if(layers.Count < 1) {
			canvas = default;
			return false;
		}

		var proto = layers.First().Canvas;
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
		if(!worked) {
			throw Squeal.LayerMustHaveAtLeast();
		}
		return canvas;
	}

	//TODO this might go away
	public static void DrawLine(this IImageEngine engine, ICanvas canvas, ColorRGBA color, PointD start, PointD end, double width = 1.0)
	{
		if(engine == null) {
			throw Squeal.ArgumentNull(nameof(engine));
		}

		if(engine is not IDrawEngine artist) {
			throw Squeal.EngineCannotDrawLines(engine.ToString());
		}

		artist.DrawLine(canvas, color, start, end, width);
	}
}