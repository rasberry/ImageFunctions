using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Collections;
using SixLabors.ImageSharp.Advanced;

namespace ImageFunctions.Core.Engines
{
	public class SixLaborsEngine : IImageEngine, IDrawEngine
	{
		public ILayers NewImage(int width, int height)
		{
			return new SLImage(width,height);
		}

		public ILayers LoadImage(string path)
		{
			var image = Image.Load<RgbaD>(path);
			var layers = new SLImage(image, path);
			return layers;
		}

		public void SaveImage(ILayers layers, string path, string format = null)
		{
			IImageEncoder enc = null;
			if (format != null) {
				var Ifm = Configuration.Default.ImageFormatsManager;
				//Name doesn't seem to be have a built-in search, so using slow search for now
				foreach(var f in Ifm.ImageFormats) {
					bool e = StringComparer.OrdinalIgnoreCase.Equals(f.Name,format);
					if (e) {
						enc = Ifm.GetEncoder(f);
						break;
					}
				}
			}

			var wrap = (SLImage)layers;

			if (enc != null) {
				wrap.Layers.Save(path, enc);
			}
			else {
				//detects format based on extension
				wrap.Layers.Save(path);
			}
		}

		//TODO don't know how to fix this..
		// can't draw a line on a frame which seems kind of an oversight
		public void DrawLine(ICanvas image, ColorRGBA color, PointD p0, PointD p1, double width = 1.0)
		{
			var opts = new DrawingOptions {
				GraphicsOptions = new GraphicsOptions { Antialias = true }
			};
			var rgba = new RgbaD { R = color.R, G = color.G, B = color.B, A = color.A };
			var c = new Color(rgba.ToScaledVector4());
			var f0 = new PointF((float)p0.X,(float)p0.Y);
			var f1 = new PointF((float)p1.X,(float)p1.Y);

			Image<RgbaD> im;
			im.Mutate((ctx) => {

			});
			var wrap = (SLCanvas)image;
			wrap.Frame.Mutate((ctx) => {
				ctx.DrawLine(opts,c,(float)width,f0,f1);
			});
			//Image<RgbaD>.WrapMemory<RgbaD>(wrap.Frame.PixelBuffer.MemoryGroup, wrap.Frame.Width, wrap.Frame.Height);

			var conf = wrap.Frame.GetConfiguration();
			var obj = conf.ImageOperationsProvider.CreateImageProcessingContext(configuration, source, mutate: true);
		}

		public IEnumerable<ImageFormat> Formats()
		{
			var Ifm = Configuration.Default.ImageFormatsManager;
			foreach(var f in Ifm.ImageFormats) {
				var enc = Ifm.GetEncoder(f);
				var dec = Ifm.GetDecoder(f);

				yield return new ImageFormat(
					f.Name,
					$"{f.DefaultMimeType} [{String.Join(",",f.FileExtensions)}]",
					enc != null,
					dec != null
				);
			}
		}

		/*
		public void Resize(ICanvas image, int width, int height)
		{
			// Log.Debug($"resizing {image.Width}x{image.Height} -> {width}x{height}");
			if (image.Width == width && image.Height == height) {
				return; //nothing to do
			}

			var nativeImage = (SLCanvas)image;
			nativeImage.Frame.Mutate((ctx) => {
				ctx.Resize(width,height);
			});
		}
		*/
	}

	class SLImage : ILayers, IDisposable
	{
		public SLImage(Image<RgbaD> image, string file)
		{
			var name = Path.GetFileName(file);
			Init(image, name);
		}

		public SLImage(int w, int h)
		{
			var image = new Image<RgbaD>(w,h);
			Init(image, "New");
		}

		void Init(Image<RgbaD> image, string name = null)
		{
			Layers = image;
			int count = image.Frames.Count;
			Names = new List<string>(image.Frames.Count);
			for(int i=0; i < count; i++) {
				Names[i] = GetDefaultName(name,i);
			}
		}

		public ICanvas this[int index] {
			get {
				var native = Layers.Frames[index];
				return new SLCanvas(native);
			}
			set {
				var wrap = (SLCanvas)value;
				Layers.Frames.RemoveFrame(index);
				Layers.Frames.InsertFrame(index, wrap.Frame);
			}
		}

		internal Image<RgbaD> Layers;
		List<string> Names;

		public int Count {
			get {
				return Layers.Frames.Count;
			}
		}

		public ICanvas AddNew(string name = null)
		{
			Layers.Frames.CreateFrame();
			Names.Add(GetDefaultName(name, Layers.Frames.Count));
			var native = Layers.Frames[Layers.Frames.Count - 1];
			return new SLCanvas(native);
		}

		public IEnumerator<ICanvas> GetEnumerator()
		{
			foreach(var native in Layers.Frames) {
				yield return new SLCanvas(native);
			}
		}

		public int IndexOf(string name, int startIndex = 0)
		{
			return Names.IndexOf(name,startIndex);
		}

