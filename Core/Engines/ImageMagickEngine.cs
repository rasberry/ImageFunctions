using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ImageMagick;
//using System.Linq;
using QType = System.Single;

namespace ImageFunctions.Core.Engines
{
	public class ImageMagickEngine : IImageEngine, IDrawEngine
	{
		static ImageMagickEngine()
		{
			MagickNET.SetTempDirectory(Environment.CurrentDirectory);
			//Log.Debug($"Quantum = {Quantum.Depth} {Quantum.Max}");
		}

		public void LoadImage(ILayers layers, string file)
		{
			var native = new MagickImageCollection(file);
			foreach(var frame in native) {
				var wrap = new IMCanvas(frame);
				layers.Add(wrap);
			}
		}

		public ICanvas NewCanvas(int width, int height)
		{
			var wrap = new IMCanvas(width, height);
			return wrap;
		}

		/*
		public ILayers LoadImage(string file)
		{
			var native = new MagickImageCollection(file);
			var layers = new IMImage(native);
			return layers;
		}
		*/

		//public ICanvas LoadImage(string file)
		//{
		//	var image = new MagickImage(file);
		//	return new IMImage(image);
		//}

		//public ILayers NewImage(int width, int height)
		//{
		//	return new IMImage(width,height);
		//}

		//public void SaveImage(ICanvas img, string path, string format = null)
		//{
		//	var image = img as IMImage;
		//	image.Save(path, format);
		//}

		public void SaveImage(ILayers layers, string path, string format = null)
		{
			if (layers.Count == 0) {
				throw Squeal.NoLayers();
			}

			//default to the format of the output file
			if (String.IsNullOrWhiteSpace(format)) {
				format = Path.GetExtension(path).Remove(0,1); //remove the dot
			}

			bool good = Enum.TryParse(format,true,out MagickFormat mf);
			if (good) {
				var info = MagickFormatInfo.Create(mf);
				good = info != null && info.SupportsWriting;
			}
			if (!good) {
				throw Squeal.FormatIsNotSupported(format);
			}

			var image = new MagickImageCollection(UnWrapLayers(layers));
			image.Write(path,mf);
		}

		static IEnumerable<IMagickImage<QType>> UnWrapLayers(ILayers layers)
		{
			foreach(var lay in layers) {
				var wrap = (IMCanvas)lay;
				yield return wrap.NativeImage;
			}
		}

		// http://www.graphicsmagick.org/Magick++/Drawable.html
		public void DrawLine(ICanvas image, ColorRGBA color, PointD p0, PointD p1, double width = 1.0)
		{
			var nativeImage = (IMCanvas)image;
			var incolor = ImageMagickUtils.ConvertToExternal(color,nativeImage.ChannelCount);
			var d0 = new DrawableStrokeColor(incolor);
			var d1 = new DrawableStrokeWidth(width);
			var d2 = new DrawableLine(p0.X,p0.Y,p1.X,p1.Y);
			nativeImage.NativeImage.Draw(d0,d1,d2);
		}

		public IEnumerable<ImageFormat> Formats()
		{
			foreach(MagickFormat mf in Enum.GetValues(typeof(MagickFormat))) {
				var info = MagickFormatInfo.Create(mf);
				if (info != null) {
					yield return new ImageFormat(
						mf.ToString(),
						info.Description,
						info.SupportsReading,
						info.SupportsWriting,
						info.SupportsMultipleFrames
					);
				}
			}
		}

		/*
		public void Resize(ICanvas image, int width, int height)
		{
			var nativeImage = (IMImage)image;
			nativeImage.Resize(width,height);
		}
		*/
	}

