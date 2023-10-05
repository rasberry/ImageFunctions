using System;
using System.Drawing;

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
		public ISampler Sampler = Registry.DefaultIFResampler;
	}
}