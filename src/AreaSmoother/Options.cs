using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace ImageFunctions.AreaSmoother
{
	public class Options
	{
		public int TotalTries = 7;
		public IResampler Sampler = Registry.DefaultResampler;
		public IMeasurer Measurer = Registry.DefaultMetric;
	}
}