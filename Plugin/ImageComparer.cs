using ImageFunctions.Core;

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

	public static double CanvasDistance(ICanvas one, ICanvas two)
	{
		var black = new ColorRGBA(0.0, 0.0, 0.0, 1.0);
		int mw = Math.Max(one.Width,two.Width);
		int mh = Math.Max(one.Height,two.Height);

		double total = 0.0;
		for(int y = 0; y < mh; y++) {
			for(int x = 0; x < mw; x++) {
				var pOne = one.Width > x && one.Height > y ? one[x,y] : black;
				var pTwo = two.Width > x && two.Height > y ? two[x,y] : black;
				double dist = ColorDistance(pOne,pTwo);
				//Log.Debug($"one={pOne}, two={pTwo}");
				total += dist;
			}
		}
		return total;
	}

	public static double ColorDistance(ColorRGBA one, ColorRGBA two)
	{
		double dr = one.R - two.R;
		double dg = one.G - two.G;
		double db = one.B - two.B;
		double da = one.A - two.A;
		return Math.Sqrt(dr*dr + dg*dg + db*db + da*da);
	}
}