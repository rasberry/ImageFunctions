using ImageFunctions.Core;

namespace ImageFunctions.Plugin.Aides;

internal static class ColorAide
{
	public static ColorRGBA Magenta { get { return new ColorRGBA(1.0, 0.0, 1.0, 1.0); } }
	public static ColorRGBA IndianRed { get { return ColorRGBA.FromRGBA255(0xCD, 0x5C, 0x5C, 0xFF); } }
	public static ColorRGBA LimeGreen { get { return ColorRGBA.FromRGBA255(0x32, 0xCD, 0x32, 0xFF); } }
}
