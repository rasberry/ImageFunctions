/*
using System.Drawing;
using ImgAide = ImageFunctions.Core.Aides.ImageAide;

namespace ImageFunctions.Core.Functions.Polygon;

class PolyMABlackBook
{
	public PolyMABlackBook(ICanvas canvas, List<Point> points, ColorRGBA color)
	{
		Canvas = canvas;
		Points = points;
		Color = color;
	}

	readonly ICanvas Canvas;
	readonly List<Point> Points;
	readonly ColorRGBA Color;

	void DrawFilledPoly(int XOffset, int YOffset)
	{
		// Pointers to global edge table (GET) and active edge table (AET)
		EdgeState GETPtr = null;
		EdgeState AETPtr;

		var EdgeTableBuffer = new EdgeState[Points.Count];
		int CurrentY;

		// It takes a minimum of 3 vertices to cause any pixels to be
		// drawn; reject polygons that are guaranteed to be invisible
		if (Points.Count < 3) { return; }
		// Build the global edge table
		BuildGET(EdgeTableBuffer, XOffset, YOffset);
		// Scan down through the polygon edges, one scan line at a time,
		// so long as at least one edge remains in either the GET or AET
		AETPtr = null;
		CurrentY = GETPtr.StartY; // start at the top polygon vertex
		while (GETPtr != null || AETPtr != null) {
			MoveXSortedToAET(CurrentY);  // update AET for this scan line
			ScanOutAET(CurrentY, Color); // draw this scan line from AET
			AdvanceAET();                // advance AET edges 1 scan line
			XSortAET();                  // resort on X
			CurrentY++;                  // advance to the next scan line
		}

		// Creates a GET in the buffer pointed to by NextFreeEdgeStruc from
		// the vertex list. Edge endpoints are flipped, if necessary, to
		// guarantee all edges go top to bottom. The GET is sorted primarily
		// by ascending Y start coordinate, and secondarily by ascending X
		// start coordinate within edges with common Y coordinates.
		static void BuildGET(EdgeState[] NextFreeEdge, int XOffset, int YOffset) {
			
		}
	
	
	}
	
	class EdgeState
	{
		public int X;
		public int StartY;
		public int WholePixelXMove;
		public int XDirection;
		public int ErrorTerm;
		public int ErrorTermAdjUp;
		public int ErrorTermAdjDown;
		public int Count;
	}

	void DrawHorizontalLineSeg(int y, int leftx, int rightx)
	{
		for(int x = leftx; x <= rightx; x++) {
			ImgAide.SetPixelSafe(this.Canvas, x, y, this.Color);
		}
	}
}
*/
