namespace ImageFunctions.Core.ColorSpace;

// https://en.wikipedia.org/wiki/YCbCr
public class ColorSpaceRGB : IColor3Space<ColorRGBA>, ILumaColorSpace
{
	public ILuma GetLuma(in ColorRGBA o)
	{
		return o;
	}

	public ColorRGBA ToNative(in ColorRGBA o)
	{
		return o;
	}

	ColorRGBA IColor3Space.ToNative(in IColor3 o)
	{
		return o is ColorRGBA n ? ToNative(n) : ToNative(new ColorRGBA(o.C1, o.C2, o.C3, o.A));
	}

	public ColorRGBA ToSpace(in ColorRGBA o)
	{
		return o;
	}

	IColor3 IColor3Space.ToSpace(in ColorRGBA o)
	{
		return o;
	}

	public ColorSpaceInfo Info {
		get {
			return new ColorSpaceInfo {
				Description = "Red, Green, Blue (passthrough)",
				ComponentNames = new[] { "R", "G", "B", "A" }
			};
		}
	}
}
