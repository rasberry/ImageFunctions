using System;
using System.IO;
using ImageMagick;
using System.Linq;

namespace ImageFunctions.Engines.ImageMagick
{
	public class IMImageConfig : IFImageConfig
	{
		static IMImageConfig()
		{
			MagickNET.SetTempDirectory(Environment.CurrentDirectory);
		}

		public IFImage LoadImage(string file)
		{
			var fs = File.Open(file,FileMode.Open,FileAccess.Read);
			using (fs) {
				var image = new MagickImage(fs);
				return new IMImage(image);
			}
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
				IPixel<float> p = Pixels[x,y];
				IMagickColor<float> c = p.ToColor();
				if (c.IsCmyk) {
					throw new NotSupportedException("CMYK is not supported");
				}
				return new IFColor { R = c.R, G = c.G, B = c.B, A = c.A };
			}
			set {
				IPixel<float> p = Pixels[x,y];

				var mc = new MagickColor(value.R,value.G,value.B,value.A);
				float[] pix = null;
				switch(ChannelCount) {
					case 1: pix = new float[] { value.R }; break;
					case 2: pix = new float[] { value.R, value.A }; break;
					case 3: pix = new float[] { value.R, value.G, value.B }; break;
					case 4: pix = new float[] { value.R, value.G, value.B, value.A }; break;
					default:
						throw new NotSupportedException($"Channel Count {ChannelCount} is not supported");
				}
				p.SetValues(pix);
			}
		}

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

		void Init(IMagickImage<float> image)
		{
			NativeImage = image;
			var cp = image.GetColorProfile();
			if (cp != null && cp.ColorSpace != ColorSpace.RGB) {
				image.SetProfile(ColorProfile.SRGB);
			}
			ChannelCount = image.Channels.Count();
			Pixels = NativeImage.GetPixels();
		}

		int ChannelCount;
		IMagickImage<float> NativeImage;
		IPixelCollection<float> Pixels;

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