using ImageFunctions.Core;

namespace ImageFunctions.Plugin.Functions.FloodFill;

public enum FillMethodKind
{
	BreadthFirst,
	DepthFirst,
}

public enum PixelMapKind
{
	Horizontal,
	Vertical,
	//CornerTopLeft, //not sure how to calc this beyond the triangle part
	//CornerTopRight,
	//CornerBottomLeft,
	//CornerBottomRight,
	//SpiralIn, //not sure how to calc this beyond the square part
	//SpiralOut,
	Random,
	Coordinate
}