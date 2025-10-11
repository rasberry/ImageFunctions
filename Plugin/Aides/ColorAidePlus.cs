using ImageFunctions.Core;

namespace ImageFunctions.Plugin.Aides;

internal static class ColorAidePlus
{
	public static ColorRGBA Magenta { get { return new ColorRGBA(1.0, 0.0, 1.0, 1.0); } }
	public static ColorRGBA IndianRed { get { return ColorRGBA.FromRGBA255(0xCD, 0x5C, 0x5C, 0xFF); } }
	public static ColorRGBA LimeGreen { get { return ColorRGBA.FromRGBA255(0x32, 0xCD, 0x32, 0xFF); } }

	/// <summary>
	/// Produces a random color
	/// </summary>
	/// <param name="rnd">Intance of the Random</param>
	/// <param name="fixedAlpha">Set the alpha to a fixed value instead of random</param>
	/// <param name="blackInclusive">Make black inclusive instead of white</param>
	/// <returns></returns>
	public static ColorRGBA RandomColor(this Random rnd, double? fixedAlpha = null, bool blackInclusive = false)
	{
		double r = rnd.NextDouble();
		double g = rnd.NextDouble();
		double b = rnd.NextDouble();
		double a = fixedAlpha ?? (blackInclusive ? rnd.NextDouble() : 1.0 - rnd.NextDouble());

		return blackInclusive
			? new ColorRGBA(r, g, b, a)
			: new ColorRGBA(1.0 - r, 1.0 - g, 1.0 - b, a)
		;
	}
}
