using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions.Deform
{
	public class Options
	{
		public Point? CenterPx = null;
		public PointF? CenterPp = null;
		public Function.Mode WhichMode = Function.Mode.Polynomial;
		public double Power = 2.0;
		public IResampler Sampler = Registry.DefaultResampler;
	}
}