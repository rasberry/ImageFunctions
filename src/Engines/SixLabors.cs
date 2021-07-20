using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ImageFunctions.Engines.SixLabors
{
	public class SLImageEngine : IImageEngine, IDrawEngine, IFormatGuide
	{
		public IImage LoadImage(string path)
		{
			return new SLImage(path);
		}

		public IImage NewImage(int width, int height)
		{
			return new SLImage(width,height);
		}

		public void SaveImage(IImage img, string path, string format = null)
		{
			var image = img as SLImage;
			image.Save(path, format);
		}

		public void DrawLine(IImage image, IColor color, PointD p0, PointD p1, double width = 1.0)
		{
			var go = new GraphicsOptions { Antialias = true };
			var rgba = new RgbaD { R = color.R, G = color.G, B = color.B, A = color.A };
			var c = new Color(rgba.ToScaledVector4());
			var f0 = new PointF((float)p0.X,(float)p0.Y);
			var f1 = new PointF((float)p1.X,(float)p1.Y);

			var nativeImage = (SLImage)image;
			nativeImage.image.Mutate((ctx) => {
				ctx.DrawLines(go,c,(float)width,f0,f1);
			});
		}

		public IEnumerable<string> ListFormatNames()
		{
			var Ifm = Configuration.Default.ImageFormatsManager;
			foreach(var f in Ifm.ImageFormats) {
				var enc = Ifm.FindEncoder(f);
				if (enc != null) {
					yield return f.Name;
				}
			}
		}

		public string GetFormatDescription(string formatName)
		{
			if (String.IsNullOrWhiteSpace(formatName)) { return null; }
			var Ifm = Configuration.Default.ImageFormatsManager;
			//Name doesn't seem to be have a built-in search, so using slow search for now
			foreach(var f in Ifm.ImageFormats) {
				bool e = StringComparer.OrdinalIgnoreCase.Equals(f.Name,formatName);
				if (e) {
					return $"{f.DefaultMimeType} [{String.Join(",",f.FileExtensions)}]";
				}
			}
			return null;
		}

		public void Resize(IImage image, int width, int height)
		{
			var nativeImage = (SLImage)image;
			nativeImage.image.Mutate((ctx) => {
				var ro = new ResizeOptions {
					Mode = ResizeMode.BoxPad,
					Size = new Size(width,height)
				};
				ctx.Resize(ro);
			});
		}
	}

	public class SLImage : IImage
	{
		public SLImage(string fileName)
		{
			image = Image.Load<RgbaD>(fileName);
		}

		public SLImage(int w, int h)
		{
			image = new Image<RgbaD>(w,h);
		}

		internal Image<RgbaD> image;

		public IColor this[int x, int y] {
			get {
				var ipix = image[x,y];
				return new IColor(ipix.R,ipix.G,ipix.B,ipix.A);
			}
			set {
				var xpix = new RgbaD { R = value.R, G = value.G, B = value.B, A = value.A };
				image[x,y] = xpix;
			}
		}

		public void Save(string fileName, string format)
		{
			IImageEncoder enc = null;
			if (format != null) {
				var Ifm = Configuration.Default.ImageFormatsManager;
				//Name doesn't seem to be have a built-in search, so using slow search for now
				foreach(var f in Ifm.ImageFormats) {
					bool e = StringComparer.OrdinalIgnoreCase.Equals(f.Name,format);
					if (e) {
						enc = Ifm.FindEncoder(f);
						break;
					}
				}
			}

			if (enc != null) {
				image.Save(fileName, enc);
			}
			else {
				//detects format based on extension
				image.Save(fileName);
			}
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

	//since native type is double, using a double based color should minimize conversions
	struct RgbaD : IEquatable<RgbaD>, IPixel<RgbaD>
	{
		public RgbaD(double r,double g,double b,double a)
		{
			//Log.Debug($"RgbaD [{r},{g},{b},{a}]");
			//bool isValid =
			//	   r >= 0.0 && g >= 0.0 && b >= 0.0 && a >= 0.0
			//	&& r <= 1.0 && g <= 1.0 && b <= 1.0 && a <= 1.0;
			//if (!isValid) {
			//	throw new ArgumentOutOfRangeException("Valid range for components is [0.0,1.0]");
			//}
			R = Math.Clamp(r,0.0,1.0);
			G = Math.Clamp(g,0.0,1.0);
			B = Math.Clamp(b,0.0,1.0);
			A = Math.Clamp(a,0.0,1.0);
		}

		public double R;
		public double G;
		public double B;
		public double A;

		public static bool operator == (RgbaD lhs, RgbaD rhs) {
			return
				   lhs.R == rhs.R
				&& lhs.G == rhs.G
				&& lhs.B == rhs.B
				&& lhs.A == rhs.A
			;
		}

		public static bool operator != (RgbaD lhs, RgbaD rhs) {
			return !(lhs == rhs);
		}

		public bool Equals(RgbaD compare)
		{
			return this == compare;
		}

		public override bool Equals(object compare)
		{
			var right = (RgbaD)compare;
			return this == right;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(R,G,B,A);
		}

		public PixelOperations<RgbaD> CreatePixelOperations()
		{
			return new PixelOperations<RgbaD>();
		}

		public void FromScaledVector4(Vector4 v)
		{
			this.R = v.X;
			this.G = v.Y;
			this.B = v.Z;
			this.A = v.W;
		}

		public Vector4 ToScaledVector4()
		{
			return new Vector4((float)R,(float)G,(float)B,(float)A);
		}

		public void FromVector4(Vector4 v)
		{
			FromScaledVector4(v);
		}

		public Vector4 ToVector4()
		{
			return ToScaledVector4();
		}

		public void FromArgb32(Argb32 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromBgra5551(Bgra5551 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromBgr24(Bgr24 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromBgra32(Bgra32 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromGray8(Gray8 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromGray16(Gray16 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromRgb24(Rgb24 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromRgba32(Rgba32 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void ToRgba32(ref Rgba32 dest)
		{
			dest.FromScaledVector4(ToScaledVector4());
		}

		public void FromRgb48(Rgb48 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}

		public void FromRgba64(Rgba64 source)
		{
			FromScaledVector4(source.ToScaledVector4());
		}
	}
}
