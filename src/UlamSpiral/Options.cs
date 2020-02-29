using System;
using SixLabors.ImageSharp;

namespace ImageFunctions.UlamSpiral
{
	public enum PickMapping {
		None = 0,
		Linear = 1,
		Diagonal = 2,
		Spiral = 3
	}

	public class Options
	{
		public bool UseFactorCount = false;
		public int? CenterX = null;
		public int? CenterY = null;
		public int Spacing = 1;
		public double DotSize = 1.0;
		public PickMapping Mapping = PickMapping.None;
		public Color ColorPrime = Color.White;
		public Color ColorComposite = Color.White;
		public Color ColorBack = Color.Black;
	}
}