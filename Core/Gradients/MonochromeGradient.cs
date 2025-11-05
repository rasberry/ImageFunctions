namespace ImageFunctions.Core.Gradients;

/// <summary>Monochrome gradient from black to white</summary>
public class MonochromeGradient : IColorGradient
{
	/// <inheritdoc/>
	public ColorRGBA GetColor(double position)
	{
		position = Math.Clamp(position, 0.0, 1.0);
		//not sure why i'm using this but log(2) is nearly linear in this range
		double pct = Math.Log(1.0 + position, 2.0);
		return new ColorRGBA(pct, pct, pct, 1.0);
	}
}
