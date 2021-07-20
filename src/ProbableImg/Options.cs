using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageFunctions.ProbableImg
{
	public class Options
	{
		public int? RandomSeed = null;
		public List<StartPoint> StartLoc = new List<StartPoint>();
		public Rectangle OutBounds = Rectangle.Empty;
		//public IMeasurer Measurer = Registry.DefaultMetric;
		//public ISampler Sampler = Registry.DefaultIFResampler;
	}

	public struct StartPoint
	{
		public bool IsLinear;
		//proportional
		public double PX;
		public double PY;
		//linear
		public int LX;
		public int LY;

		public static StartPoint FromLinear(int x,int y) {
			return new StartPoint {
				IsLinear = true,
				PX = 0.0, PY = 0.0,
				LX = x, LY = y
			};
		}
		public static StartPoint FromPro(double x, double y) {
			return new StartPoint {
				IsLinear = false,
				PX = x, PY = y,
				LX = 0, LY = 0
			};
		}
	}
}

// TODO allow output image to be a different size than the input
// TODO maybe add spiral traversal
// TODO maybe go through original image pixel-by-pixel and run the random color updater
// TODO possibly use huge structures to store profile ?
// TODO maybe use tiles instead of pixels ?
// TODO maybe research flood fill so i don't use random stack ?