		public void InsertAt(int index, ICanvas layer, string name = null)
		{
			var wrap = (SLCanvas)layer;
			Layers.Frames.InsertFrame(index, wrap.Frame);
			Names.Insert(index, GetDefaultName(name,index));
		}

		public ICanvas RemoveAt(int index)
		{
			var wrap = new SLCanvas(Layers.Frames[index]);
			Layers.Frames.RemoveFrame(index);
			Names.RemoveAt(index);
			return wrap;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		static string GetDefaultName(string name, int index)
		{
			if (string.IsNullOrWhiteSpace(name)) {
				name = "Layer";
			}
			return $"{name}-{index}";
		}

		public void Dispose()
		{
			if (Layers != null) {
				Layers.Dispose();
			}
		}
	}

	class SLCanvas : ICanvas
	{
		//public SLImage(string fileName)
		//{
		//	image = Image.Load<RgbaD>(fileName);
		//}
		//
		//public SLImage(int w, int h)
		//{
		//	image = new Image<RgbaD>(w,h);
		//}
		//internal Image<RgbaD> image;

		public SLCanvas(ImageFrame<RgbaD> frame)
		{
			Frame = frame;
		}

		internal ImageFrame<RgbaD> Frame;

		public ColorRGBA this[int x, int y] {
			get {
				var ipix = Frame[x,y];
				return new ColorRGBA(ipix.R,ipix.G,ipix.B,ipix.A);
			}
			set {
				var xpix = new RgbaD { R = value.R, G = value.G, B = value.B, A = value.A };
				Frame[x,y] = xpix;
			}
		}

		/*
		public void Save(string fileName, string format)
		{
			IImageEncoder enc = null;
			if (format != null) {
				var Ifm = Configuration.Default.ImageFormatsManager;
				//Name doesn't seem to be have a built-in search, so using slow search for now
				foreach(var f in Ifm.ImageFormats) {
					bool e = StringComparer.OrdinalIgnoreCase.Equals(f.Name,format);
					if (e) {
						enc = Ifm.GetEncoder(f);
						break;
					}
				}
			}

			if (enc != null) {
				Frame.Save(fileName, enc);
			}
			else {
				//detects format based on extension
				Frame.Save(fileName);
			}
		}
		*/

		public int Width { get { return Frame.Width; }}
		public int Height { get { return Frame.Height; }}

		/*
		public void Dispose()
		{
			if (Frame != null) {
				Frame.Dispose();
			}
		}
		*/
	}

	//since native type is double, using a double based color should minimize conversions
	struct RgbaD : IEquatable<RgbaD>, IPixel<RgbaD>
	{
		public RgbaD(double r,double g,double b,double a)
		{
			R = Math.Clamp(r,0.0,1.0);
			G = Math.Clamp(g,0.0,1.0);
			B = Math.Clamp(b,0.0,1.0);
			A = Math.Clamp(a,0.0,1.0);
		}

		public double R;
		public double G;
		public double B;
		public double A;

		public static bool operator == (RgbaD lhs, RgbaD rhs)
		{
			return
				   lhs.R == rhs.R
				&& lhs.G == rhs.G
				&& lhs.B == rhs.B
				&& lhs.A == rhs.A
			;
		}

		public static bool operator != (RgbaD lhs, RgbaD rhs)
		{
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
		public void FromVector4(Vector4 v) { FromScaledVector4(v); }
		public Vector4 ToVector4() { return ToScaledVector4(); }
		public void ToRgba32(ref Rgba32 dest) { dest.FromScaledVector4(ToScaledVector4()); }

		public void FromArgb32(Argb32 source)     { FromScaledVector4(source.ToScaledVector4()); }
		public void FromBgra5551(Bgra5551 source) { FromScaledVector4(source.ToScaledVector4()); }
		public void FromBgr24(Bgr24 source)       { FromScaledVector4(source.ToScaledVector4()); }
		public void FromBgra32(Bgra32 source)     { FromScaledVector4(source.ToScaledVector4()); }
		public void FromL8(L8 source)             { FromScaledVector4(source.ToScaledVector4()); }
		public void FromL16(L16 source)           { FromScaledVector4(source.ToScaledVector4()); }
		public void FromLa16(La16 source)         { FromScaledVector4(source.ToScaledVector4()); }
		public void FromLa32(La32 source)         { FromScaledVector4(source.ToScaledVector4()); }
		public void FromRgb24(Rgb24 source)       { FromScaledVector4(source.ToScaledVector4()); }
		public void FromRgba32(Rgba32 source)     { FromScaledVector4(source.ToScaledVector4()); }
		public void FromRgb48(Rgb48 source)       { FromScaledVector4(source.ToScaledVector4()); }
		public void FromRgba64(Rgba64 source)     { FromScaledVector4(source.ToScaledVector4()); }
		public void FromAbgr32(Abgr32 source)     { FromScaledVector4(source.ToScaledVector4()); }
	}
}
