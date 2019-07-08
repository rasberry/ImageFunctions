
using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions
{
	public static class Helpers
	{
		public static void Assert(bool isTrue, string message = null)
		{
			if (!isTrue) {
				//throw new System.ApplicationException(message ?? "Assertion Failed");
				Log.Debug(message ?? "Assertion Failed");
			}
		}

		public static string FunctionName(Action a)
		{
			return ((int)a).ToString() + ". "+a.ToString();
		}

		public static string CreateOutputFileName(string input)
		{
			//string ex = Path.GetExtension(input);
			string name = Path.GetFileNameWithoutExtension(input);
			string outFile = name+"-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".png";
			return outFile;
		}

		public static void SaveAsPng<TPixel>(string fileName, Image<TPixel> image)
			where TPixel : struct, IPixel<TPixel>
		{
			PngEncoder encoder = new PngEncoder();
			encoder.CompressionLevel = 9;
			image.Save(fileName,encoder);
		}

		public static void SetMaxDegreeOfParallelism<TPixel>(this Image<TPixel> image,int? max)
			where TPixel : struct, IPixel<TPixel>
		{
			if (max.HasValue) {
				var config = image.GetConfiguration();
				config.MaxDegreeOfParallelism = max.Value;
			}
		}

		public static bool ParseNumberPercent(string num, out double val)
		{
			val = 0.0;
			bool isPercent = false;
			if (num.EndsWith('%')) {
				isPercent = true;
				num = num.Remove(num.Length - 1);
			}
			if (!double.TryParse(num, out double d)) {
				Log.Error("could not parse \""+num+"\" as a number");
				return false;
			}
			if (!double.IsFinite(d)) {
				Log.Error("invalid number \""+d+"\"");
				return false;
			}
			val = isPercent ? d/100.0 : d;
			return true;
		}

		public static int IntCeil(int num, int den)
		{
			int floor = num / den;
			int extra = num % den == 0 ? 0 : 1;
			return floor + extra;
		}

		public static string DebugString(this Rectangle r)
		{
			return "X="+r.X+" Y="+r.Y+" W="+r.Width+" H="+r.Height
				+" T="+r.Top+" B="+r.Bottom+" L="+r.Left+" R="+r.Right
			;
		}

		public static TPixel ToGrayScale<TPixel>(TPixel c)
			where TPixel : struct, IPixel<TPixel>
		{
			Rgba32 tmp = default(Rgba32);
			c.ToRgba32(ref tmp);
			tmp = ToGrayScale(tmp);
			c.FromRgba32(tmp);
			return c;
		}

		public static Rgba32 ToGrayScale(Rgba32 c)
		{
			double val = c.R * 0.2126 + c.G * 0.7152 + c.B * 0.0722;
			byte g = (byte)Math.Clamp(val,0.0,255.0);
			return new Rgba32(g,g,g,c.A);
		}

		public static Rgba32 ToColor<TPixel>(this TPixel color)
			where TPixel : struct, IPixel<TPixel>
		{
			Rgba32 c = default(Rgba32);
			color.ToRgba32(ref c);
			return c;
		}

		public static TPixel FromColor<TPixel>(this Rgba32 color)
			where TPixel : struct, IPixel<TPixel>
		{
			TPixel p = default(TPixel);
			p.FromRgba32(color);
			return p;
		}

		public static void BlitImage<TPixel>(this ImageFrame<TPixel> frame, Image<TPixel> image, Rectangle spot)
			where TPixel : struct, IPixel<TPixel>
		{
			var cspan = image.GetPixelSpan();
			var fspan = frame.GetPixelSpan();
			for(int y = spot.Top; y < spot.Bottom; y++) {
				int cy = y - spot.Top;
				for(int x = spot.Left; x < spot.Right; x++) {
					int cx = x - spot.Left;
					int foff = y * frame.Width + x;
					int coff = cy * spot.Width + cx;
					fspan[foff] = cspan[coff];
				}
			}
		}

		public static void ThreadPixels(Rectangle rect,int maxThreads,Action<int,int> callback)
		{
			long max = (long)rect.Width * rect.Height;
			var po = new ParallelOptions {
				MaxDegreeOfParallelism = maxThreads
			};
			Parallel.For(0,max,po,num => {
				int y = (int)(num / (long)rect.Width);
				int x = (int)(num % (long)rect.Width);
				callback(x + rect.Left,y + rect.Top);
			});
		}

		public static void ThreadRows(Rectangle rect, int maxThreads, Action<int> callback)
		{
			var po = new ParallelOptions {
				MaxDegreeOfParallelism = maxThreads
			};
			Parallel.For(rect.Top,rect.Bottom,po,callback);
		}

		public static void ThreadColumns(Rectangle rect, int maxThreads, Action<int> callback)
		{
			var po = new ParallelOptions {
				MaxDegreeOfParallelism = maxThreads
			};
			Parallel.For(rect.Left,rect.Right,po,callback);
		}

		public static bool TryParse<V>(string sub, out V val) where V : IConvertible
		{
			val = default(V);
			TypeCode tc = val.GetTypeCode();
			switch(tc)
			{
			case TypeCode.Double: {
				double.TryParse(sub,out double b);
				val = (V)((object)b); return true;
			}
			case TypeCode.Int32: {
				int.TryParse(sub,out int b);
				val = (V)((object)b); return true;
			}
			}
			return false;
		}

		public static double Fractional(this double number)
		{
			//return number - Math.Truncate(number); //TODO returns negative numbers - don't know why
			return Math.Abs(number % 1.0);
		}
		public static double Integral(this double number)
		{
			return Math.Truncate(number);
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

			//byte moff = (byte)Math.Clamp(255.0 * (fx + fy) / 2.0,0.0,255.0);
			//var mcolor = new Rgba32(moff,moff,moff,255);
			//return mcolor.FromColor<TPixel>();

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
			Rgba32 p0,p1,p2,p3;
			FillQuadrantColors(img, pxf < 0.5, pyf < 0.5, locX, locY, out p0, out p1, out p2, out p3);
			
			double Rf = CalcSample(p0.R, p1.R, p2.R, p3.R, dx, dy);
			double Gf = CalcSample(p0.G, p1.G, p2.G, p3.G, dx, dy);
			double Bf = CalcSample(p0.B, p1.B, p2.B, p3.B, dx, dy);
			double Af = CalcSample(p0.A, p1.A, p2.A, p3.A, dx, dy);

			var color = new Rgba32((byte)Rf,(byte)Gf,(byte)Bf,(byte)Af);
			TPixel pixi = color.FromColor<TPixel>();
			//Log.Debug("pix = "+pixi);
			return pixi;
		}

		static double CalcSample(byte v0,byte v1,byte v2,byte v3,double dx, double dy)
		{
			//four corner values
			double b0 = (double)v0;
			double b1 = (double)v1;
			double b2 = (double)v2;
			double b3 = (double)v3;

			//interpolation calc
			double h0 = (b0 * (1.0 - dx)) + (b1 * dx);
			double h1 = (b3 * (1.0 - dx)) + (b2 * dx);
			double vf = (h0 * (1.0 - dy)) + (h1 * dy);

			return Math.Clamp(vf,0.0,255.0);
		}

		static void FillQuadrantColors<TPixel>(
			ImageFrame<TPixel> img, bool xIsPos, bool yIsPos,double px, double py,
			out Rgba32 q0, out Rgba32 q1, out Rgba32 q2, out Rgba32 q3)
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
			q0 = img.GetPixelSafe(px0,py0).ToColor();
			q1 = img.GetPixelSafe(px1,py1).ToColor();
			q2 = img.GetPixelSafe(px2,py2).ToColor();
			q3 = img.GetPixelSafe(px3,py3).ToColor();
		}
	}
}
