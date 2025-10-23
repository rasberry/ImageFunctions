namespace ImageFunctions.Core;

/// <summary>
/// Represents a color gradient
/// </summary>
public interface IColorGradient
{
	/// <summary>Gets the color at the given index position</summary>
	/// <param name="position">position between 0.0 and 1.0 (inclusive)</param>
	ColorRGBA GetColor(double position);
}
