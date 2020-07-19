using System;
using ImageFunctions.Helpers;

namespace ImageFunctions.SpearGraphic
{
	public enum Graphic
	{
		None = 0,
		First_Twist1,
		First_Twist2,
		First_Twist3,
		Second_Twist3a,
		Second_Twist3b,
		Second_Twist3c,
		Second_Twist4,
		Third,
		Fourth
	}

	public class Options
	{
		public Graphic Spear = Graphic.None;
		public IColor BackgroundColor = ColorHelpers.Transparent;
		public int? RandomSeed = null;
	}
}