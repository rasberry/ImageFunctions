using System;

namespace ImageFunctions.AllColors
{
	public enum Pattern {
		None = 0,
		BitOrder = 1,
		AERT,
		HSP,
		WCAG2,
		SMPTE240M,
		Luminance709,
		Luminance601,
		Luminance2020,
	}

	public enum Space {
		None = 0,
		RGB,
		HSV,
		HSL,
		HSI,
		YCbCr,
		//CieLab,
		//CieLch,
		//CieLchuv,
		//CieLuv,
		//CieXyy,
		CieXyz,
		Cmyk,
		//HunterLab,
		//LinearRgb,
		//Lms
	}

	public class Options
	{
		public Pattern SortBy = Pattern.None;
		public Space WhichSpace = Space.None;
		public int[] Order = null;
		public const int FourKWidth = 4096;
		public const int FourKHeight = 4096;
		public bool NoParallelSort = false;
	}
}