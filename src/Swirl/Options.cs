using System;
using System.Drawing;

namespace ImageFunctions.Swirl
{
	public class Options
	{
		public Point? CenterPx = null;
		public PointF? CenterPp = null;
		public int? RadiusPx = null;
		public double? RadiusPp = null;
		public double Rotations = 0.9;
		public bool CounterClockwise = false;
		public IFResampler Sampler = Registry.DefaultIFResampler;
		public IMeasurer Measurer = Registry.DefaultMetric;
	}
}