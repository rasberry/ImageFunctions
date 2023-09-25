namespace ImageFunctions.Core;

/// <summary>
/// Shortcut to often used stuff
/// </summary>
public static class Tools
{
	/// <summary>
	/// The chosen IImageEngine
	/// </summary>
	public static IImageEngine Engine {
		get {
			return Options.Engine.Value;
		}
	}

	/// <summary>
	/// The MaxDegreeOfParallelism settings value
	/// </summary>
	public static int? MaxDegreeOfParallelism {
		get {
			return Options.MaxDegreeOfParallelism;
		}
	}

	/// <summary>
	/// Calls in parallel the given function once for each pixel (x,y) in the image.
	///  Note: ICanvas is not thread safe.
	/// </summary>
	/// <param name="image"></param>
	/// <param name="callback"></param>
	/// <param name="progress"></param>
	public static void ThreadPixels(this ICanvas image, Action<int,int> callback,
		IProgress<double> progress = null)
	{
		long done = 0;
		long max = (long)image.Width * image.Height;
		var po = new ParallelOptions {
			MaxDegreeOfParallelism = MaxDegreeOfParallelism.GetValueOrDefault(1)
		};
		Parallel.For(0, max, po, num => {
			int y = (int)(num / image.Width);
			int x = (int)(num % image.Width);
			Interlocked.Add(ref done,1);
			progress?.Report((double)done / max);
			callback(x,y);
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
	public static ICanvas NewCanvasFromLayersOrDefault(this ILayers layers, int width, int height)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}

		ICanvas more;
		if (layers.Count < 1) {
			more = Engine.NewCanvas(width, height);
		}
		else {
			var proto = layers.First();
			more = Engine.NewCanvas(proto.Width, proto.Height);
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
	public static bool TryNewCanvasFromLayers(this ILayers layers, out ICanvas canvas)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}

		if (layers.Count < 1) {
			canvas = default;
			return false;
		}

		var proto = layers.First();
		canvas = Engine.NewCanvas(proto.Width, proto.Height);
		layers.Add(canvas);

		return true;
	}

	/// <summary>
	/// Creates an image with the same dimensions as the first image in the layers list
	///  Note: if you add the new canvas to the layers list, do not dispose (don't add 'using')
	/// </summary>
	/// <param name="layers">The ILayers object</param>
	/// <returns>The newly created canvas</returns>
	public static ICanvas NewCanvasFromLayers(this ILayers layers)
	{
		bool worked = TryNewCanvasFromLayers(layers, out var canvas);
		if (!worked) {
			throw Squeal.LayerMustHaveOne();
		}
		return canvas;
	}
}