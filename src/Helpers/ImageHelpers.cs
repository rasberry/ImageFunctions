using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace ImageFunctions.Helpers
{
	public static class ImageHelpers
	{
		/*
		public static void SaveAsPng(string fileName, Image image)
		{
			PngEncoder encoder = new PngEncoder();
			encoder.CompressionLevel = 9;
			image.Save(fileName,encoder);
		}
		*/

		/*
		public static TPixel ToGrayScale<TPixel>(TPixel c)
			where TPixel : struct, IPixel<TPixel>
		{
			RgbaD v = ToColor(c);
			double val = v.R * 0.2126 + v.G * 0.7152 + v.B * 0.0722;
			var vGray = new RgbaD(val,val,val,v.A);
			return vGray.FromColor<TPixel>();
		}
		*/

		public static IFColor ToGrayScale(IFColor c)
		{
			double val = c.R * 0.2126 + c.G * 0.7152 + c.B * 0.0722;
			var vGray = new IFColor(val,val,val,c.A);
			return vGray;
		}

		//(val - from.min) * ((to.max - to.min)/(from.max - from.min)) + (to.min)
		public static Color NativeToRgba(IFColor color)
		{
			return Color.FromArgb(
				(int)(color.A * 255.0),
				(int)(color.R * 255.0),
				(int)(color.G * 255.0),
				(int)(color.B * 255.0)
			);
		}

		public static IFColor RgbaToNative(Color color)
		{
			return new IFColor(
				color.R / 255.0,
				color.G / 255.0,
				color.B / 255.0,
				color.A / 255.0
			);
		}

		/*
		public static RgbaD ToColor<TPixel>(this TPixel color)
			where TPixel : struct, IPixel<TPixel>
		{
			if (color is RgbaD) {
				return (RgbaD)((IPixel<RgbaD>)color);
			}
			var c = new RgbaD();
			c.FromScaledVector4(color.ToScaledVector4());
			return c;
		}
		*/

		/*
		public static TPixel FromColor<TPixel>(this RgbaD color)
			where TPixel : struct, IPixel<TPixel>
		{
			TPixel p = default(TPixel);
			if (p is RgbaD) {
				p = (TPixel)((IPixel<RgbaD>)color);
			}
			else {
				p.FromScaledVector4(color.ToScaledVector4());
			}
			return p;
		}
		*/

		/*
		public static void BlitImage<TPixel>(this ImageFrame<TPixel> dstImg, ImageFrame<TPixel> srcImg,
			SixLabors.Primitives.Rectangle dstRect = default(SixLabors.Primitives.Rectangle),
			SixLabors.Primitives.Point srcPoint = default(SixLabors.Primitives.Point))
			where TPixel : struct, IPixel<TPixel>
		{
			//TODO this needs to be tested better

			var srcspan = srcImg.GetPixelSpan();
			var dstspan = dstImg.GetPixelSpan();
			for(int y = dstRect.Top; y < dstRect.Bottom; y++) {
				int cy = y - dstRect.Top + srcPoint.Y;
				for(int x = dstRect.Left; x < dstRect.Right; x++) {
					int cx = x - dstRect.Left + srcPoint.X;
					int dstoff = y * dstImg.Width + x;
					int srcoff = cy * dstRect.Width + cx;
					dstspan[dstoff] = srcspan[srcoff];
				}
			}
		}
		*/

		public static void BlitImage(this IFImage dstImg, IFImage srcImg,
			Rectangle dstRect = default(Rectangle),
			Point srcPoint = default(Point))
		{
			//TODO this needs to be tested better

			for(int y = dstRect.Top; y < dstRect.Bottom; y++) {
				int cy = y - dstRect.Top + srcPoint.Y;
				for(int x = dstRect.Left; x < dstRect.Right; x++) {
					int cx = x - dstRect.Left + srcPoint.X;
					int dstoff = y * dstImg.Width + x;
					int srcoff = cy * dstRect.Width + cx;
					dstImg[x,y] = srcImg[cx,cy];
				}
			}
		}

		/*
		public static TPixel GetPixelSafe<TPixel>(this ImageFrame<TPixel> img, int x, int y)
			where TPixel : struct, IPixel<TPixel>
		{
			int px = Math.Clamp(x,0,img.Width - 1);
			int py = Math.Clamp(y,0,img.Height - 1);
			int off = py * img.Width + px;
			//Log.Debug("GPS off = "+off);
			return img.GetPixelSpan()[off];
		}
		*/

		public static IFColor GetPixelSafe(this IFImage img, int x, int y)
		{
			int px = Math.Clamp(x,0,img.Width - 1);
			int py = Math.Clamp(y,0,img.Height - 1);
			return img[px,py];
		}

		/*
		public static TPixel Sample<TPixel>(this ImageFrame<TPixel> img, double locX, double locY, IResampler sampler = null)
			where TPixel : struct, IPixel<TPixel>
		{
			if (sampler == null) {
				TPixel pixn = img.GetPixelSafe((int)locX,(int)locY);
				return pixn;
			}
			else {
				TPixel pixc = SampleComplex(img,locX,locY,sampler);
				return pixc;
			}
		}
		*/

		//public static IFColor Sample(this IFImage img, double locX, double locY, IFResampler sampler = null)
		//{
		//	return sampler == null
		//		? GetPixelSafe(img,(int)locX,(int)locY)
		//		: SampleHelpers.GetSample(img,sampler,(int)locX,(int)locY)
		//	;
		//}

		/*
		static TPixel SampleComplex<TPixel>(this ImageFrame<TPixel> img, double locX, double locY, IResampler sampler = null)
			where TPixel : struct, IPixel<TPixel>
		{
			double m = sampler.Radius / 0.5; //stretch length 0.5 to Radius
			double pxf = locX.Fractional();
			double pyf = locY.Fractional();

			//mirror as necessary around center
			double fx = 0.5 + (pxf < 0.5 ? pxf : 1.0 - pxf);
			double fy = 0.5 + (pyf < 0.5 ? pyf : 1.0 - pyf);

			//dx, dy are distance from center values - "how much of the other pixel do you want?"
			//dx, dy are valued as:
			//  1.0 = 100% original pixel
			//  0.0 = 100% other pixel
			//sampler is scaled:
			//  0.0 = closest to wanted value - returns 1.0
			//  1.0 = farthest from wanted value - return 0.0
			double sx = Math.Abs(fx) * m;
			double sy = Math.Abs(fy) * m;
			float dx = sampler.GetValue((float)sx);
			float dy = sampler.GetValue((float)sy);
			//Log.Debug("sx="+sx+" dx="+dx+" sy="+sy+" dy="+dy);

			//pick and sample the 4 pixels;
			TPixel p0,p1,p2,p3;
			FillQuadrantColors<TPixel>(img, pxf < 0.5, pyf < 0.5, locX, locY, out p0, out p1, out p2, out p3);

			RgbaD v0 = p0.ToColor();
			RgbaD v1 = p1.ToColor();
			RgbaD v2 = p2.ToColor();
			RgbaD v3 = p3.ToColor();

			double Rf = CalcSample(v0.R, v1.R, v2.R, v3.R, dx, dy);
			double Gf = CalcSample(v0.G, v1.G, v2.G, v3.G, dx, dy);
			double Bf = CalcSample(v0.B, v1.B, v2.B, v3.B, dx, dy);
			double Af = CalcSample(v0.A, v1.A, v2.A, v3.A, dx, dy);

			var color = new RgbaD(Rf,Gf,Bf,Af);
			TPixel pixi = color.FromColor<TPixel>();
			//Log.Debug("pix = "+pixi);
			return pixi;
		}
		*/

		/*
		static double CalcSample(double v0,double v1,double v2,double v3,double dx, double dy)
		{
			//interpolation calc
			double h0 = (v0 * (1.0 - dx)) + (v1 * dx);
			double h1 = (v3 * (1.0 - dx)) + (v2 * dx);
			double vf = (h0 * (1.0 - dy)) + (h1 * dy);
			return vf;
		}
		*/

		/*
		static void FillQuadrantColors<TPixel>(
			ImageFrame<TPixel> img, bool xIsPos, bool yIsPos,double px, double py,
			out TPixel q0, out TPixel q1, out TPixel q2, out TPixel q3)
			where TPixel : struct, IPixel<TPixel>
		{
			int cx = (int)px;
			int cy = (int)py;
			int px0,py0,px1,py1,px2,py2,px3,py3;
			if (!xIsPos && !yIsPos) {
				px0 = cx - 1; py0 = cy - 1;
				px1 = cx + 0; py1 = cy - 1;
				px2 = cx + 0; py2 = cy + 0;
				px3 = cx - 1; py3 = cy + 0;
			}
			else if (xIsPos && !yIsPos) {
				px1 = cx + 0; py1 = cy - 1;
				px0 = cx + 1; py0 = cy - 1;
				px3 = cx + 1; py3 = cy + 0;
				px2 = cx + 0; py2 = cy + 0;
			}
			else if (xIsPos && yIsPos) {
				px2 = cx + 0; py2 = cy + 0;
				px3 = cx + 1; py3 = cy + 0;
				px0 = cx + 1; py0 = cy + 1;
				px1 = cx + 0; py1 = cy + 1;
			}
			else { // if (!xpos && ypos) {
				px3 = cx - 1; py3 = cy + 0;
				px2 = cx + 0; py2 = cy + 0;
				px1 = cx + 0; py1 = cy + 1;
				px0 = cx - 1; py0 = cy + 1;
			}

			//had to mirror the p(n) order around the x,y axes so that this part
			//  could stay the same (note the strange ordering above - q2 is always +0,+0)
			q0 = img.GetPixelSafe(px0,py0);
			q1 = img.GetPixelSafe(px1,py1);
			q2 = img.GetPixelSafe(px2,py2);
			q3 = img.GetPixelSafe(px3,py3);
		}
		*/

		/*
		//ratio 0.0 = 100% a
		//ratio 1.0 = 100% b
		public static TPixel BetweenColor<TPixel>(TPixel a, TPixel b, double ratio)
			where TPixel : struct, IPixel<TPixel>
		{
			var va = a.ToColor();
			var vb = b.ToColor();
			ratio = Math.Clamp(ratio,0.0,1.0);
			double nr = (1.0 - ratio) * va.R + ratio * vb.R;
			double ng = (1.0 - ratio) * va.G + ratio * vb.G;
			double nb = (1.0 - ratio) * va.B + ratio * vb.B;
			double na = (1.0 - ratio) * va.A + ratio * vb.A;
			var btw = new RgbaD(nr,ng,nb,na);
			// Log.Debug("between a="+a+" b="+b+" r="+ratio+" nr="+nr+" ng="+ng+" nb="+nb+" na="+na+" btw="+btw);
			return btw.FromColor<TPixel>();
		}
		*/

		//ratio 0.0 = 100% a
		//ratio 1.0 = 100% b
		public static IFColor BetweenColor(IFColor a, IFColor b, double ratio)
		{
			ratio = Math.Clamp(ratio,0.0,1.0);
			double nr = (1.0 - ratio) * a.R + ratio * b.R;
			double ng = (1.0 - ratio) * a.G + ratio * b.G;
			double nb = (1.0 - ratio) * a.B + ratio * b.B;
			double na = (1.0 - ratio) * a.A + ratio * b.A;
			var btw = new IFColor(nr,ng,nb,na);
			// Log.Debug("between a="+a+" b="+b+" r="+ratio+" nr="+nr+" ng="+ng+" nb="+nb+" na="+na+" btw="+btw);
			return btw;
		}

		public static (double,double,double) ConvertToHSI(Color c)
		{
			int max = Math.Max(c.R,Math.Max(c.G,c.B));
			int min = Math.Min(c.R,Math.Min(c.G,c.B));
			int chr = max - min;
			double H = 0.0;
			if (max == c.R) {
				H = (c.G - c.B / (double)chr) % 6.0;
			}
			else if (max == c.G) {
				H = (c.B - c.R / (double)chr) + 2.0;
			}
			else if (max == c.B) {
				H = (c.R - c.G / (double)chr) + 4.0;
			}
			double I = (c.R + c.G + c.B) / 3.0;
			double S = 1.0 - 3 * min / I;

			return (H,S,I);
		}

		/*
		public static void FillWithColor<TPixel>(ImageFrame<TPixel> frame, SixLabors.Primitives.Rectangle rect,TPixel color)
			where TPixel : struct, IPixel<TPixel>
		{
			var bounds = frame.Bounds();
			bounds.Intersect(rect);
			var span = frame.GetPixelSpan();

			for(int y=bounds.Top; y<bounds.Bottom; y++) {
				for(int x=bounds.Left; x<bounds.Right; x++) {
					int off = y * frame.Width + x;
					span[off] = color;
				}
			}
		}
		*/

		public static void FillWithColor(IFImage frame, Rectangle rect,IFColor color)
		{
			var bounds = new Rectangle(0,0,frame.Width,frame.Height);
			bounds.Intersect(rect);

			for(int y=bounds.Top; y<bounds.Bottom; y++) {
				for(int x=bounds.Left; x<bounds.Right; x++) {
					frame[x,y] = color;
				}
			}
		}

		public static void DrawPolyLine(IFImage img, PointD[] list, IFColor c, double thickness = 1.0)
		{
			double xMin = double.MaxValue,xMax = double.MinValue,
				   yMin = double.MaxValue,yMax = double.MinValue;

			foreach(PointD p in list) {
				if (p.X < xMin) { xMin = p.X; }
				if (p.X > xMax) { xMax = p.X; }
				if (p.Y < yMin) { yMin = p.Y; }
				if (p.Y > yMax) { yMax = p.Y; }
			}

			double tHalf = thickness / 2.0;
			double t = Math.Floor(yMin - tHalf) + 0.5;
			double b = Math.Floor(yMax + tHalf) + 0.5;
			double l = Math.Floor(xMin - tHalf) + 0.5;
			double r = Math.Floor(xMax + tHalf) + 0.5;

			if (t == b && l == r) {
				return; //bounding rectangle is zero
			}

			for(double y = t; y < b; y += 1.0) {
				for(double x = l; x < r; x += 1.0) {
					double mind = double.MaxValue;
					for(int p = 0; p < list.Length; p++) {
						var p0 = p == 0 ? list[list.Length - 1] : list[p-1];
						var p1 = list[p];
						double d = DistanceToLine(new PointD(x,y),p0,p1);
						if (d < mind) { mind = d; }
					}
					Plot(img,c,x,y,Math.Clamp(1.0 - mind/thickness,0.0,1.0));
				}
			}
		}

		// https://wrf.ecse.rpi.edu/Research/Short_Notes/pnpoly.html#The%20C%20Code
		public static bool IsPointInPoly(PointD point, PointD[] list)
		{
			bool c = false;
			int len = list.Length;
			for(int i = 0, j = len - 1; i < len; j = i++) {
				var ip = list[i];
				var jp = list[j];
				if (
					((ip.Y > point.Y) != (jp.Y > point.Y)) &&
					(point.X < (jp.X - ip.X) * (point.Y - ip.Y) / (jp.Y - ip.Y) + ip.X)
				)
				{ c = !c; }
			}
			return c;
		}

		// https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line#Line_defined_by_two_points
		public static double DistanceToLine(PointD pt, PointD p0, PointD p1)
		{
			double dy = p1.Y - p0.Y;
			double dx = p1.X - p0.X;
			//if line segment is zero width just do distance to point
			if (Math.Abs(dx) < double.Epsilon && Math.Abs(dy) < double.Epsilon) {
				dy = pt.Y - p0.Y;
				dx = pt.X - p0.X;
				return Math.Sqrt(dx*dx + dy*dy);
			}
			double den = Math.Sqrt(dy*dy + dx*dx);
			double num = 
				(p0.X - pt.X)*p1.Y +
				(pt.X - p1.X)*p0.Y +
				(p1.X - p0.X)*pt.Y
			;
			return num / den;
		}

/*
def plot_antialiased_point(x: float, y: float):
    for roundedx in floor(x) to ceil(x):
        for roundedy in floor(y) to ceil(y):
            percent_x = 1 - abs(x - roundedx)
            percent_y = 1 - abs(y - roundedy)
            percent = percent_x * percent_y
            draw_pixel(coordinates=(roundedx, roundedy), color=percent (range 0-1))
 */

		// https://rosettacode.org/wiki/Xiaolin_Wu%27s_line_algorithm#C.23
		public static void DrawLine2(IFImage img, IFColor c, PointD p0, PointD p1)
		{
			double x0 = p0.X, y0 = p0.Y;
			double x1 = p1.X, y1 = p1.Y;
			bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
			if (steep) {
				(x0,y0,x1,y1) = (y0,x0,y1,x1); //swap
			}
			if (x0 > x1) {
				(x0,x1,y0,y1) = (x1,x0,y1,y0); //swap
			}

			double dx = x1 - x0;
			double dy = y1 - y0;
			double grad = dy / dx;
			if (dx < double.Epsilon) { grad = 1.0; }
			double xEnd = Round(x0);
			double yEnd = y0 + grad * (xEnd - x0);
			double xGap = RFPart(x0 + 0.5);
			double xp1 = xEnd;
			double yp1 = IPart(yEnd);

			if (steep) {
				Plot(img,c,yp1,xp1,RFPart(yEnd)*xGap);
				Plot(img,c,yp1 + 1,xp1,FPart(yEnd)*xGap);
			}
			else {
				Plot(img,c,xp1,yp1,RFPart(yEnd)*xGap);
				Plot(img,c,xp1,yp1+1,FPart(yEnd)*xGap);
			}
			double intery = yEnd + grad;

			xEnd = Round(x1);
			yEnd = y1 + grad*(xEnd - x1);
			xGap = FPart(x1 + 0.5);
			double xp2 = xEnd;
			double yp2 = IPart(yEnd);

			if (steep) {
				Plot(img,c,yp2,xp2,RFPart(yEnd)*xGap);
				Plot(img,c,yp2+1.0,xp2,FPart(yEnd)*xGap);
			}
			else {
				Plot(img,c,xp2,yp2,RFPart(yEnd)*xGap);
				Plot(img,c,xp2,yp2+1.0,FPart(yEnd)*xGap);
			}

			int s = (int)(xp1 + 1.0);
			int e = (int)(xp2 - 1.0);
			if (steep) {
				for(int x = s; x <= e; x++) {
					Plot(img,c,IPart(intery),x,RFPart(intery));
					Plot(img,c,IPart(intery)+1.0,x,FPart(intery));
					intery += grad;
				}
			}
			else {
				for(int x = s; x <= e; x++) {
					Plot(img,c,x,IPart(intery),RFPart(intery));
					Plot(img,c,x,IPart(intery)+1.0,FPart(intery));
					intery += grad;
				}
			}
		}

		static double Round(double n)
		{
			return Math.Truncate(n + 0.5);
		}

		static double IPart(double n)
		{
			return Math.Truncate(n);
		}

		static double FPart(double n)
		{
			return n < 0
				? 1.0 + Math.Floor(n) - n
				: n - Math.Floor(n)
			;
		}

		static double RFPart(double n)
		{
			return 1.0 - FPart(n);
		}

		public static void DrawLine(IFImage img, Color c, PointD p0, PointD p1, double width = 1.0)		{
			var rgba = RgbaToNative(c);
			DrawLine(img,rgba,p0,p1,width);
		}

		//http://members.chello.at/easyfilter/bresenham.html
		public static void DrawLine(IFImage img, IFColor c, PointD p0, PointD p1, double width = 1.0)
		{
			int x0 = (int)p0.X, x1 = (int)p1.X, y0 = (int)p0.Y, y1 = (int)p1.Y;
			int dx = (int)Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
			int dy = (int)Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
			int err = dx - dy, e2, x2, y2; //error value e_xy
			double ed = dx + dy == 0 ? 1.0 : Math.Sqrt((double)dx * dx + (double)dy * dy);

			for(width = (width + 1.0)/2.0 ; ;) { //pixel loop
				{ //block for scoping 'a'
					double a = width - Math.Abs(err - dx + dy) / ed;
					Plot(img, c, x0, y0, Math.Clamp(a,0.0,1.0));
				}
				e2 = err; x2 = x0;
				if (2 * e2 >= -dx) { // x step
					for(e2 += dy, y2 = y0; e2 < ed * width && (y1 != y2 || dx > dy); e2 += dx) {
						double a = width - Math.Abs(e2) / ed;
						Plot(img, c, x0, y2 += sy, Math.Clamp(a,0.0,1.0));
					}
					if (x0 == x1) { break; }
					e2 = err; err -= dy; x0 += sx;
				}
				if (2 * e2 <= dy) { //y step
					for(e2 = dx - e2; e2 < ed * width && (x1 != x2 || dx < dy); e2 += dy) {
						double a = width - Math.Abs(e2) / ed;
						Plot(img, c, x2 += sx, y0, Math.Clamp(a,0.0,1.0));
					}
					if (y0 == y1) { break; }
					err += dx; y0 += sy;
				}
			}
		}

		public static void DrawLine3(IFImage img, IFColor c, PointD p0, PointD p1)
		{
			int x0 = (int)p0.X, y0 = (int)p0.Y;
			int x1 = (int)p1.X, y1 = (int)p1.Y;

			int dx = Math.Abs(x1-x0), sx = x0<x1 ? 1 : -1;
			int dy = Math.Abs(y1-y0), sy = y0<y1 ? 1 : -1; 
			int err = dx-dy, e2, x2; // error value e_xy
			double ed = dx+dy == 0 ? 1.0 : Math.Sqrt((double)dx*dx+(double)dy*dy);

			for ( ; ; ){ // pixel loop
				Plot(img,c,x0,y0,Math.Abs(err-dx+dy)/ed);
				//setPixelAA(x0,y0, 255*abs(err-dx+dy)/ed);
				e2 = err; x2 = x0;
				if (2*e2 >= -dx) { // x step
					if (x0 == x1) { break; }
					if (e2+dy < ed) {
						Plot(img,c,x0,y0+sy,1.0 - (e2+dy)/ed);
						// setPixelAA(x0,y0+sy, 255*(e2+dy)/ed);
					}
					err -= dy; x0 += sx; 
				} 
				if (2*e2 <= dy) { // y step
					if (y0 == y1) { break; }
					if (dx-e2 < ed) {
						Plot(img,c,x2+sx,y0,1.0 - (dx-e2)/ed);
						//setPixelAA(x2+sx,y0, 255*(dx-e2)/ed);
					}
					err += dx; y0 += sy; 
				}
			}

		}

		//TODO
		//look at https://www.geeksforgeeks.org/scan-line-polygon-filling-using-opengl-c/
		// ploygon scan algorith seems pretty flexible
		///
		// https://www.cs.uic.edu/~jbell/CourseNotes/ComputerGraphics/PolygonFilling.html
		// https://alienryderflex.com/polygon_fill/
		// https://imagej.nih.gov/ij/developer/source/ij/process/PolygonFiller.java.html
		
		static void Plot(IFImage img, IFColor c, double x, double y, double k)
		{
			int ix = (int)x, iy = (int)y;
			if (ix < 0 || iy < 0 || ix >= img.Width || iy >= img.Height) {
				return;
			}
			if (k >= 1.0) {
				img[ix,iy] = c; //100% opacity so replace
			}
			else if (k < double.Epsilon) {
				return; //0% opacity so keep original
			}
			else {
				//https://en.wikipedia.org/wiki/Alpha_compositing
				var o = img[ix,iy];
				double oma = (c.A * k)*(1.0 - o.A);
				double a = o.A + oma;
				double r = (o.R*o.A + c.R*oma) / a;
				double g = (o.G*o.A + c.G*oma) / a;
				double b = (o.B*o.A + c.B*oma) / a;
				img[ix,iy] = new IFColor(r,g,b,a);
			}
		}

		// https://gist.github.com/randvoorhies/807ce6e20840ab5314eb7c547899de68#file-bresenham-js-L813
		public static void DrawLine4(IFImage img, IFColor c, PointD p0, PointD p1, double th = 1.0)
		{
			double x0 = p0.X, y0 = p0.Y;
			double x1 = p1.X, y1 = p1.Y;
			double dx = Math.Abs(x1-x0), sx = x0 < x1 ? 1 : -1;
			double dy = Math.Abs(y1-y0), sy = y0 < y1 ? 1 : -1;
			double err, e2 = Math.Sqrt(dx*dx+dy*dy);

			if (th <= 1.0 || e2 < double.Epsilon) {
				DrawLine(img,c,p0,p1);
				return;
			}

			dx *= 1.0/e2; dy *= 1.0/e2; //w = 1.0*(w-1)

			if (dx < dy) {                                   // steep line
				x1 = Math.Round((e2+th/2)/dy);               // start offset
				err = x1*dy-th/2;                            // shift error value to offset width
				for (x0 -= x1*sx; ; y0 += sy) {
					Plot(img,c,x1 = x0, y0, err);            // aliasing pre-pixel
					for (e2 = dy-err-th; e2+dy < 255; e2 += dy) {
						Plot(img,c,x1 += sx, y0,1.0);        // pixel on the line
					}
					Plot(img,c,x1+sx, y0, e2);               // aliasing post-pixel
					if (y0 == y1) { break; }
					err += dx;                               // y-step
					if (err > 255) { err -= dy; x0 += sx; }  // x-step
				}
			}
			else {                                           // flat line
				y1 = Math.Round((e2+th/2)/dx);               // start offset
				err = y1*dx-th/2;                            // shift error value to offset width
				for (y0 -= y1*sy; ; x0 += sx) {
					Plot(img,c,x0, y1 = y0, err);            // aliasing pre-pixel
					for (e2 = dx-err-th; e2+dx < 255; e2 += dx) {
						Plot(img,c,x0, y1 += sy,1.0);        // pixel on the line
					}
					Plot(img,c,x0, y1+sy, e2);               // aliasing post-pixel
					if (x0 == x1) { break; }
					err += dy;                               // x-step
					if (err > 255) { err -= dx; y0 += sy; }  // y-step
				}
			}

		}

		//#if true
		//public void DrawShape(IFImage img, IFColor c)
		//{
		//	
		//}
		//
		//#endif


		/*
		
// CPP program to illustrate 
// Scanline Polygon fill Algorithm 
  
#include <stdio.h> 
#include <math.h> 
#include <GL/glut.h> 
#define maxHt 800 
#define maxWd 600 
#define maxVer 10000 
  
FILE *fp; 
  
// Start from lower left corner 
typedef struct edgebucket  
{ 
    int ymax;   //max y-coordinate of edge 
    float xofymin;  //x-coordinate of lowest edge point updated only in aet 
    float slopeinverse; 
}EdgeBucket; 
  
typedef struct edgetabletup 
{ 
    // the array will give the scanline number 
    // The edge table (ET) with edges entries sorted  
    // in increasing y and x of the lower end 
      
    int countEdgeBucket;    //no. of edgebuckets 
    EdgeBucket buckets[maxVer]; 
}EdgeTableTuple; 
  
EdgeTableTuple EdgeTable[maxHt], ActiveEdgeTuple; 
  
  
// Scanline Function 
void initEdgeTable() 
{ 
    int i; 
    for (i=0; i<maxHt; i++) 
    { 
        EdgeTable[i].countEdgeBucket = 0; 
    } 
      
    ActiveEdgeTuple.countEdgeBucket = 0; 
} 
  
  
void printTuple(EdgeTableTuple *tup) 
{ 
    int j; 
      
    if (tup->countEdgeBucket) 
        printf("\nCount %d-----\n",tup->countEdgeBucket); 
          
        for (j=0; j<tup->countEdgeBucket; j++) 
        {  
            printf(" %d+%.2f+%.2f", 
            tup->buckets[j].ymax, tup->buckets[j].xofymin,tup->buckets[j].slopeinverse); 
        } 
} 
  
void printTable() 
{ 
    int i,j; 
      
    for (i=0; i<maxHt; i++) 
    { 
        if (EdgeTable[i].countEdgeBucket) 
            printf("\nScanline %d", i); 
              
        printTuple(&EdgeTable[i]); 
    }  
} 
  
  
// Function to sort an array using insertion sort
void insertionSort(EdgeTableTuple *ett) 
{ 
    int i,j; 
    EdgeBucket temp;  
  
    for (i = 1; i < ett->countEdgeBucket; i++)  
    { 
        temp.ymax = ett->buckets[i].ymax; 
        temp.xofymin = ett->buckets[i].xofymin; 
        temp.slopeinverse = ett->buckets[i].slopeinverse; 
        j = i - 1; 
  
    while ((temp.xofymin < ett->buckets[j].xofymin) && (j >= 0))  
    { 
        ett->buckets[j + 1].ymax = ett->buckets[j].ymax; 
        ett->buckets[j + 1].xofymin = ett->buckets[j].xofymin; 
        ett->buckets[j + 1].slopeinverse = ett->buckets[j].slopeinverse; 
        j = j - 1; 
    } 
    ett->buckets[j + 1].ymax = temp.ymax; 
    ett->buckets[j + 1].xofymin = temp.xofymin; 
    ett->buckets[j + 1].slopeinverse = temp.slopeinverse; 
    } 
} 
  
  
void storeEdgeInTuple (EdgeTableTuple *receiver,int ym,int xm,float slopInv) 
{ 
    // both used for edgetable and active edge table.. 
    // The edge tuple sorted in increasing ymax and x of the lower end. 
    (receiver->buckets[(receiver)->countEdgeBucket]).ymax = ym; 
    (receiver->buckets[(receiver)->countEdgeBucket]).xofymin = (float)xm; 
    (receiver->buckets[(receiver)->countEdgeBucket]).slopeinverse = slopInv; 
              
    // sort the buckets 
    insertionSort(receiver); 
          
    (receiver->countEdgeBucket)++;  
      
      
} 
  
void storeEdgeInTable (int x1,int y1, int x2, int y2) 
{ 
    float m,minv; 
    int ymaxTS,xwithyminTS, scanline; //ts stands for to store 
      
    if (x2==x1) 
    { 
        minv=0.000000; 
    } 
    else
    { 
    m = ((float)(y2-y1))/((float)(x2-x1)); 
      
    // horizontal lines are not stored in edge table 
    if (y2==y1) 
        return; 
          
    minv = (float)1.0/m; 
    printf("\nSlope string for %d %d & %d %d: %f",x1,y1,x2,y2,minv); 
    } 
      
    if (y1>y2) 
    { 
        scanline=y2; 
        ymaxTS=y1; 
        xwithyminTS=x2; 
    } 
    else
    { 
        scanline=y1; 
        ymaxTS=y2; 
        xwithyminTS=x1;      
    } 
    // the assignment part is done..now storage.. 
    storeEdgeInTuple(&EdgeTable[scanline],ymaxTS,xwithyminTS,minv); 
      
      
} 
  
void removeEdgeByYmax(EdgeTableTuple *Tup,int yy) 
{ 
    int i,j; 
    for (i=0; i< Tup->countEdgeBucket; i++) 
    { 
        if (Tup->buckets[i].ymax == yy) 
        { 
            printf("\nRemoved at %d",yy); 
              
            for ( j = i ; j < Tup->countEdgeBucket -1 ; j++ ) 
                { 
                Tup->buckets[j].ymax =Tup->buckets[j+1].ymax; 
                Tup->buckets[j].xofymin =Tup->buckets[j+1].xofymin; 
                Tup->buckets[j].slopeinverse = Tup->buckets[j+1].slopeinverse; 
                } 
                Tup->countEdgeBucket--; 
            i--; 
        } 
    } 
}      
  
  
void updatexbyslopeinv(EdgeTableTuple *Tup) 
{ 
    int i; 
      
    for (i=0; i<Tup->countEdgeBucket; i++) 
    { 
        (Tup->buckets[i]).xofymin =(Tup->buckets[i]).xofymin + (Tup->buckets[i]).slopeinverse; 
    } 
} 
  
  
void ScanlineFill() 
{ 
   //  Follow the following rules: 
   // 1. Horizontal edges: Do not include in edge table 
   // 2. Horizontal edges: Drawn either on the bottom or on the top. 
   // 3. Vertices: If local max or min, then count twice, else count 
   //     once. 
   // 4. Either vertices at local minima or at local maxima are drawn.
  
  
    int i, j, x1, ymax1, x2, ymax2, FillFlag = 0, coordCount; 
      
    // we will start from scanline 0;  
    // Repeat until last scanline: 
    for (i=0; i<maxHt; i++)//4. Increment y by 1 (next scan line) 
    { 
          
        // 1. Move from ET bucket y to the 
        // AET those edges whose ymin = y (entering edges) 
        for (j=0; j<EdgeTable[i].countEdgeBucket; j++) 
        { 
            storeEdgeInTuple(&ActiveEdgeTuple,EdgeTable[i].buckets[j]. 
                     ymax,EdgeTable[i].buckets[j].xofymin, 
                    EdgeTable[i].buckets[j].slopeinverse); 
        } 
        printTuple(&ActiveEdgeTuple); 
          
        // 2. Remove from AET those edges for  
        // which y=ymax (not involved in next scan line) 
        removeEdgeByYmax(&ActiveEdgeTuple, i); 
          
        //sort AET (remember: ET is presorted) 
        insertionSort(&ActiveEdgeTuple); 
          
        printTuple(&ActiveEdgeTuple); 
          
        //3. Fill lines on scan line y by using pairs of x-coords from AET 
        j = 0;  
        FillFlag = 0; 
        coordCount = 0; 
        x1 = 0; 
        x2 = 0; 
        ymax1 = 0; 
        ymax2 = 0; 
        while (j<ActiveEdgeTuple.countEdgeBucket) 
        { 
            if (coordCount%2==0) 
            { 
                x1 = (int)(ActiveEdgeTuple.buckets[j].xofymin); 
                ymax1 = ActiveEdgeTuple.buckets[j].ymax; 
                if (x1==x2) 
                { 
                // three cases can arrive- 
                //  1. lines are towards top of the intersection 
                //  2. lines are towards bottom 
                //  3. one line is towards top and other is towards bottom 
                //
                    if (((x1==ymax1)&&(x2!=ymax2))||((x1!=ymax1)&&(x2==ymax2))) 
                    { 
                        x2 = x1; 
                        ymax2 = ymax1; 
                    } 
                  
                    else
                    { 
                        coordCount++; 
                    } 
                } 
                  
                else
                { 
                        coordCount++; 
                } 
            } 
            else
            { 
                x2 = (int)ActiveEdgeTuple.buckets[j].xofymin; 
                ymax2 = ActiveEdgeTuple.buckets[j].ymax;  
              
                FillFlag = 0; 
                  
                // checking for intersection... 
                if (x1==x2) 
                { 
                //three cases can arive- 
                //  1. lines are towards top of the intersection 
                //  2. lines are towards bottom 
                //  3. one line is towards top and other is towards bottom 
                //
                    if (((x1==ymax1)&&(x2!=ymax2))||((x1!=ymax1)&&(x2==ymax2))) 
                    { 
                        x1 = x2; 
                        ymax1 = ymax2; 
                    } 
                    else
                    { 
                        coordCount++; 
                        FillFlag = 1; 
                    } 
                } 
                else
                { 
                        coordCount++; 
                        FillFlag = 1; 
                }  
              
              
            if(FillFlag) 
            { 
                //drawing actual lines... 
                glColor3f(0.0f,0.7f,0.0f); 
                  
                glBegin(GL_LINES); 
                glVertex2i(x1,i); 
                glVertex2i(x2,i); 
                glEnd(); 
                glFlush();          
                  
                // printf("\nLine drawn from %d,%d to %d,%d",x1,i,x2,i); 
            } 
              
        } 
              
        j++; 
    }  
              
          
    // 5. For each nonvertical edge remaining in AET, update x for new y 
    updatexbyslopeinv(&ActiveEdgeTuple); 
} 
  
  
printf("\nScanline filling complete"); 
  
} 
  
  
void myInit(void) 
{ 
  
    glClearColor(1.0,1.0,1.0,0.0); 
    glMatrixMode(GL_PROJECTION); 
      
    glLoadIdentity(); 
    gluOrtho2D(0,maxHt,0,maxWd); 
    glClear(GL_COLOR_BUFFER_BIT); 
} 
  
void drawPolyDino() 
{ 
  
    glColor3f(1.0f,0.0f,0.0f); 
    int count = 0,x1,y1,x2,y2; 
    rewind(fp); 
    while(!feof(fp) ) 
    { 
        count++; 
        if (count>2) 
        { 
            x1 = x2; 
            y1 = y2; 
            count=2; 
        } 
        if (count==1) 
        { 
            fscanf(fp, "%d,%d", &x1, &y1); 
        } 
        else
        { 
            fscanf(fp, "%d,%d", &x2, &y2); 
            printf("\n%d,%d", x2, y2); 
            glBegin(GL_LINES); 
                glVertex2i( x1, y1); 
                glVertex2i( x2, y2); 
            glEnd(); 
            storeEdgeInTable(x1, y1, x2, y2);//storage of edges in edge table. 
              
              
            glFlush(); 
        } 
    } 
          
          
} 
  
void drawDino(void) 
{ 
    initEdgeTable(); 
    drawPolyDino(); 
    printf("\nTable"); 
    printTable(); 
      
    ScanlineFill();//actual calling of scanline filling.. 
} 
  
void main(int argc, char** argv) 
{ 
    fp=fopen ("PolyDino.txt","r"); 
    if ( fp == NULL ) 
    { 
        printf( "Could not open file" ) ; 
        return; 
    } 
    glutInit(&argc, argv); 
    glutInitDisplayMode(GLUT_SINGLE | GLUT_RGB);  
    glutInitWindowSize(maxHt,maxWd); 
    glutInitWindowPosition(100, 150); 
    glutCreateWindow("Scanline filled dinosaur"); 
    myInit(); 
    glutDisplayFunc(drawDino); 
      
    glutMainLoop(); 
    fclose(fp); 
} 
		
		 */

	}


}
