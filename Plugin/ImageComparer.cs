using System.Numerics;
using System.Security.Principal;
using ImageFunctions.Core;
using ImageFunctions.Core.Metrics;

namespace ImageFunctions.Plugin;

public static class ImageComparer
{
	public static bool AreCanvasEqual(ICanvas one, ICanvas two)
	{
		if (one.Width != two.Width || one.Height != two.Height) {
			return false;
		}

		for(int y = 0; y < one.Height; y++) {
			for(int x = 0; x < one.Width; x++) {
				var po = one[x,y];
				var pt = two[x,y];
				bool same = AreColorsEqual(po,pt);
				if (!same) { return false; }
			}
		}
		return true;
	}

	public static bool AreColorsEqual(ColorRGBA one, ColorRGBA two)
	{
		bool same =
			one.A == two.A &&
			one.R == two.R &&
			one.G == two.G &&
			one.B == two.B
		;
		return same;
	}

	public static ComponentDistance CanvasDistance(ICanvas one, ICanvas two,
		IMetric metric = null, bool excludeAlpha = false)
	{
		var black = new ColorRGBA(0.0, 0.0, 0.0, 1.0);
		int mw = Math.Max(one.Width,two.Width);
		int mh = Math.Max(one.Height,two.Height);
		double dR = 0.0, dG = 0.0, dB = 0.0, dA = 0.0;

		double total = 0.0;
		for(int y = 0; y < mh; y++) {
			for(int x = 0; x < mw; x++) {
				var pOne = one.Width > x && one.Height > y ? one[x,y] : black;
				var pTwo = two.Width > x && two.Height > y ? two[x,y] : black;

				var dist = ColorDistance(pOne,pTwo,metric,excludeAlpha);
				dR += dist.R;
				dG += dist.G;
				dB += dist.B;
				dA += dist.A;
				total += dist.Total;
			}
		}

		return new ComponentDistance {
			R = dR, G = dG, B = dB, A = dA, Total = total
		};
	}

	public static ComponentDistance ColorDistance(ColorRGBA one, ColorRGBA two,
		IMetric metric = null, bool excludeAlpha = false)
	{
		double dr = one.R - two.R;
		double dg = one.G - two.G;
		double db = one.B - two.B;
		double da = one.A - two.A;
		double total;

		if (metric != null) {
			var oneList = excludeAlpha
				? new [] { one.R, one.G, one.B }
				: new [] { one.R, one.G, one.B, one.A }
			;
			var twoList = excludeAlpha
				? new [] { two.R, two.G, two.B }
				: new [] { two.R, two.G, two.B, two.A }
			;
			total = metric.Measure(oneList,twoList);
		}
		else {
			total = excludeAlpha
				? Math.Sqrt(dr*dr + dg*dg + db*db)
				: Math.Sqrt(dr*dr + dg*dg + db*db + da*da)
			;
		}

		return new ComponentDistance {
			R = dr, G = dg, B = db, A = da, Total = total
		};
	}

	public readonly record struct ComponentDistance
	{
		public double R { get; init; }
		public double G { get; init; }
		public double B { get; init; }
		public double A { get; init; }
		public double Total { get; init; }
	}

	public static double Max(this IMetric metric, bool excludeAlpha = false)
	{
		if (metric == null) {
			return excludeAlpha ? Math.Sqrt(3.0) : 2.0;
		}

		var one = excludeAlpha ? new[] { 0.0,0.0,0.0 } : new[] { 0.0,0.0,0.0,0.0 };
		var two = excludeAlpha ? new[] { 1.0,1.0,1.0 } : new[] { 1.0,1.0,1.0,1.0 };
		var max = metric.Measure(one,two);
		return max;
	}
}