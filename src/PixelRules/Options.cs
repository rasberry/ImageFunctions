using System;

namespace ImageFunctions.PixelRules
{
	public class Options
	{
		public Function.Mode WhichMode = Function.Mode.StairCaseDescend;
		public int Passes = 1;
		public int MaxIters = 100;
		public IMeasurer Measurer = Registry.DefaultMetric;
		public ISampler Sampler = Registry.DefaultIFResampler;
	}
}