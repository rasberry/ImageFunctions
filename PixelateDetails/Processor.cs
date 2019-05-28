using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;

namespace ImageFunctions.PixelateDetails
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public double ImageSplitFactor { get; set; } = 2.0;
		public bool UseProportionalSplit { get; set; } = false;
		public double DescentFactor { get; set; } = 0.5;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rectangle, Configuration config)
		{
			//rectangle = new Rectangle(0,0,400,400);
			SplitAndAverage(frame,rectangle,config);
		}

		void SplitAndAverage(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			//Log.Debug("SplitAndAverage "+rect.DebugString());
			if (rect.Width < 1 || rect.Height < 1) { return; }

			int chunkW,chunkH,remW,remH;
			if (UseProportionalSplit) {
				chunkW = (int)((double)rect.Width  / ImageSplitFactor);
				chunkH = (int)((double)rect.Height / ImageSplitFactor);
			}
			else {
				int dim = Math.Min(rect.Width,rect.Height);
				chunkW = chunkH = (int)((double)dim / ImageSplitFactor);
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

			int recurseCount = DescentFactor < 1.0
				? (int)(grid.Count * DescentFactor)
				: (int)DescentFactor
			;
			recurseCount = Math.Max(1,Math.Min(recurseCount,grid.Count - 1));

			for(int g=grid.Count-1; g>=0; g--) {
				var sp = grid[g];
				//Log.Debug("sorted "+g+" "+sp.Value+" "+sp.Rect.DebugString());
				if (g < recurseCount) {
					SplitAndAverage(frame,sp.Rect,config);
				} else {
					ReplaceWithColor(frame,sp.Rect,FindAverage(frame,sp.Rect));
				}
			}
		}

		static double Measure(ImageFrame<TPixel> frame, Rectangle rect)
		{
			if (rect.Width < 2 || rect.Height < 2) {
				var c = frame.GetPixelRowSpan(rect.Top)[rect.Left];
				double pxvc = GetPixelValue(c);
				return pxvc;
			}

			double sum = 0.0;
			for(int y = rect.Top; y < rect.Bottom; y++) {
				for(int x = rect.Left; x < rect.Right; x++) {
					int num = 0;
					TPixel? c = null,n = null,e = null,s = null,w = null;
					c = frame.GetPixelRowSpan(y)[x];
					if (x > rect.Left)     { w = frame.GetPixelRowSpan(y)[x-1]; num++; }
					if (x < rect.Right-1)  { e = frame.GetPixelRowSpan(y)[x+1]; num++; }
					if (y > rect.Top)      { n = frame.GetPixelRowSpan(y-1)[x]; num++; }
					if (y < rect.Bottom-1) { s = frame.GetPixelRowSpan(y+1)[x]; num++; }
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

		TPixel FindAverage(ImageFrame<TPixel> frame, Rectangle rect)
		{
			var span = frame.GetPixelSpan();
			double r=0.0, g=0.0, b=0.0;

			for(int y = rect.Top; y < rect.Bottom; y++) {
				for(int x = rect.Left; x < rect.Right; x++) {
					int off = y * frame.Width + x;
					Rgba32 c = default(Rgba32);
					span[off].ToRgba32(ref c);
					//TODO maybe multiply by alpha ?
					r += c.R;
					g += c.G;
					b += c.B;
				}
			}
			double den = rect.Width * rect.Height;
			var avg = new Rgba32(
				 (byte)(r / den)
				,(byte)(g / den)
				,(byte)(b / den)
			);
			var pix = default(TPixel);
			pix.FromRgba32(avg);
			return pix;
		}

		void ReplaceWithColor(ImageFrame<TPixel> frame, Rectangle rect, TPixel color)
		{
			// Log.Debug("ReplaceWithColor r="+rect.DebugString());
			var span = frame.GetPixelSpan();
			//var red = default(TPixel);
			//red.FromRgba32(Rgba32.Red);

			for(int y = rect.Top; y < rect.Bottom; y++) {
				for(int x = rect.Left; x < rect.Right; x++) {
					int off = y * frame.Width + x;
					//bool onBorder =
					//	x == rect.Left || x == rect.Right-1
					//	|| y == rect.Top || y == rect.Bottom-1
					//;
					//if (onBorder) {
					//	span[off] = red;
					//} else {
						span[off] = color;
					//}
				}
			}
		}

		static double GetPixelValue(TPixel? p)
		{
			if (!p.HasValue) { return 0.0; }
			Rgba32 c = default(Rgba32);
			p.Value.ToRgba32(ref c);
			double val = (c.R + c.G + c.B)/3.0;
			//Log.Debug("GetPixelValue val="+val+" r="+c.R+" g="+c.G+" b="+c.B);
			return val;
		}

		void Swap<T>(ref T a, ref T b) where T : struct
		{
			T tmp = a;
			a = b;
			b = tmp;
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

			public static SortPair FromRect(ImageFrame<TPixel> frame,Rectangle r)
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
	}
}
