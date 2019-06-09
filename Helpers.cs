using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace ImageFunctions
{
	public static class Helpers
	{
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
	}
}
