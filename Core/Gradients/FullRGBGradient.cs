namespace ImageFunctions.Core.Gradients;

/// <summary>Gradient from black to white and cycles thought all hues 4x</summary>
public class FullRGBGradient : IColorGradient
{
	/// <inheritdoc/>
	public ColorRGBA GetColor(double position)
	{
		position = Math.Clamp(position, 0.0, 1.0);

		// //iterate HSL L=[0 to 1] S=1 H[0 to 1]
		double l = position;
		double s = 1.0;
		double h = (position % 4.0) / 4.0; //4 slows down the cycle 4x

		var rgb = ColorSpace.ToNative(new ColorSpace.ColorSpaceHsl.HSL(h, s, l));
		return rgb;
	}
	static readonly ColorSpace.ColorSpaceHsl ColorSpace = new();
}
