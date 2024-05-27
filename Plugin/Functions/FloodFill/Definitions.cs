namespace ImageFunctions.Plugin.Functions.FloodFill;

public enum FillMethodKind
{
	BreadthFirst,
	DepthFirst,
}

public enum PixelMapKind
{
	Horizontal = 1,
	Vertical = 2,
	Random = 3,
	Coordinate = 4
	//CornerTopLeft, //not sure how to calc this beyond the triangle part
	//CornerTopRight,
	//CornerBottomLeft,
	//CornerBottomRight,
	//SpiralIn, //not sure how to calc this beyond the square part
	//SpiralOut,
}
