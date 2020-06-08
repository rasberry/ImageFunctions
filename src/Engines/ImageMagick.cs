using System;
using System.IO;
using ImageMagick;
//using System.Linq;
using QType = System.Single;

namespace ImageFunctions.Engines.ImageMagick
{
	public class IMImageConfig : IFImageConfig
	{
		static IMImageConfig()
		{
			MagickNET.SetTempDirectory(Environment.CurrentDirectory);
			//Log.Debug($"Quantum = {Quantum.Depth} {Quantum.Max}");
		}

		public IFImage LoadImage(string file)
		{
			var image = new MagickImage(file);
			return new IMImage(image);
		}

		public IFImage NewImage(int width, int height)
		{
			return new IMImage(width,height);
		}

		public void SaveImage(IFImage img, string path)
		{
			var image = img as IMImage;
			image.Save(path);
		}
	}

	public class IMImage : IFImage
	{
		public IMImage(MagickImage image)
		{
			Init(image);
		}

		public IMImage(int w,int h)
		{
			var image = new MagickImage(MagickColors.None,w,h);
			Init(image);
		}

		public IFColor this[int x, int y] {
			get {
				//Log.Debug("PP1: "+printpixel(Pixels.GetPixel(20,20)));
				var vals = Pixels.GetValue(x,y);
				//Log.Debug(printpixel(new Pixel(x,y,vals)));
				IPixel<QType> p = Pixels.GetPixel(x,y);
				//Log.Debug(printpixel(p));
				IMagickColor<QType> c = p.ToColor();
				if (c.IsCmyk) {
					throw new NotSupportedException("CMYK is not supported");
				}
				//Log.Debug($"Get Pixel @ [{x},{y}] ({c.R} {c.G} {c.B} {c.A}]");
				return ConvertToInternal(c);
			}
			set {
				IPixel<QType> p = Pixels[x,y];
				QType[] color = ConvertToExternal(value,ChannelCount);
				p.SetValues(color);
			}
		}

		static IFColor ConvertToInternal(IMagickColor<QType> x)
		{
			double max = Quantum.Max;
			double ir = Math.Clamp((double)x.R / max,0.0,1.0);
			double ig = Math.Clamp((double)x.G / max,0.0,1.0);
			double ib = Math.Clamp((double)x.B / max,0.0,1.0);
			double ia = Math.Clamp((double)x.A / max,0.0,1.0);
			var color = new IFColor { R = ir, G = ig, B = ib, A = ia };
			return color;
		}

		static QType[] ConvertToExternal(IFColor i, int channelCount)
		{
			double max = Quantum.Max;
			QType xr = (QType)(i.R * max);
			QType xg = (QType)(i.G * max);
			QType xb = (QType)(i.B * max);
			QType xa = (QType)(i.A * max);

			QType[] color;
			switch(channelCount) {
				case 1: color = new QType[] { xr }; break;
				case 2: color = new QType[] { xr, xa }; break;
				case 3: color = new QType[] { xr, xg, xb }; break;
				case 4: color = new QType[] { xr, xg, xb, xa }; break;
				default:
					throw new NotSupportedException($"Channel Count {channelCount} is not supported");
			}
			return color;
		}

		//string printpixel(IPixel<QType> p)
		//{
		//	return $"pix {p.Channels} [{p.X} {p.Y}] ({String.Join(',',p.ToArray())})";
		//}

		public void Save(string path)
		{
			var fs = File.Open(path,FileMode.CreateNew,FileAccess.Write,FileShare.Read);
			using (fs) {
				NativeImage.Write(fs);
			}
		}

		public void Dispose()
		{
			if (Pixels != null) {
				Pixels.Dispose();
			}
			if (NativeImage != null) {
				NativeImage.Dispose();
			}
		}

		public int Width { get { return NativeImage.Width; }}
		public int Height { get { return NativeImage.Height; }}

		void Init(IMagickImage<QType> image)
		{
			NativeImage = image;
			//var cp = image.GetColorProfile();
			//if (cp != null && cp.ColorSpace != ColorSpace.RGB) {
			//	image.SetProfile(ColorProfile.SRGB);
			//}
			ChannelCount = image.ChannelCount;
			Pixels = image.GetPixels();
			//Log.Debug("PP1: "+printpixel(Pixels.GetPixel(20,20)));
		}

		int ChannelCount;
		IMagickImage<QType> NativeImage;
		IPixelCollection<QType> Pixels;
	}
}

		//// https://www.rapidtables.com/convert/color/cmyk-to-rgb.html
		//static IFColor CYMKToRGB(IMagickColor<float> color)
		//{
		//	float km = 1.0f - color.K;
		//	float r = Quantum.Max * (1.0f - color.R) * km;
		//	float g = Quantum.Max * (1.0f - color.G) * km;
		//	float b = Quantum.Max * (1.0f - color.B) * km;
		//	return new IFColor { R = r, G = g, B = b, A = color.A };
		//}

		//// https://www.rapidtables.com/convert/color/rgb-to-cmyk.html
		//static IMagickColor<float> RGBToCMYK(IFColor color)
		//{
		//	float r = color.R / Quantum.Max;
		//	float g = color.G / Quantum.Max;
		//	float b = color.B / Quantum.Max;
		//	float k = 1.0f - Math.Max(Math.Max(r,g),b);
		//	float c = 1.0f - r - k;
		//	float m = 1.0f - g - k;
		//	float y = 1.0f - b - k;
		//	return new MagickColor(c,m,y,k,color.A);
		//}