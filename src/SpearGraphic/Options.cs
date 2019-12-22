using System;

namespace ImageFunctions.SpearGraphic
{
	public enum Graphic
	{
		None = 0,
		First_Twist1,
		First_Twist2,
		First_Twist3,
		Second,
		Third,
		Fourth
	}

	public class Options
	{
		public Graphic Spear = Graphic.None;
	}
}