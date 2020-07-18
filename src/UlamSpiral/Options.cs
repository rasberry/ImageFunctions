using System;

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

	public enum PickDot {
		None = 0,
		Blob = 1,
		Circle = 2,
		Square = 3
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
		public PickDot WhichDot = PickDot.None;
		public PickMapping Mapping = PickMapping.None;
		public IColor? Color1 = null;
		public IColor? Color2 = null;
		public IColor? Color3 = null;
		public IColor? Color4 = null;
	}
}