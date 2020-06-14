using System;
using System.Drawing;

namespace ImageFunctions.ZoomBlur
{
	public class Options
	{
		public IFResampler Sampler = Registry.DefaultIFResampler;
		public IMeasurer Measurer = Registry.DefaultMetric;
		public Point? CenterPx = null;
		public PointF? CenterRt = null;
		public double ZoomAmount = 1.1;
	}
}