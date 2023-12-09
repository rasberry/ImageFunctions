namespace ImageFunctions.Core.ColorSpace;

// https://en.wikipedia.org/wiki/YCbCr
public class ColorSpaceRGB : IColor3Space<ColorRGBA>, ILumaColorSpace
{
	public ILuma GetLuma(in ColorRGBA o) {
		return o;
	}

	public ColorRGBA ToNative(in ColorRGBA o) {
		return o;
	}

	public ColorRGBA ToNative(in IColor3 o) {
		return (ColorRGBA)o;
	}

	public ColorRGBA ToSpace(in ColorRGBA o) {
		return o;
	}

	IColor3 IColor3Space.ToSpace(in ColorRGBA o) {
		return o;
	}
}