using ImageFunctions.Core.Aides;
using System.Drawing;
using ImgAide = ImageFunctions.Core.Aides.ImageAide;

namespace ImageFunctions.Core.Functions.Line;

public class Function : IFunction
{
	public static IFunction Create(IFunctionContext context)
	{
		if (context == null) {
			throw Squeal.ArgumentNull(nameof(context));
		}

		var f = new Function {
			Context = context,
			O = new(context)
		};
		return f;
	}
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb, Context.Register);
	}

	public IOptions Options { get { return O; } }
	IFunctionContext Context;
	Options O;

	public bool Run(string[] args)
	{
		if(Context.Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!Options.ParseArgs(args, Context.Register)) {
			return false;
		}

		var canvas = Context.Layers.First().Canvas;
		if(O.PointList == null || O.PointList.Count < 2) {
			return true; //nothing to do
		}

		//all Draw function have the same signature, so choose the one to use
		Action<ICanvas, ColorRGBA, Point, Point> DrawMethod = O.Kind switch {
			Line.Options.LineKind.DDA => DrawLineDDA,
			Line.Options.LineKind.Bresenham => DrawBresenham,
			Line.Options.LineKind.XiaolinWu => DrawXiaolinWu,
			Line.Options.LineKind.WuBlackBook => DrawWuBlackBook,
			_ => DrawRunLengthSlice,
		};

		// draw lines until we run out of points
		int pCount = O.PointList.Count;
		for(int p = 1; p < pCount; p++) {
			var sp = O.PointList[p - 1];
			var ep = O.PointList[p];
			DrawMethod(canvas, O.Color, sp, ep);
		}

		return true;
	}

	//https://en.wikipedia.org/wiki/Digital_differential_analyzer_(graphics_algorithm)
	void DrawLineDDA(ICanvas canvas, ColorRGBA color, Point p1, Point p2)
	{
		var dx = Math.Abs(p1.X - p2.X);
		var dy = Math.Abs(p1.Y - p2.Y);

		bool swap = dy > dx; //it's better to travel along the longer dimension
		double m = swap ? (double)dx / dy : (double)dy / dx;
		var s = swap ? p1.Y < p2.Y ? p1.Y : p2.Y : p1.X < p2.X ? p1.X : p2.X;
		var e = swap ? p1.Y < p2.Y ? p2.Y : p1.Y : p1.X < p2.X ? p2.X : p1.X;
		var b = swap ? p1.X < p2.X ? p1.X : p2.X : p1.Y < p2.Y ? p1.Y : p2.Y;

		double mb = b; //keep track of the other dimension accurately
		for(int a = s; a <= e; a++) {
			b = (int)Math.Round(mb);
			if(swap) {
				ImgAide.SetPixelSafe(canvas, b, a, color);
			}
			else {
				ImgAide.SetPixelSafe(canvas, a, b, color);
			}
			//Log.Debug($"[{a},{b}] mb={mb} m={m}");
			mb += m;
		}
	}

	// https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
	void DrawBresenham(ICanvas canvas, ColorRGBA color, Point p1, Point p2)
	{
		int dx = Math.Abs(p2.X - p1.X);
		int sx = p1.X < p2.X ? 1 : -1;
		int dy = -1 * Math.Abs(p2.Y - p1.Y);
		int sy = p1.Y < p2.Y ? 1 : -1;

		int error = dx + dy;
		int x = p1.X, y = p1.Y;
		while(true) {
			ImgAide.SetPixelSafe(canvas, x, y, color);
			//Log.Debug($"[{x},{y}] err={error}");
			if(x == p2.X && y == p2.Y) { break; }
			int e2 = 2 * error;
			if(e2 >= dy) {
				if(x == p2.X) { break; }
				error += dy;
				x += sx;
			}
			if(e2 <= dx) {
				if(y == p2.Y) { break; }
				error += dx;
				y += sy;
			}
		}
	}

	// https://en.wikipedia.org/wiki/Xiaolin_Wu%27s_line_algorithm
	void DrawXiaolinWu(ICanvas canvas, ColorRGBA color, Point p1, Point p2)
	{
		//c is the pixel brightness [0.0,1.0]
		void plot(double x, double y, double c)
		{
			var cc = ImgAide.GetPixelSafe(canvas, (int)x, (int)y);
			var bc = ColorAide.BetweenColor(cc, color, c);
			ImgAide.SetPixelSafe(canvas, (int)x, (int)y, bc);
		}
		double fpart(double n)
		{
			return n - Math.Floor(n);
		}
		double rfpart(double n)
		{
			return 1.0 - fpart(n);
		}

		double x0 = p1.X, x1 = p2.X;
		double y0 = p1.Y, y1 = p2.Y;
		bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
		if(steep) {
			(x0, y0) = (y0, x0);
			(x1, y1) = (y1, x1);
		}
		if(x0 > x1) {
			(x0, x1) = (x1, x0);
			(y0, y1) = (y1, y0);
		}

		double dx = x1 - x0;
		double dy = y1 - y0;
		double gradient = dx == 0.0 ? 1.0 : dy / dx;

		// handle first endpoint
		double xend = Math.Round(x0);
		double yend = y0 + gradient * (xend - x0);
		double xgap = rfpart(x0 + 0.5);
		double xpxl1 = xend; // this will be used in the main loop
		double ypxl1 = Math.Floor(yend);
		if(steep) {
			plot(ypxl1, xpxl1, rfpart(yend) * xgap);
			plot(ypxl1 + 1, xpxl1, fpart(yend) * xgap);
		}
		else {
			plot(xpxl1, ypxl1, rfpart(yend) * xgap);
			plot(xpxl1, ypxl1 + 1, fpart(yend) * xgap);
		}
		double intery = yend + gradient; // first y-intersection for the main loop

		// handle second endpoint
		xend = Math.Round(x1);
		yend = y1 + gradient * (xend - x1);
		xgap = fpart(x1 + 0.5);
		double xpxl2 = xend; // this will be used in the main loop
		double ypxl2 = Math.Floor(yend);
		if(steep) {
			plot(ypxl2, xpxl2, rfpart(yend) * xgap);
			plot(ypxl2 + 1, xpxl2, fpart(yend) * xgap);
		}
		else {
			plot(xpxl2, ypxl2, rfpart(yend) * xgap);
			plot(xpxl2, ypxl2 + 1, fpart(yend) * xgap);
		}

		// main loop
		if(steep) {
			for(double x = xpxl1 + 1; x <= xpxl2 - 1; x++) {
				plot(Math.Floor(intery), x, rfpart(intery));
				plot(Math.Floor(intery) + 1, x, fpart(intery));
				intery += gradient;
			}
		}
		else {
			for(double x = xpxl1 + 1; x <= xpxl2 - 1; x++) {
				plot(x, Math.Floor(intery), rfpart(intery));
				plot(x, Math.Floor(intery) + 1, fpart(intery));
				intery += gradient;
			}
		}
	}

	// https://www.phatcode.net/res/224/files/html/ch36/36-03.html
	void DrawRunLengthSlice(ICanvas canvas, ColorRGBA color, Point p1, Point p2)
	{
		int x = p1.X, y = p1.Y, xEnd = p2.X, yEnd = p2.Y;
		int adjUp, adjDown, errorTerm, xAdvance, xDelta, yDelta;
		int wholeStep, initialPixelCount, finalPixelCount, runLength;

		if(y > yEnd) {
			(y, yEnd) = (yEnd, y);
			(x, xEnd) = (xEnd, x);
		}

		// Figure out whether we’re going left or right, and how far we’re going horizontally
		xDelta = xEnd - x;
		if(xDelta < 0) {
			xAdvance = -1;
			xDelta = -xDelta;
		}
		else {
			xAdvance = 1;
		}

		// Figure out how far we’re going vertically 
		yDelta = yEnd - y;

		//Special-case horizontal, vertical, and diagonal lines, for speed
		// and to avoid nasty boundary conditions and division by 0
		if(xDelta == 0) { // Vertical line
			for(int i = 0; i <= yDelta; i++) {
				ImgAide.SetPixelSafe(canvas, x, y, color);
				y++;
			}
			return;
		}
		if(yDelta == 0) { // Horizontal line
			for(int i = 0; i <= xDelta; i++) {
				ImgAide.SetPixelSafe(canvas, x, y, color);
				x += xAdvance;
			}
			return;
		}
		if(xDelta == yDelta) { // Diagonal line
			for(int i = 0; i <= xDelta; i++) {
				ImgAide.SetPixelSafe(canvas, x, y, color);
				x += xAdvance;
				y++;
			}
			return;
		}

		// Draws a horizontal run of pixels, then advances the bitmap pointer to the first pixel of the next run.
		void DrawHorizontalRun(int runLengthH)
		{
			for(int i = 0; i < runLengthH; i++) {
				ImgAide.SetPixelSafe(canvas, x, y, color);
				x += xAdvance;
			}
			// Advance to the next scan line
			y++;
		}

		// Draws a vertical run of pixels, then advances the bitmap pointer to the first pixel of the next run. 
		void DrawVerticalRun(int runLengthV)
		{
			for(int i = 0; i < runLengthV; i++) {
				ImgAide.SetPixelSafe(canvas, x, y, color);
				y++;
			}
			// Advance to the next column 
			x += xAdvance;
		}

		// Determine whether the line is X or Y major, and handle accordingly
		if(xDelta >= yDelta) {
			// X major line
			// Minimum # of pixels in a run in this line 
			wholeStep = xDelta / yDelta;
			// Error term adjust each time Y steps by 1; used to tell when one
			// extra pixel should be drawn as part of a run, to account for
			// fractional steps along the X axis per 1-pixel steps along Y
			adjUp = (xDelta % yDelta) * 2;
			// Error term adjust when the error term turns over, used to factor
			// out the X step made at that time 
			adjDown = yDelta * 2;
			//Initial error term; reflects an initial step of 0.5 along the Y axis
			errorTerm = (xDelta % yDelta) - (yDelta * 2);
			// The initial and last runs are partial, because Y advances only 0.5
			// for these runs, rather than 1. Divide one full run, plus the
			// initial pixel, between the initial and last runs
			initialPixelCount = (wholeStep / 2) + 1;
			finalPixelCount = initialPixelCount;
			// If the basic run length is even and there’s no fractional
			// advance, we have one pixel that could go to either the initial
			// or last partial run, which we’ll arbitrarily allocate to the
			// last run
			if(adjUp == 0 && (wholeStep & 0x01) == 0) {
				initialPixelCount--;
			}
			// If there’re an odd number of pixels per run, we have 1 pixel that can’t
			// be allocated to either the initial or last partial run, so we’ll add 0.5
			// to error term so this pixel will be handled by the normal full-run loop
			if((wholeStep & 0x01) != 0) {
				errorTerm += yDelta;
			}
			// Draw the first, partial run of pixels
			DrawHorizontalRun(initialPixelCount);
			// Draw all full runs 
			for(int i = 0; i < yDelta - 1; i++) {
				runLength = wholeStep; // run is at least this long
									   // Advance the error term and add an extra pixel if the error term so indicates 
				errorTerm += adjUp;
				if(errorTerm > 0) {
					runLength++;
					errorTerm -= adjDown; //reset the error term 
				}
				// Draw this scan line’s run 
				DrawHorizontalRun(runLength);
			}
			// Draw the final run of pixels
			DrawHorizontalRun(finalPixelCount);
			return;
		}
		else {
			// Y major line
			// Minimum # of pixels in a run in this line 
			wholeStep = yDelta / xDelta;
			// Error term adjust each time X steps by 1; used to tell when 1 extra
			// pixel should be drawn as part of a run, to account for
			// fractional steps along the Y axis per 1-pixel steps along X
			adjUp = (yDelta % xDelta) * 2;
			// Error term adjust when the error term turns over, used to factor
			// out the Y step made at that time 
			adjDown = xDelta * 2;
			// Initial error term; reflects initial step of 0.5 along the X axis
			errorTerm = (yDelta % xDelta) - (xDelta * 2);
			// The initial and last runs are partial, because X advances only 0.5
			// for these runs, rather than 1. Divide one full run, plus the
			// initial pixel, between the initial and last runs 
			initialPixelCount = (wholeStep / 2) + 1;
			finalPixelCount = initialPixelCount;
			// If the basic run length is even and there’s no fractional advance, we
			// have 1 pixel that could go to either the initial or last partial run,
			// which we’ll arbitrarily allocate to the last run
			if(adjUp == 0 && ((wholeStep & 0x01) == 0)) {
				initialPixelCount--;
			}
			// If there are an odd number of pixels per run, we have one pixel
			// that can’t be allocated to either the initial or last partial
			// run, so we’ll add 0.5 to the error term so this pixel will be
			// handled by the normal full-run loop
			if((wholeStep & 0x01) != 0) {
				errorTerm += xDelta;
			}
			// Draw the first, partial run of pixels
			DrawVerticalRun(initialPixelCount);
			// Draw all full runs
			for(int i = 0; i < xDelta - 1; i++) {
				runLength = wholeStep; // run is at least this long
									   // Advance the error term and add an extra pixel if the error term so indicates 
				errorTerm += adjUp;
				if(errorTerm > 0) {
					runLength++;
					errorTerm -= adjDown; // reset the error term
				}
				// Draw this scan line’s run
				DrawVerticalRun(runLength);
			}
			// Draw the final run of pixels
			DrawVerticalRun(finalPixelCount);
			return;
		}
	}

	// https://www.phatcode.net/res/224/files/html/ch42/42-02.html
	void DrawWuBlackBook(ICanvas canvas, ColorRGBA color, Point p1, Point p2)
	{
		int numLevels = 256;
		int intensityBits = 8;

		void DrawPixel(int x, int y, int shift)
		{
			//Log.Debug($"{x} {y} {shift}");
			if(shift < 1) {
				ImgAide.SetPixelSafe(canvas, x, y, color);
			}
			else {
				double pct = (numLevels - shift) / (double)numLevels;
				var cc = ImgAide.GetPixelSafe(canvas, x, y);
				var bc = ColorAide.BetweenColor(cc, color, pct);
				ImgAide.SetPixelSafe(canvas, x, y, bc);
			}
		}

		int y0 = p1.Y, y1 = p2.Y;
		int x0 = p1.X, x1 = p2.X;

		//using ushort since it appears that the code was written in a time where int was 16 bits
		ushort errorAdj, errorAcc;
		ushort errorAccTemp, weighting, weightingComplementMask;
		int intensityShift, deltaX, deltaY, xDir, baseColor = 0;

		// Make sure the line runs top to bottom
		if(y0 > y1) {
			(y0, y1) = (y1, y0);
			(x0, x1) = (x1, x0);
		}
		// Draw the initial pixel, which is always exactly intersected by
		// the line and so needs no weighting
		DrawPixel(x0, y0, baseColor);

		deltaX = x1 - x0;
		if(deltaX >= 0) {
			xDir = 1;
		}
		else {
			xDir = -1;
			deltaX = -deltaX; // make DeltaX positive
		}
		// Special-case horizontal, vertical, and diagonal lines, which
		// require no weighting because they go right through the center of
		// every pixel
		deltaY = y1 - y0;
		if(deltaY == 0) {
			// Horizontal line
			while(deltaX-- != 0) {
				x0 += xDir;
				DrawPixel(x0, y0, baseColor);
			}
			return;
		}
		if(deltaX == 0) {
			// Vertical line
			do {
				y0++;
				DrawPixel(x0, y0, baseColor);
			} while(--deltaY != 0);
			return;
		}
		if(deltaX == deltaY) {
			// Diagonal line
			do {
				x0 += xDir;
				y0++;
				DrawPixel(x0, y0, baseColor);
			} while(--deltaY != 0);
			return;
		}
		// line is not horizontal, diagonal, or vertical
		errorAcc = 0;  // initialize the line error accumulator to 0
					   // # of bits by which to shift ErrorAcc to get intensity level
		intensityShift = 16 - intensityBits;
		// Mask used to flip all bits in an intensity weighting, producing the
		// result (1 - intensity weighting)
		weightingComplementMask = (ushort)(numLevels - 1);
		// Is this an X-major or Y-major line?
		if(deltaY > deltaX) {
			// Y-major line; calculate 16-bit fixed-point fractional part of a
			// pixel that X advances each time Y advances 1 pixel, truncating the
			// result so that we won't overrun the endpoint along the X axis
			errorAdj = (ushort)((deltaX << 16) / deltaY);
			// Draw all pixels other than the first and last
			while(--deltaY != 0) {
				errorAccTemp = errorAcc; // remember currrent accumulated error
				errorAcc += errorAdj; // calculate error for next pixel
				if(errorAcc <= errorAccTemp) {
					// The error accumulator turned over, so advance the X coord
					x0 += xDir;
				}
				y0++; // Y-major, so always advance Y
					  // The IntensityBits most significant bits of ErrorAcc give us the
					  // intensity weighting for this pixel, and the complement of the
					  // weighting for the paired pixel
				weighting = (ushort)(errorAcc >> intensityShift);
				DrawPixel(x0, y0, (int)(baseColor + weighting));
				DrawPixel(x0 + xDir, y0, (int)(baseColor + (weighting ^ weightingComplementMask)));
			}
		}
		else {
			// It's an X-major line; calculate 16-bit fixed-point fractional part of a
			// pixel that Y advances each time X advances 1 pixel, truncating the
			// result to avoid overrunning the endpoint along the X axis */
			errorAdj = (ushort)((deltaY << 16) / deltaX);
			/* Draw all pixels other than the first and last */
			while(--deltaX != 0) {
				errorAccTemp = errorAcc; // remember currrent accumulated error
				errorAcc += errorAdj; // calculate error for next pixel
				if(errorAcc <= errorAccTemp) {
					// The error accumulator turned over, so advance the Y coord
					y0++;
				}
				x0 += xDir; // X-major, so always advance X
							// The IntensityBits most significant bits of ErrorAcc give us the
							// intensity weighting for this pixel, and the complement of the
							// weighting for the paired pixel
				weighting = (ushort)(errorAcc >> intensityShift);
				DrawPixel(x0, y0, baseColor + weighting);
				DrawPixel(x0, y0 + 1, baseColor + (weighting ^ weightingComplementMask));
			}
		}
		// Draw the final pixel, which is always exactly intersected by the line
		// and so needs no weighting
		DrawPixel(x1, y1, baseColor);
	}
}
