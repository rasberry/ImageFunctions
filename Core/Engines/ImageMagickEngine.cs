using ImageMagick;
using QType = System.Single;

namespace ImageFunctions.Core.Engines;

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

	public void SaveImage(ILayers layers, string path, string format = null)
	{
		if (layers.Count == 0) {
			throw Squeal.NoLayers();
		}

		MagickFormat mf;
		if (String.IsNullOrWhiteSpace(format)) {
			if (layers.Count == 1) {
				mf = MagickFormat.Png;
			}
			else {
				mf = MagickFormat.Tif;
			}
		}
		else {
			bool good = Enum.TryParse(format,true,out mf);
			if (good) {
				var info = MagickFormatInfo.Create(mf);
				good = info != null && info.SupportsWriting;
			}

			if (!good) {
				throw Squeal.FormatIsNotSupported(format);
			}
		}

		//make sure the output file has the right extension
		path = Path.ChangeExtension(path,mf.ToString());

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
				throw Squeal.NotSupportedCMYK();
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
				throw Squeal.NotSupportedChannelCount(channelCount);
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
				throw Squeal.NotSupportedChannelCount(channelCount);
		}
	}
}
