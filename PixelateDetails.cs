using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;

namespace ImageFunctions
{
	public class PixelateDetails<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rectangle, Configuration config)
		{
			SplitAndAverage(frame,rectangle,config);
		}

		void SplitAndAverage(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			if (rect.Width < 2 || rect.Height < 2) { return; }

			int mx = rect.Width / 2;
			int my = rect.Height / 2;
			
			Rectangle rTL,rTR,rBL,rBR;
			double mTL = Measure(frame,rTL = new Rectangle(rect.Left + 00,rect.Top + 00,mx,my));
			double mTR = Measure(frame,rTR = new Rectangle(rect.Left + mx,rect.Top + 00,mx,my));
			double mBL = Measure(frame,rBL = new Rectangle(rect.Left + 00,rect.Top + my,mx,my));
			double mBR = Measure(frame,rBR = new Rectangle(rect.Left + mx,rect.Top + my,mx,my));

			// https://stackoverflow.com/questions/25070577/sort-4-numbers-without-array
			SortPair a = new SortPair { Value = mTL, Rect = rTL };
			SortPair b = new SortPair { Value = mTR, Rect = rTR };
			SortPair c = new SortPair { Value = mBL, Rect = rBL };
			SortPair d = new SortPair { Value = mBR, Rect = rBR };
			if (a < b) { Swap(ref a,ref b); }
			if (c < d) { Swap(ref c,ref d); }
			if (a < c) { Swap(ref a,ref c); }
			if (b < d) { Swap(ref b,ref d); }
			if (b < c) { Swap(ref b,ref c); }
			
			Log.Debug("mTL="+mTL+" mTR="+mTR+" mBL="+mBL+" mBR="+mBR);
			Log.Debug("F = "+a.Value);
			Log.Debug("S = "+b.Value);

			SplitAndAverage(frame,a.Rect,config);
			SplitAndAverage(frame,b.Rect,config);
			SplitAndAverage(frame,c.Rect,config);
			ReplaceWithColor(frame,d.Rect,FindAverage(frame,d.Rect));
		}

		double Measure(ImageFrame<TPixel> frame, Rectangle rect)
		{
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
			
			return sum;
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
			var span = frame.GetPixelSpan();

			for(int y = rect.Top; y < rect.Bottom; y++) {
				for(int x = rect.Left; x < rect.Right; x++) {
					int off = y * frame.Width + x;
					span[off] = color;
				}
			}
		}

		double GetPixelValue(TPixel? p)
		{
			if (!p.HasValue) { return 0.0; }
			Rgba32 c = Rgba32.Black;
			p.Value.ToRgba32(ref c);
			return (c.R + c.G + c.B)/3.0;
		}

		void Swap<T>(ref T a, ref T b) where T : struct
		{
			T tmp = a;
			a = b;
			b = tmp;
		}

		struct SortPair
		{
			public double Value;
			public Rectangle Rect;

			public static bool operator <(SortPair a, SortPair b) {
				return a.Value < b.Value;
			}
			public static bool operator >(SortPair a, SortPair b) {
				return a.Value > b.Value;
			}
		}
	}
}
