using ImageFunctions.Core;

namespace ImageFunctions.Plugin;

internal static class PlugColors
{
	public static ColorRGBA White       { get { return new ColorRGBA(1.0,1.0,1.0,1.0); }}
	public static ColorRGBA Black       { get { return new ColorRGBA(0.0,0.0,0.0,1.0); }}
	public static ColorRGBA Transparent { get { return new ColorRGBA(0.0,0.0,0.0,0.0); }}
	public static ColorRGBA Magenta     { get { return new ColorRGBA(1.0,0.0,1.0,1.0); }}
}