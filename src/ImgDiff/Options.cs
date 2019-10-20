using SixLabors.ImageSharp;

namespace ImageFunctions.ImgDiff
{
	public class Options
	{
		public double? HilightOpacity = null;
		public bool MatchSamePixels = false;
		public bool OutputOriginal = false;
		public Color HilightColor = Color.Magenta;
		public string CompareImage = null;
	}
}