using System;
using System.Collections.Generic;
using System.IO;
using ImageMagick;
//using System.Linq;
using QType = System.Single;

namespace ImageFunctions.Engines.ImageMagick
{
	public class IMImageEngine : IImageEngine, IDrawEngine, IFormatGuide
	{
		static IMImageEngine()
		{
			MagickNET.SetTempDirectory(Environment.CurrentDirectory);
			//Log.Debug($"Quantum = {Quantum.Depth} {Quantum.Max}");
		}

		public IImage LoadImage(string file)
		{
			var image = new MagickImage(file);
			return new IMImage(image);
		}

		public IImage NewImage(int width, int height)
		{
			return new IMImage(width,height);
		}

		public void SaveImage(IImage img, string path, string format = null)
		{
			var image = img as IMImage;
			image.Save(path, format);
		}

		// http://www.graphicsmagick.org/Magick++/Drawable.html
		public void DrawLine(IImage image, IColor color, PointD p0, PointD p1, double width = 1.0)
		{
			var nativeImage = (IMImage)image;
			var incolor = ImageMagickUtils.ConvertToExternal(color,nativeImage.ChannelCount);
			var d0 = new DrawableStrokeColor(incolor);
			var d1 = new DrawableStrokeWidth(width);
			var d2 = new DrawableLine(p0.X,p0.Y,p1.X,p1.Y);
			nativeImage.NativeImage.Draw(d0,d1,d2);
		}

		public IEnumerable<string> ListFormatNames()
		{
			foreach(MagickFormat mf in Enum.GetValues(typeof(MagickFormat))) {
				var info = MagickFormatInfo.Create(mf);
				if (info != null && info.IsWritable) {
					yield return mf.ToString();
				}
			}
		}

		public string GetFormatDescription(string formatName)
		{
			if (String.IsNullOrWhiteSpace(formatName)) { return ""; }
			bool w = Enum.TryParse<MagickFormat>(formatName,true,out MagickFormat mf);
			if (w) {
				var info = MagickFormatInfo.Create(mf);
				return info.Description;
			}
			return "";
		}

		public void Resize(IImage image, int width, int height)
		{
			var nativeImage = (IMImage)image;
			nativeImage.Resize(width,height);
		}

	}

	public class IMImage : IImage
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

		public IColor this[int x, int y] {
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
				return ImageMagickUtils.ConvertToInternal(c);
			}
			set {
				IPixel<QType> p = Pixels[x,y];
				QType[] color = ImageMagickUtils.ConvertToComponents(value,ChannelCount);
				p.SetValues(color);
			}
		}

		//string printpixel(IPixel<QType> p)
		//{
		//	return $"pix {p.Channels} [{p.X} {p.Y}] ({String.Join(',',p.ToArray())})";
		//}

		public void Save(string path, string format = null)
		{
			if (String.IsNullOrWhiteSpace(format)) {
				format = Path.GetExtension(path);
			}

			bool good = Enum.TryParse<MagickFormat>(format,true,out MagickFormat mf);
			if (good) {
				var info = MagickFormatInfo.Create(mf);
				good = info != null && info.IsWritable;
			}
			if (!good) {
				throw new NotSupportedException($"Format '{format??""}' is not supported");
			}

			var fs = File.Open(path,FileMode.CreateNew,FileAccess.Write,FileShare.Read);
			using (fs) {
				NativeImage.Write(fs,mf);
			}
		}

		public void Resize(int width, int height)
		{
			var mg = new MagickGeometry(width,height);
			NativeImage.Extent(width,height,Gravity.Northwest,MagickColors.Transparent);
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

		internal int ChannelCount;
		internal IMagickImage<QType> NativeImage;
		IPixelCollection<QType> Pixels;
	}

	internal static class ImageMagickUtils
	{
		internal static IColor ConvertToInternal(IMagickColor<QType> x)
		{
			double max = Quantum.Max;
			double ir = Math.Clamp((double)x.R / max,0.0,1.0);
			double ig = Math.Clamp((double)x.G / max,0.0,1.0);
			double ib = Math.Clamp((double)x.B / max,0.0,1.0);
			double ia = Math.Clamp((double)x.A / max,0.0,1.0);
			var color = new IColor(ir,ig,ib,ia);
			return color;
		}

		internal static QType[] ConvertToComponents(IColor i, int channelCount)
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

		internal static IMagickColor<QType> ConvertToExternal(IColor i, int channelCount)
		{
			var comps = ConvertToComponents(i,channelCount);
			switch(channelCount) {
				case 1: return new MagickColor(comps[0],comps[0],comps[0]);
				case 2: return new MagickColor(comps[0],comps[0],comps[0],comps[1]);
				case 3: return new MagickColor(comps[0],comps[1],comps[2]);
				case 4: return new MagickColor(comps[0],comps[1],comps[2],comps[3]);
				default:
					throw new NotSupportedException($"Channel Count {channelCount} is not supported");
			}
		}
	}
}
