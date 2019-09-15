using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions.ZoomBlur
{
	public class Options
	{
		public IResampler Sampler = Registry.DefaultResampler;
		public IMeasurer Measurer = Registry.DefaultMetric;
		public Point? CenterPx = null;
		public PointF? CenterRt = null;
		public double ZoomAmount = 1.1;
	}
}