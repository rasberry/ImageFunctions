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
}