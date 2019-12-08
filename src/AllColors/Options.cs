using System;

namespace ImageFunctions.AllColors
{
	public enum Pattern {
		None = 0,
		BitOrder = 1,
		AERT = 2,
		HSP = 3,
		WCAG2 = 4,
		SMPTE240M = 5,
		VofHSV = 6,
		IofHSI = 7,
		LofHSL = 8,
		Luminance709 = 9,
		Luminance601 = 10,
		Luminance2020 = 11,
	}

	public class Options
	{
		public Pattern SortBy = Pattern.None;
	}
}