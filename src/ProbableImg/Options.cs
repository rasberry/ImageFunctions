using System;
using System.Collections.Generic;
using System.Drawing;

namespace ImageFunctions.ProbableImg
{
	public class Options
	{
		public int? RandomSeed = null;
		public int? TotalNodes = null;
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
