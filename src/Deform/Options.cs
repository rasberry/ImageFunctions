using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions.Deform
{
	public enum Mode {
		None = 0,
		Polynomial = 1,
		Inverted = 2
	}

	public class Options
	{
		public Point? CenterPx = null;
		public PointF? CenterPp = null;
		public Mode WhichMode = Mode.Polynomial;
		public double Power = 2.0;
		public IResampler Sampler = Registry.DefaultResampler;
	}
}