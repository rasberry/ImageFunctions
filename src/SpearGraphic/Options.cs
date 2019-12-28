using System;
using SixLabors.ImageSharp;

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
		Third_Twist1,
		Fourth
	}

	public class Options
	{
		public Graphic Spear = Graphic.None;
		public Color BackgroundColor = Color.Transparent;
	}
}