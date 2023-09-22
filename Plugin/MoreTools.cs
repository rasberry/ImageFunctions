using System.Drawing;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin;

internal static class MoreTools
{
	public static void ParallelSort<T>(IList<T> array, IComparer<T> comp = null, IProgress<double> progress = null, int? MaxDegreeOfParallelism = null)
	{
		var ps = new ParallelSort<T>(array,comp,progress);
		if (MaxDegreeOfParallelism.HasValue && MaxDegreeOfParallelism.Value > 0) {
			ps.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
		}
		ps.Sort();
	}

	public static ParseParams.Result BeGreaterThanZero<T>(this ParseParams.Result r, string name, T val, bool includeZero = false)
	{
		if (r.IsBad()) { return r; }

		var t = typeof(T);
		var nullType = Nullable.GetUnderlyingType(t);
		if (nullType != null) { t = nullType; }
		bool isInvalid = false;

		if (t.Equals(typeof(double))) {
			double v = (double)(object)val;
			if ((!includeZero && v >= double.Epsilon)
				|| (includeZero && v >= 0.0)) {
				return ParseParams.Result.Good;
			}
			isInvalid = true;
		}
		else if (t.Equals(typeof(int))) {
			int v = (int)(object)val;
			if ((!includeZero && v > 0)
				|| (includeZero && v >= 0)) {
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
}