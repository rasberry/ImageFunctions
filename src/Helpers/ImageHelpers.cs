using System;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions.Helpers
{
	public static class ImageHelpers
	{
		public static void SaveAsPng(string fileName, Image image)
		{
			PngEncoder encoder = new PngEncoder();
			encoder.CompressionLevel = 9;
			image.Save(fileName,encoder);
		}

		public static TPixel ToGrayScale<TPixel>(TPixel c)
			where TPixel : struct, IPixel<TPixel>
		{
			RgbaD v = ToColor(c);
			double val = v.R * 0.2126 + v.G * 0.7152 + v.B * 0.0722;
			var vGray = new RgbaD(val,val,val,v.A);
			return vGray.FromColor<TPixel>();
		}

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

		public static void BlitImage<TPixel>(this ImageFrame<TPixel> dstImg, ImageFrame<TPixel> srcImg,
			Rectangle dstRect = default(Rectangle), Point srcPoint = default(Point))
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

		public static TPixel GetPixelSafe<TPixel>(this ImageFrame<TPixel> img, int x, int y)
			where TPixel : struct, IPixel<TPixel>
		{
			int px = Math.Clamp(x,0,img.Width - 1);
			int py = Math.Clamp(y,0,img.Height - 1);
			int off = py * img.Width + px;
			//Log.Debug("GPS off = "+off);
			return img.GetPixelSpan()[off];
		}

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

		static double CalcSample(double v0,double v1,double v2,double v3,double dx, double dy)
		{
			//interpolation calc
			double h0 = (v0 * (1.0 - dx)) + (v1 * dx);
			double h1 = (v3 * (1.0 - dx)) + (v2 * dx);
			double vf = (h0 * (1.0 - dy)) + (h1 * dy);
			return vf;
		}

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
	}
}
