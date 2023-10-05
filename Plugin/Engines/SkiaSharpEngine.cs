using System.Text;
using ImageFunctions.Core;
using SkiaSharp;

namespace ImageFunctions.Plugin.Engines;

public class SkiaSharpEngine : IImageEngine, IDrawEngine
{
	public IEnumerable<ImageFormat> Formats()
	{
		foreach(var f in Enum.GetValues<SKEncodedImageFormat>()) {
			string desc = GetFormatDesc(f);
			if (desc == null) { continue; } //not supported

			yield return new ImageFormat(
				f.ToString(),
				GetFormatDesc(f),
				true, true,
				false //as far as i can tell SkiaSharp does not support writing multi-layer images
			);
		}
	}

	// https://learn.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/bitmaps/saving
	static string GetFormatDesc(SKEncodedImageFormat format)
	{
		switch(format) {
			case SKEncodedImageFormat.Astc: return "Adaptive Scalable Texture Compression";
			case SKEncodedImageFormat.Bmp:  return "Windows Bitmap";
			case SKEncodedImageFormat.Dng:  return "Adobe Digital Negative";
			case SKEncodedImageFormat.Gif:  return "Graphics Interchange Format";
			case SKEncodedImageFormat.Heif: return "High Efficiency Image File format";
			case SKEncodedImageFormat.Ico:  return "Windows icon images";
			case SKEncodedImageFormat.Jpeg: return "Joint Photographic Experts Group";
			case SKEncodedImageFormat.Ktx:  return "Khronos texture format for OpenGL";
			case SKEncodedImageFormat.Pkm:  return "Custom format for GrafX2";
			case SKEncodedImageFormat.Png:  return "Portable Network Graphics";
			case SKEncodedImageFormat.Wbmp: return "Wireless Application Protocol Bitmap Format (1 bit per pixel)";
			case SKEncodedImageFormat.Webp: return "Google WebP format";
		}

		return null;
	}

	static string GetExtension(SKEncodedImageFormat format)
	{
		switch(format) {
			case SKEncodedImageFormat.Astc: return ".astc";
			case SKEncodedImageFormat.Bmp:  return ".bmp";
			case SKEncodedImageFormat.Dng:  return ".dng";
			case SKEncodedImageFormat.Gif:  return ".gif";
			case SKEncodedImageFormat.Heif: return ".heif";
			case SKEncodedImageFormat.Ico:  return ".ico";
			case SKEncodedImageFormat.Jpeg: return ".jpg";
			case SKEncodedImageFormat.Ktx:  return ".ktx";
			case SKEncodedImageFormat.Pkm:  return ".pkm";
			case SKEncodedImageFormat.Png:  return ".png";
			case SKEncodedImageFormat.Wbmp: return ".wbmp";
			case SKEncodedImageFormat.Webp: return ".webp";
		}

		return null;
	}

	public void LoadImage(ILayers layers, string path)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		var fileStream = File.OpenRead(path);
		using var skStream = new SKManagedStream(fileStream);
		using var codec = SKCodec.Create(skStream,out var result);
		if (result != SKCodecResult.Success) {
			throw Squeal.CouldNotLoadFile(path, result.ToString());
		}

		string name = Path.GetFileName(path);

		//images with no frames are normal images with one layer
		if (codec.FrameCount == 0) {
			var bitmap = SKBitmap.Decode(codec);
			layers.Push(new InSkiaCanvas(bitmap), name);
			return;
		}

