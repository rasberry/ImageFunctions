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

	public enum Space {
		None = 0,
		RGB,
		HSV,
		HSL,
		HSI,
		YCbCr,
		CieLab,
		CieLch,
		CieLchuv,
		CieLuv,
		CieXyy,
		CieXyz,
		Cmyk,
		HunterLab,
		LinearRgb,
		Lms
	}

	public enum Component {
		None = 0,
		First = 1,
		Second = 2,
		Third = 3,
		Fourth = 4
	}

	public class Options
	{
		public Pattern SortBy = Pattern.None;
		public Space WhichSpace = Space.None;
		public Component WhichComp = Component.None;
	}
}