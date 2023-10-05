using System;

namespace ImageFunctions.AreaSmoother
{
	public class Options
	{
		public int TotalTries = 7;
		public ISampler Sampler = Registry.DefaultIFResampler;
		public IMeasurer Measurer = Registry.DefaultMetric;
	}
}