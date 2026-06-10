using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics;

namespace ImageFunctions.Core.Engines;

#pragma warning disable CA2000 //Dispose objects before losing scope - dispose is handeled by layers

public class SixLaborsEngine : IImageEngine, IDrawEngine
{
	/// <inheritdoc/>
	public void LoadImage(ILayers layers, IFileClerk clerk, string layerName = null)
	{
		if(layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if(clerk == null) {
			throw Squeal.ArgumentNull(nameof(clerk));
		}

		layerName ??= clerk.GetLabel(null);
		var image = Image.Load<Rgba256>(clerk.ReadStream());

		//for images with one frame just use the original
		if(image.Frames.Count == 1) {
			var lay = new SLCanvas(image);
			layers.Push(lay, layerName);
			// don't dispose of image since were using it directly
			return;
		}

		//otherwise copy each frame to a new image.
		// dispose of image since were copying the data
		int count = 0;
		using(image) {
			foreach(var frame in image.Frames) {
				int w = frame.Width;
				int h = frame.Height;

				var layer = new Image<Rgba256>(w, h);
				var memory = new Rgba256[w * h];
				var span = new Span<Rgba256>(memory);

				frame.CopyPixelDataTo(span);
				var copy = Image.LoadPixelData<Rgba256>(span, w, h);
				var lay = new SLCanvas(copy);
				//push to the end so that the order does not get reversed
				layers.PushAt(layers.Count, lay, clerk.GetLabel(layerName, null, $"{++count}"));
			}
		}
	}

	/// <inheritdoc/>
	public void SaveImage(ILayers layers, IFileClerk clerk, string format = null)
	{
		if(layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if(layers.Count == 0) {
			throw Squeal.NoLayers();
		}
		if(clerk == null) {
			throw Squeal.ArgumentNull(nameof(clerk));
		}

		IImageFormat sixFormat;
		var ifm = Configuration.Default.ImageFormatsManager;
		if(format != null) {
			if(!ifm.TryFindFormatByFileExtension(format, out sixFormat)) {
				throw Squeal.FormatIsNotSupported(format);
			}
		}
		else {
			if(layers.Count == 1) {
				sixFormat = SixLabors.ImageSharp.Formats.Png.PngFormat.Instance;
			}
			else {
				sixFormat = SixLabors.ImageSharp.Formats.Tiff.TiffFormat.Instance;
			}
		}

		//make sure the output file has the right extension
		//filePath = Path.ChangeExtension(filePath, GetBestExtension(sixFormat));

		//.. maybe this is trivial but wanted to cover my bases
		//h = has multiple layers
		//c = format supports layers
		//o = save one image
		//m = save multiple images
		//h c o m
		//0 0 1 0
		//0 1 1 0
		//1 0 0 1
		//1 1 1 0

		var ext = GetBestExtension(sixFormat);
		bool hasMulti = layers.Count > 1;
		bool canMulti = FormatSupportsFrames(sixFormat);

		if(hasMulti && !canMulti) {
			//save each frame as it's own image
			var enc = ifm.GetEncoder(sixFormat);

			var streamFactory = clerk.WriteFactory(ext);
			foreach(var lay in layers) {
				var native = (SLCanvas)lay.Canvas;
				var img = native.Image;
				var stream = streamFactory();
				img.Save(stream, enc);
			}
		}
		else {
			//copy all frames into a single image
			var firstImg = (SLCanvas)layers.First().Canvas;
			using var final = new Image<Rgba256>(firstImg.Width, firstImg.Height);

			foreach(var lay in layers) {
				var native = (SLCanvas)lay.Canvas;
				var img = native.Image;

				//each layer should only have a single frame
				final.Frames.AddFrame(img.Frames.RootFrame);
			}

			//we have to remove the top auto-created frame
			// seems to be fairly difficult to start a new image with the contents of a frame
			final.Frames.RemoveFrame(0);

			var enc = ifm.GetEncoder(sixFormat);
			var stream = clerk.WriteStream(ext);
			final.Save(stream, enc);
		}
	}

	/// <inheritdoc/>
	public ICanvas NewCanvas(int width, int height)
	{
		var native = new Image<Rgba256>(width, height);
		var img = new SLCanvas(native);
		return img;
	}

	/// <inheritdoc/>
	public void DrawLine(ICanvas image, ColorRGBA color, PointD p0, PointD p1, double width = 1.0)
	{
		if(image == null) {
			throw Squeal.ArgumentNull(nameof(image));
		}

		var rgba = new Rgba256 { R = color.R, G = color.G, B = color.B, A = color.A };
		var f0 = new PointF((float)p0.X, (float)p0.Y);
		var f1 = new PointF((float)p1.X, (float)p1.Y);

		var wrap = (SLCanvas)image;
		wrap.Image.Mutate(ctx => ctx.Paint(canvas => {
			// ctx.SetGraphicsOptions(opts => {
			// 	opts.Antialias = true;
			// });
			var pen = Pens.Solid(Color.FromPixel(rgba), (float)width);
			canvas.DrawLine(pen, f0, f1);
		}));
	}

	/// <inheritdoc/>
	public IEnumerable<ImageFormat> Formats()
	{
		var Ifm = Configuration.Default.ImageFormatsManager;
		foreach(var f in Ifm.ImageFormats) {
			var enc = Ifm.GetEncoder(f);
			var dec = Ifm.GetDecoder(f);

			yield return new ImageFormat(
				f.Name,
				$"{f.DefaultMimeType} [{String.Join(",", f.FileExtensions)}]",
				enc != null,
				dec != null,
				FormatSupportsFrames(f),
				GetBestExtension(f),
				GetMimeType(f)
			);
		}
	}

	bool FormatSupportsFrames(IImageFormat format)
	{
		bool isFramey = format switch {
			SixLabors.ImageSharp.Formats.Bmp.BmpFormat => false,
			SixLabors.ImageSharp.Formats.Gif.GifFormat => true,
			SixLabors.ImageSharp.Formats.Jpeg.JpegFormat => false,
			SixLabors.ImageSharp.Formats.Pbm.PbmFormat => false,
			SixLabors.ImageSharp.Formats.Png.PngFormat => false,
			SixLabors.ImageSharp.Formats.Qoi.QoiFormat => false,
			SixLabors.ImageSharp.Formats.Tga.TgaFormat => false,
			SixLabors.ImageSharp.Formats.Tiff.TiffFormat => true,
			SixLabors.ImageSharp.Formats.Webp.WebpFormat => false,
			SixLabors.ImageSharp.Formats.Exr.ExrFormat => false,
			SixLabors.ImageSharp.Formats.Ico.IcoFormat => true,
			SixLabors.ImageSharp.Formats.Cur.CurFormat => true,
			_ => throw Squeal.FormatIsNotSupported(format.Name)
		};
		return isFramey;
	}

	string GetBestExtension(IImageFormat format)
	{
		string ext = format switch {
			SixLabors.ImageSharp.Formats.Bmp.BmpFormat => ".bmp",
			SixLabors.ImageSharp.Formats.Gif.GifFormat => ".gif",
			SixLabors.ImageSharp.Formats.Jpeg.JpegFormat => ".jpg",
			SixLabors.ImageSharp.Formats.Pbm.PbmFormat => ".pbm",
			SixLabors.ImageSharp.Formats.Png.PngFormat => ".png",
			SixLabors.ImageSharp.Formats.Qoi.QoiFormat => ".qoi",
			SixLabors.ImageSharp.Formats.Tga.TgaFormat => ".tga",
			SixLabors.ImageSharp.Formats.Tiff.TiffFormat => ".tif",
			SixLabors.ImageSharp.Formats.Webp.WebpFormat => ".webp",
			SixLabors.ImageSharp.Formats.Exr.ExrFormat => ".exr",
			SixLabors.ImageSharp.Formats.Ico.IcoFormat => ".ico",
			SixLabors.ImageSharp.Formats.Cur.CurFormat => ".cur",
			_ => throw Squeal.FormatIsNotSupported(format.Name)
		};
		return ext;
	}

	string GetMimeType(IImageFormat format)
	{
		string ext = format switch {
			SixLabors.ImageSharp.Formats.Bmp.BmpFormat => "image/bmp",
			SixLabors.ImageSharp.Formats.Gif.GifFormat => "image/gif",
			SixLabors.ImageSharp.Formats.Jpeg.JpegFormat => "image/jpeg",
			SixLabors.ImageSharp.Formats.Pbm.PbmFormat => "image/x-portable-bitmap",
			SixLabors.ImageSharp.Formats.Png.PngFormat => "image/png",
			SixLabors.ImageSharp.Formats.Qoi.QoiFormat => "image/qoi", // https://gitlab.freedesktop.org/xdg/shared-mime-info/-/blob/master/data/freedesktop.org.xml.in
			SixLabors.ImageSharp.Formats.Tga.TgaFormat => "image/x-tga", // https://en.wikipedia.org/wiki/Truevision_TGA#MIME_type
			SixLabors.ImageSharp.Formats.Tiff.TiffFormat => "image/tiff",
			SixLabors.ImageSharp.Formats.Webp.WebpFormat => "image/webp",
			SixLabors.ImageSharp.Formats.Exr.ExrFormat => "image/x-exr",
			SixLabors.ImageSharp.Formats.Ico.IcoFormat => " image/x-icon", // https://en.wikipedia.org/wiki/ICO_%28file_format%29#MIME_type
			SixLabors.ImageSharp.Formats.Cur.CurFormat => " image/x-icon", // https://en.wikipedia.org/wiki/CUR_%28file_format%29#MIME_type
			_ => throw Squeal.FormatIsNotSupported(format.Name)
		};
		return ext;
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

class SLCanvas : ICanvas
{
	public SLCanvas(Image<Rgba256> image)
	{
		Image = image;
	}

	internal Image<Rgba256> Image;

	public ColorRGBA this[int x, int y] {
		get {
			var ipix = Image[x, y];
			return new ColorRGBA(ipix.R, ipix.G, ipix.B, ipix.A);
		}
		set {
			var xpix = new Rgba256 { R = value.R, G = value.G, B = value.B, A = value.A };
			Image[x, y] = xpix;
		}
	}

	public int Width { get { return Image.Width; } }
	public int Height { get { return Image.Height; } }

	public void Dispose()
	{
		Image?.Dispose();
	}
}

//Named Rgba256 following SixLabors convention (64*4)
//since native type is double, using a double based color should minimize conversions
// admittedly using 64bit floats for each component is slightly excessive but
// double and float seem to be about the same speed 🤷 so might as well use the better precision
struct Rgba256 : IEquatable<Rgba256>, IPixel<Rgba256>
{
	public Rgba256(double r, double g, double b, double a)
	{
		R = Math.Clamp(r, 0.0, 1.0);
		G = Math.Clamp(g, 0.0, 1.0);
		B = Math.Clamp(b, 0.0, 1.0);
		A = Math.Clamp(a, 0.0, 1.0);
	}

	public double R, G, B, A;

	public static bool operator ==(Rgba256 lhs, Rgba256 rhs)
	{
		return
			   lhs.R == rhs.R
			&& lhs.G == rhs.G
			&& lhs.B == rhs.B
			&& lhs.A == rhs.A
		;
	}

	public static bool operator !=(Rgba256 lhs, Rgba256 rhs)
	{
		return !(lhs == rhs);
	}

	public readonly bool Equals(Rgba256 compare)
	{
		return this == compare;
	}

	public override readonly bool Equals(object compare)
	{
		if(compare == null) { return false; }
		var right = (Rgba256)compare;
		return this == right;
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(R, G, B, A);
	}

	public static PixelOperations<Rgba256> CreatePixelOperations()
	{
		return new PixelOperations<Rgba256>();
	}

	public static PixelTypeInfo GetPixelTypeInfo()
	{
		Rgba32.GetPixelTypeInfo();
		return PixelTypeInfo.Create<Rgba256>(
			PixelComponentInfo.Create<Rgba256>(4, 64, 64, 64, 64),
			PixelColorType.RGB | PixelColorType.Alpha,
			PixelAlphaRepresentation.Unassociated
		);
	}

	public static Rgba256 FromAbgr32(Abgr32 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromArgb32(Argb32 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromBgr24(Bgr24 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromBgra32(Bgra32 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromBgra5551(Bgra5551 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromL16(L16 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromL8(L8 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromLa16(La16 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromLa32(La32 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromRgb24(Rgb24 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromRgb48(Rgb48 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromRgba32(Rgba32 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromRgba64(Rgba64 source) { return FromScaledVector4(source.ToScaledVector4()); }
	public static Rgba256 FromVector4(Vector4 source) { return FromScaledVector4(source); }
	public readonly Rgba32 ToRgba32() { return Rgba32.FromScaledVector4(ToScaledVector4()); }
	public readonly Vector4 ToVector4() { return ToScaledVector4(); }

	public static Rgba256 FromScaledVector4(Vector4 source)
	{
		return new Rgba256 {
			R = source.X,
			G = source.Y,
			B = source.Z,
			A = source.W,
		};
	}

	public readonly Vector4 ToScaledVector4()
	{
		return new Vector4((float)R, (float)G, (float)B, (float)A);
	}
}
