using System;

namespace ImageFunctions.ImgDiff
{
	public class Options
	{
		public double? HilightOpacity = null;
		public bool MatchSamePixels = false;
		public bool OutputOriginal = false;
		public IColor HilightColor = Helpers.ColorHelpers.Magenta;
		public string CompareImage = null;
	}
}