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

	public enum PickColor {
		None = 0,
		Back = 1,
		Prime = 2,
		Comp = 3,
		Prime2 = 4
	}

	public class Options
	{
		public bool ColorComposites = false;
		public bool ColorPrimesBy6m = false;
		public bool ColorPrimesForce = false;
		public int? CenterX = null;
		public int? CenterY = null;
		public int Spacing = 1;
		public double DotSize = 1.0;
		public PickMapping Mapping = PickMapping.None;
		public Color? Color1 = null;
		public Color? Color2 = null;
		public Color? Color3 = null;
		public Color? Color4 = null;
	}
}