		//handle multilayer images (only gif ?)
		// https://learn.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/bitmaps/animating
		int FrameCount = codec.FrameCount;
		string ext = Path.GetExtension(path);
		for (int frame = 0; frame < FrameCount; frame++) {
			// Create a full-color bitmap for each frame
			SKImageInfo imageInfo = new SKImageInfo(codec.Info.Width, codec.Info.Height);
			var bitmap = new SKBitmap(imageInfo);

			// Get the address of the pixels in that bitmap
			IntPtr pointer = bitmap.GetPixels();

			// Create an SKCodecOptions value to specify the frame
			SKCodecOptions codecOptions = new SKCodecOptions(frame);

			// Copy pixels from the frame into the bitmap
			codec.GetPixels(imageInfo, pointer, codecOptions);

			layers.Push(new InSkiaCanvas(bitmap), $"{name}.{frame + 1}");
		}
	}

	public ICanvas NewCanvas(int width, int height)
	{
		return new InSkiaCanvas(width,height);
	}

	public void SaveImage(ILayers layers, string path, string format = null)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if (layers.Count < 1) {
			throw Squeal.NoLayers();
		}

		SKEncodedImageFormat skFormat;
		if (format == null) {
			skFormat = SKEncodedImageFormat.Png;
		}
		else {
			if (!Enum.TryParse(format, true, out skFormat)) {
				throw Squeal.FormatIsNotSupported(format);
			}
		}

		//make sure the output file has the right extension
		path = Path.ChangeExtension(path,GetExtension(skFormat));

		if (layers.Count == 1) {
			var canvas = (InSkiaCanvas)layers.First();
			WriteImage(canvas.Bitmap, path, skFormat);
		}
		else {
			string ext = Path.GetExtension(path);

			int count = 1;
			foreach(var lay in layers) {
				var canvas = (InSkiaCanvas)lay;
				string name = Path.ChangeExtension(path,$"{count}{ext}");
				WriteImage(canvas.Bitmap, name, skFormat);
			}
		}
	}

	const int SkiaMaxQuality = 100;
	static void WriteImage(SKBitmap bitmap, string path, SKEncodedImageFormat format)
	{
		using var bufferStream = new MemoryStream();
		using var imageStream = new SKManagedWStream(bufferStream);

		bitmap.Encode(imageStream, format, SkiaMaxQuality);

		using var fileStream = File.Create(path);
		bufferStream.Seek(0, SeekOrigin.Begin);
		bufferStream.CopyTo(fileStream);
	}

	public void DrawLine(ICanvas image, ColorRGBA color, PointD p0, PointD p1, double width = 1)
	{
		var canvas = (InSkiaCanvas)image;
		var tools = new SKCanvas(canvas.Bitmap);
		var paint = new SKPaint {
			StrokeCap = SKStrokeCap.Butt,
			Style = SKPaintStyle.Stroke,
			Color = ToSKColor(color),
			StrokeWidth = (float)width
		};
		tools.DrawLine((float)p0.X, (float)p0.Y, (float)p1.X, (float)p1.Y, paint);
	}

	static ColorRGBA ToColorRGBA(SKColor color)
	{
		return ColorRGBA.FromRGBA255(
			color.Red, color.Green, color.Blue, color.Alpha
		);
	}

	static SKColor ToSKColor(ColorRGBA color)
	{
		return new SKColor(
			(byte)(color.R * 255.0),
			(byte)(color.G * 255.0),
			(byte)(color.B * 255.0),
			(byte)(color.A * 255.0)
		);
	}

	class InSkiaCanvas : ICanvas
	{
		public InSkiaCanvas(SKBitmap bitmap) {
			Bitmap = bitmap;
		}
		public InSkiaCanvas(int width, int height) {
			Bitmap = new SKBitmap(width,height,SKColorType.Rgba8888,SKAlphaType.Unpremul);
		}

		//TODO pixel access is crazy slow
		// it would be better to maintain our own data structure
		// and copy in / out during load / save
		public ColorRGBA this[int x, int y] {
			get {
				var pix = Bitmap.GetPixel(x,y);
				return ToColorRGBA(pix);
			}
			set {
				var pix = ToSKColor(value);
				Bitmap.SetPixel(x,y,pix);
			}
		}

		public int Width { get { return Bitmap.Width; }}
		public int Height { get { return Bitmap.Height; }}

		public void Dispose()
		{
			Bitmap.Dispose();
			Bitmap = null;
		}

		internal SKBitmap Bitmap;
	}
}