	/*
	class IMImage : ILayers, IDisposable
	{
		public IMImage(MagickImageCollection image)
		{
			Init(image);
		}

		public IMImage(int w,int h)
		{
			var frame = new MagickImage(MagickColors.None,w,h);
			var image = new MagickImageCollection { frame };
			Init(image);
		}

		void Init(MagickImageCollection image)
		{
			Layers = image;

			//fill in the names
			int count = image.Count;
			Names = new List<string>(count);

			for(int i=0; i < count; i++) {
				var frame = image[i];

				string fileName = !String.IsNullOrWhiteSpace(frame.FileName)
					? Path.GetFileName(frame.FileName)
					: ""
				;
				Names[i] = GetDefaultName(fileName,i);
			}
		}

		internal MagickImageCollection Layers;
		List<string> Names;

		public ICanvas this[int index] {
			get {
				var imLayer = Layers[index];
				return new IMCanvas(imLayer);
			}
			set {
				var wrap = (IMCanvas)value;
				var imLayer = wrap.NativeImage;
				Layers[index] = imLayer;
			}
		}

		public int Count {
			get {
				return Layers.Count;
			}
		}

		//public void Add(ICanvas layer, string name = null)
		//{
		//	var wrap = (IMCanvas)layer;
		//	Layers.Add(wrap.NativeImage);
		//	Names.Add(name);
		//
		//	if (Layers.Count != Names.Count) {
		//		throw new NotSupportedException("Layers and Names are out of sync");
		//	}
		//}

		public ICanvas AddNew(string name = null)
		{
			var (w,h) = GetWidthHeight();
			var wrap = new IMCanvas(w,h);
			Layers.Add(wrap.NativeImage);
			Names.Add(GetDefaultName(name,Layers.Count));
			return wrap;
		}

		public IEnumerator<ICanvas> GetEnumerator()
		{
			foreach(var frame in Layers) {
				yield return new IMCanvas(frame);
			}
		}

		public int IndexOf(string name, int startIndex = 0)
		{
			return Names.IndexOf(name,startIndex);
		}

		public void InsertAt(int index, ICanvas layer, string name = null)
		{
			var wrap = (IMCanvas)layer;
			Layers.Insert(index, wrap.NativeImage);
			Names.Insert(index,GetDefaultName(name, Layers.Count));
		}

		public ICanvas RemoveAt(int index)
		{
			var wrap = new IMCanvas(Layers[index]);
			Layers.RemoveAt(index);
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

		(int,int) GetWidthHeight()
		{
			if (Layers.Count < 1) {
				throw new ArgumentOutOfRangeException("Layers.Count");
			}
			var layer = Layers[0];
			return (layer.Width, layer.Height);
		}

		public void Dispose()
		{
			if (Layers != null) {
				Layers.Dispose();
			}
			if (Names != null) {
				Names = null;
			}
		}
	}
	*/

	public class IMCanvas : ICanvas, IDisposable
	{
		public IMCanvas(IMagickImage<QType> image)
		{
			Init(image);
		}

		public IMCanvas(int w,int h)
		{
			var image = new MagickImage(MagickColors.None,w,h);
			Init(image);
		}

		public ColorRGBA this[int x, int y] {
			get {
				//Log.Debug("PP1: "+printpixel(Pixels.GetPixel(20,20)));
				//var vals = Pixels.GetValue(x,y);
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

		/*
		public void Resize(int width, int height)
		{
			if (width == NativeImage.Width && height == NativeImage.Height) {
				return; //nothing to do
			}

			//Log.Debug($"resizing im {NativeImage.Width}x{NativeImage.Height} -> {width}x{height}");
			var mg = new MagickGeometry(width,height);
			NativeImage.Extent(width,height,Gravity.Northwest,MagickColors.Transparent);
			Init(NativeImage); //pixels have changed so re-init
		}
		*/

		//string printpixel(IPixel<QType> p)
		//{
		//	return $"pix {p.Channels} [{p.X} {p.Y}] ({String.Join(',',p.ToArray())})";
		//}

		/*
		public void Save(string path, string format = null)
		{
			if (String.IsNullOrWhiteSpace(format)) {
				format = Path.GetExtension(path).Remove(0,1); //remove the dot
			}

			bool good = Enum.TryParse(format,true,out MagickFormat mf);
			if (good) {
				var info = MagickFormatInfo.Create(mf);
				good = info != null && info.SupportsWriting;
			}
			if (!good) {
				throw new NotSupportedException($"Format '{format??""}' is not supported");
			}

			var fs = File.Open(path,FileMode.CreateNew,FileAccess.Write,FileShare.Read);
			using (fs) {
				NativeImage.Write(fs,mf);
			}
		}
		*/

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
			ChannelCount = image.ChannelCount;
			Pixels = image.GetPixels();
		}

		internal int ChannelCount;
		internal IMagickImage<QType> NativeImage;
		IPixelCollection<QType> Pixels;
	}

	internal static class ImageMagickUtils
	{
		internal static ColorRGBA ConvertToInternal(IMagickColor<QType> x)
		{
			double max = Quantum.Max;
			double ir = Math.Clamp((double)x.R / max,0.0,1.0);
			double ig = Math.Clamp((double)x.G / max,0.0,1.0);
			double ib = Math.Clamp((double)x.B / max,0.0,1.0);
			double ia = Math.Clamp((double)x.A / max,0.0,1.0);
			var color = new ColorRGBA(ir,ig,ib,ia);
			return color;
		}

		internal static QType[] ConvertToComponents(ColorRGBA i, int channelCount)
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

		internal static IMagickColor<QType> ConvertToExternal(ColorRGBA i, int channelCount)
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
