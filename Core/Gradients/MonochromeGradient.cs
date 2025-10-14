namespace ImageFunctions.Core.Gradients;

public class MonochromeGradient : IColorGradient
{
	public ColorRGBA GetColor(double position)
	{
		position = Math.Clamp(position,0.0,1.0);
		double pct = Math.Log(1.0 + position,2.0);
		return new ColorRGBA(pct,pct,pct,1.0);
	}
}