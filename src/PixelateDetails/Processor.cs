using System;
using System.Drawing;
using ImageFunctions.Helpers;
using System.Collections.Generic;

namespace ImageFunctions.PixelateDetails
{
	public class Processor : IFAbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			// TODO use proucer comsumer model to parallelize
			// https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library
			// https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-implement-a-producer-consumer-dataflow-pattern


			SplitAndAverage(Source,Bounds);
		}

		void SplitAndAverage(IFImage frame, Rectangle rect)
		{
			//Log.Debug("SplitAndAverage "+rect.DebugString());
			if (rect.Width < 1 || rect.Height < 1) { return; }

			int chunkW,chunkH,remW,remH;
			if (O.UseProportionalSplit) {
				chunkW = (int)((double)rect.Width  / O.ImageSplitFactor);
				chunkH = (int)((double)rect.Height / O.ImageSplitFactor);
			}
			else {
				int dim = Math.Min(rect.Width,rect.Height);
				chunkW = chunkH = (int)((double)dim / O.ImageSplitFactor);
			}
			if (chunkW < 1 || chunkH < 1) { return; }
			remW = rect.Width  % chunkW;
			remH = rect.Height % chunkH;

			//Log.Debug("["+rect.Width+"x"+rect.Height+"] sf="+ImageSplitFactor+" P="+UseProportionalSplit+" cW="+chunkW+" cH="+chunkH+" rW="+remW+" rH="+remH);

			int gridW = rect.Width / chunkW;
			int gridH = rect.Height / chunkH;
			var grid = new List<SortPair>(gridW * gridH);

			int xStart = rect.Left;
			int xEnd = rect.Right - chunkW;
			int yStart = rect.Top;
			int yEnd = rect.Bottom - chunkH;

			//Log.Debug("xs="+xStart+" xe="+xEnd+" ys="+yStart+" ye="+yEnd);

			//using w and h to account for remainders
			int w=0,h=0;
			for(int y = yStart; y <= yEnd; y += h) {
				for(int x = xStart; x <= xEnd; x += w) {
					w = chunkW + (x == xStart ? remW : 0);
					h = chunkH + (y == yStart ? remH : 0);
					var r = new Rectangle(x,y,w,h);
					//Log.Debug("r = "+r.DebugString());
					var sp = SortPair.FromRect(frame,r);
					grid.Add(sp);
				}
			}

			//Log.Debug("grid count = "+grid.Count);
			grid.Sort();

			int recurseCount = O.DescentFactor < 1.0
				? (int)(grid.Count * O.DescentFactor)
				: (int)O.DescentFactor
			;
			recurseCount = Math.Max(1,Math.Min(recurseCount,grid.Count - 1));
			//Log.Debug("c="+grid.Count+" df = "+DescentFactor+" rc="+recurseCount);

			for(int g=grid.Count-1; g>=0; g--) {
				var sp = grid[g];
				//Log.Debug("sorted "+g+" "+sp.Value+" "+sp.Rect.DebugString());
				if (g < recurseCount) {
					SplitAndAverage(frame,sp.Rect);
				} else {
					ReplaceWithColor(frame,sp.Rect,FindAverage(frame,sp.Rect));
				}
			}
		}

		static double Measure(IFImage frame, Rectangle rect)
		{
			if (rect.Width < 2 || rect.Height < 2) {
				var c = frame[rect.Left,rect.Top];
				double pxvc = GetPixelValue(c);
				return pxvc;
			}

			double sum = 0.0;
			for(int y = rect.Top; y < rect.Bottom; y++) {
				for(int x = rect.Left; x < rect.Right; x++) {
					int num = 0;
					IFColor? c = null,n = null,e = null,s = null,w = null;
					c = frame[x,y];
					if (x > rect.Left)     { w = frame[x-1,y]; num++; }
					if (x < rect.Right-1)  { e = frame[x+1,y]; num++; }
					if (y > rect.Top)      { n = frame[x,y-1]; num++; }
					if (y < rect.Bottom-1) { s = frame[x,y+1]; num++; }
					double pxvc = GetPixelValue(c);
					sum += ((
						  Math.Abs(pxvc - GetPixelValue(n))
						+ Math.Abs(pxvc - GetPixelValue(e))
						+ Math.Abs(pxvc - GetPixelValue(s))
						+ Math.Abs(pxvc - GetPixelValue(w))
					) / num);
				}
			}
			
			//Log.Debug("measure sum="+sum+" den="+rect.Width * rect.Height);
			return sum / (rect.Width * rect.Height);
		}

		IFColor FindAverage(IFImage frame, Rectangle rect)
		{
			double r=0.0, g=0.0, b=0.0;

			for(int y = rect.Top; y < rect.Bottom; y++) {
				for(int x = rect.Left; x < rect.Right; x++) {
					var c = frame[x,y];
					//TODO maybe multiply by alpha ?
					r += c.R;
					g += c.G;
					b += c.B;
				}
			}
			double den = rect.Width * rect.Height;
			var avg = new IFColor(
				 r / den
				,g / den
				,b / den
				,1.0
			);
			return avg;
		}

		void ReplaceWithColor(IFImage frame, Rectangle rect, IFColor color)
		{
			// Log.Debug("ReplaceWithColor r="+rect.DebugString());
			//var red = default(TPixel);
			//red.FromRgba32(Rgba32.Red);

			for(int y = rect.Top; y < rect.Bottom; y++) {
				for(int x = rect.Left; x < rect.Right; x++) {
					//bool onBorder =
					//	x == rect.Left || x == rect.Right-1
					//	|| y == rect.Top || y == rect.Bottom-1
					//;
					//if (onBorder) {
					//	frame[x,y] = red;
					//} else {
						frame[x,y] = color;
					//}
				}
			}
		}

		static double GetPixelValue(IFColor? p)
		{
			if (!p.HasValue) { return 0.0; }
			var c = p.Value;
			double val = (c.R + c.G + c.B)/3.0;
			//Log.Debug("GetPixelValue val="+val+" r="+c.R+" g="+c.G+" b="+c.B);
			return val;
		}

		struct SortPair : IComparable
		{
			public double Value;
			public Rectangle Rect;

			public static bool operator <(SortPair a, SortPair b) {
				return a.Value < b.Value;
			}
			public static bool operator >(SortPair a, SortPair b) {
				return a.Value > b.Value;
			}

			public static SortPair FromRect(IFImage frame,Rectangle r)
			{
				double m = Measure(frame,r);
				return new SortPair {
					Value = m, Rect = r
				};
			}

			public int CompareTo(object obj)
			{
				var sub = (SortPair)obj;
				return -1 * this.Value.CompareTo(sub.Value);
			}
		}

		public override void Dispose() {}
	}

}
