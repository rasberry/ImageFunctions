using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace ImageFunctions.Engines.SixLabors
{
	public class SLImageConfig : IFImageConfig
	{
		public IFImage LoadImage(string path)
		{
			return new SLImage(path);
		}

		public IFImage NewImage(int width, int height)
		{
			return new SLImage(width,height);
		}

		public void SaveImage(IFImage img, string path)
		{
			var image = img as SLImage;
			image.Save(path);
		}
	}

	public class SLImage : IFImage
	{
		public SLImage(string fileName)
		{
			image = Image.Load<RgbaD>(fileName);
		}

		public SLImage(int w, int h)
		{
			image = new Image<RgbaD>(w,h);
		}

		Image<RgbaD> image;

		public IFColor this[int x, int y] {
			get {
				var ipix = image[x,y];
				return new IFColor { R = ipix.R, G = ipix.G, B = ipix.B, A = ipix.A };
			}
			set {
				var xpix = new RgbaD { R = value.R, G = value.G, B = value.B, A = value.A };
				image[x,y] = xpix;
			}
		}

		public void Save(string fileName)
		{
			image.Save(fileName);
		}

		public int Width { get { return image.Width; }}
		public int Height { get { return image.Height; }}

		public void Dispose()
		{
			if (image != null) {
				image.Dispose();
			}
		}
	}
}
