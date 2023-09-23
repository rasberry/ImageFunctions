using System.Drawing;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin;

internal static class MoreTools
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
		var ps = new ParallelSort<T>(array,comp,progress);
		if (MaxDegreeOfParallelism.HasValue && MaxDegreeOfParallelism.Value > 0) {
			ps.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
		}
		ps.Sort();
	}

	/// <summary>
	/// Ensures the parameter is greater than zero.
	/// </summary>
	/// <typeparam name="T">The only supported types are double and int</typeparam>
	/// <param name="r">The result of the parameter parsing</param>
	/// <param name="name">name of the option</param>
	/// <param name="val">value of the parameter</param>
	/// <param name="includeZero">whether to include zero as a valid option or not</param>
	/// <returns></returns>
	public static ParseParams.Result BeGreaterThanZero<T>(this ParseParams.Result r, string name, T val, bool includeZero = false)
	{
		if (r.IsBad()) { return r; }

		var t = typeof(T);
		var nullType = Nullable.GetUnderlyingType(t);
		if (nullType != null) { t = nullType; }
		bool isInvalid = false;

		if (val is double vd) {
			if ((!includeZero && vd >= double.Epsilon)
				|| (includeZero && vd >= 0.0)) {
				return ParseParams.Result.Good;
			}
			isInvalid = true;
		}
		else if (val is int vi) {
			if ((!includeZero && vi > 0)
				|| (includeZero && vi >= 0)) {
				return ParseParams.Result.Good;
			}
			isInvalid = true;
		}

		if (isInvalid) {
			Core.Tell.MustBeGreaterThanZero(name,includeZero);
			return ParseParams.Result.UnParsable;
		}
		else {
			throw PlugSqueal.NotSupportedTypeByFunc(t,nameof(BeGreaterThanZero));
		}
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
		ratio = Math.Clamp(ratio,0.0,1.0);
		double nr = (1.0 - ratio) * a.R + ratio * b.R;
		double ng = (1.0 - ratio) * a.G + ratio * b.G;
		double nb = (1.0 - ratio) * a.B + ratio * b.B;
		double na = (1.0 - ratio) * a.A + ratio * b.A;
		var btw = new ColorRGBA(nr,ng,nb,na);
		// Log.Debug("between a="+a+" b="+b+" r="+ratio+" nr="+nr+" ng="+ng+" nb="+nb+" na="+na+" btw="+btw);
		return btw;
	}

	/// <summary>
	/// Wrapper for Parallel.For that includes progress and observes the --max-threads option
	/// </summary>
	/// <param name="max">The total number of iterations</param>
	/// <param name="callback">The callsback is called on each iteration</param>
	/// <param name="progress">Optional progress object</param>
	public static void ThreadRun(int max, Action<int> callback, IProgress<double> progress = null)
	{
		int done = 0;
		var po = new ParallelOptions {
			MaxDegreeOfParallelism = Tools.MaxDegreeOfParallelism.GetValueOrDefault(1)
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
		if (dstRect.IsEmpty) {
			dstRect = new Rectangle(0,0,srcImg.Width,srcImg.Height);
		}

		for(int y = dstRect.Top; y < dstRect.Bottom; y++) {
			int cy = y - dstRect.Top + srcPoint.Y;
			for(int x = dstRect.Left; x < dstRect.Right; x++) {
				int cx = x - dstRect.Left + srcPoint.X;
				dstImg[x,y] = srcImg[cx,cy];
			}
		}
	}

	/// <summary>
	/// Parse a parameter as a number or percent
	/// </summary>
	/// <param name="num">The parameter value. the format should be a decimal number with an optional % sign e.g. 0.401 or 40.1%</param>
	/// <param name="val">The parsed parameter</param>
	/// <returns>Whether the parsing was successfull</returns>
	public static bool ParseNumberPercent(string num, out double? val)
	{
		val = null;
		bool worked = ParseNumberPercent(num,out double v);
		if (worked) { val = v; }
		return worked;
	}

	/// <summary>
	/// Parse a parameter as a number or percent
	/// </summary>
	/// <param name="num">The parameter value. the format should be a decimal number with an optional % sign e.g. 0.401 or 40.1%</param>
	/// <param name="val">The parsed parameter</param>
	/// <returns>Whether the parsing was successfull</returns>
	public static bool ParseNumberPercent(string num, out double val)
	{
		val = 0.0;
		bool isPercent = false;
		if (num.EndsWith('%')) {
			isPercent = true;
			num = num.Remove(num.Length - 1);
		}
		if (!double.TryParse(num, out double d)) {
			Log.Error("could not parse \""+num+"\" as a number");
			return false;
		}
		if (!double.IsFinite(d)) {
			Log.Error("invalid number \""+d+"\"");
			return false;
		}
		val = isPercent ? d/100.0 : d;
		return true;
	}

	/// <summary>
	/// Helper method to create an image with the same dimensions as the first image in the layers list
	/// </summary>
	/// <param name="engine">The IImageEngine object</param>
	/// <param name="layers">The ILayers object</param>
	/// <param name="canvas">The newly created canvas</param>
	/// <returns>False if there are no layers otherwise true</returns>
	public static bool TryNewCanvasFromLayers(this IImageEngine engine, ILayers layers, out ICanvas canvas)
	{
		if (layers.Count < 1) {
			PlugTell.LayerMustHaveOne();
			canvas = default;
			return false;
		}

		var proto = layers.First();
		canvas = engine.NewCanvas(proto.Width, proto.Height);
		return true;
	}
}