namespace ImageFunctions.Core.Aides;

public static class ColorAide
{
	public static ColorRGBA White { get { return new ColorRGBA(1.0, 1.0, 1.0, 1.0); } }
	public static ColorRGBA Black { get { return new ColorRGBA(0.0, 0.0, 0.0, 1.0); } }
	public static ColorRGBA Transparent { get { return new ColorRGBA(0.0, 0.0, 0.0, 0.0); } }

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
}
