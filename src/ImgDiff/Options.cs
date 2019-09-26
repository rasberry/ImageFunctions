using SixLabors.ImageSharp.PixelFormats;

namespace ImageFunctions.ImgDiff
{
	public class Options
	{
		public double? HilightOpacity = null;
		public bool MatchSamePixels = false;
		public bool OutputOriginal = false;
		public Rgba32 HilightColor = NamedColors<Rgba32>.Magenta;
		public string CompareImage = null;
	}
}