using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace ImageFunctions.UlamSpiral
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Options O = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			var span = frame.GetPixelSpan();
			var black = Color.Black.ToPixel<TPixel>();
			ImageHelpers.FillWithColor(frame,rect,O.ColorBack.ToPixel<TPixel>());

			if (O.UseFactorCount) {
				DrawFactors(frame,rect,config);
			}
			else {
				DrawPrimes(frame,rect,config);
			}
		}

		void DrawFactors(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			var srect = GetSpacedRectangle(rect);
			var (cx, cy) = GetCenterXY(srect);
			int maxFactor = int.MinValue;
			object maxLock = new object();

			//TODO surely there's a way to estimate the max factorcount so we don't have to actually find it
			var pb1 = new ProgressBar() { Prefix = "Step 1 " };
			using (pb1)
			{
				MoreHelpers.ThreadPixels(srect, config.MaxDegreeOfParallelism, (x, y) =>
				{
					long num = MapXY(x, y, cx, cy, srect.Width);
					int count = Primes.CountFactors(num);
					if (count > maxFactor)
					{
						//the lock ensures we don't accidentally miss a larger value
						lock (maxLock)
						{
							if (count > maxFactor) { maxFactor = count; }
						}
					}
				}, pb1);
			}
			//Log.Debug($"maxFactor={maxFactor}");

			var fcolor = O.ColorComposite.ToPixel<TPixel>();
			var bcolor = O.ColorBack.ToPixel<TPixel>();
			double factor = 1.0 / maxFactor;
			var drawFunc = GetDrawFunc();
			//int maxThreads = O.DotSize > O.Spacing ? 1 : config.MaxDegreeOfParallelism;

			var pb2 = new ProgressBar() { Prefix = "Step 2 " };
			using(pb2) {
				if (O.DotSize > O.Spacing) {
					DrawFactorsSlow(frame, config, srect, cx, cy, fcolor, bcolor, factor, drawFunc, pb2);
				}
				else {
					DrawFactorsFast(frame, config, srect, cx, cy, fcolor, bcolor, factor, drawFunc, pb2);
				}
			}

			//long pbcount = 0;
			//long max = (long)srect.Width * srect.Height * maxFactor;
			//var pb2 = new ProgressBar() { Prefix = "Step 2 " };
			//using (pb2) {
			//	for(int f = 1; f<= maxFactor; f++) {
			//		MoreHelpers.ThreadPixels(srect,maxThreads,(x,y) => {
			//			Interlocked.Increment(ref pbcount);
			//			pb2.Report((double)pbcount / max);
			//			long num = MapXY(x,y,cx,cy,srect.Width);
			//			int count = Primes.CountFactors(num,f);
			//			if (count <= f) {
			//				var color = ImageHelpers.BetweenColor(bcolor,fcolor,count * factor);
			//				drawFunc(frame,x,y,color,count * factor);
			//			}
			//		});
			//	}
			//}
		}

		void DrawFactorsFast(ImageFrame<TPixel> frame, Configuration config, Rectangle srect, int cx, int cy, TPixel fcolor, TPixel bcolor, double factor, Action<ImageFrame<TPixel>, int, int, TPixel, double> drawFunc, ProgressBar pb2)
		{
			MoreHelpers.ThreadPixels(srect, config.MaxDegreeOfParallelism, (x, y) =>
			{
				long num = MapXY(x, y, cx, cy, srect.Width);
				int count = Primes.CountFactors(num);
				var color = ImageHelpers.BetweenColor(bcolor, fcolor, count * factor);
				drawFunc(frame, x, y, color, count * factor);
			}, pb2);
		}

		void DrawFactorsSlow(ImageFrame<TPixel> frame, Configuration config, Rectangle srect, int cx, int cy, TPixel fcolor, TPixel bcolor, double factor, Action<ImageFrame<TPixel>, int, int, TPixel, double> drawFunc, ProgressBar pb2)
		{
			int s = O.Spacing;
			var list = new List<(int,int,int)>();
			double pbmax = srect.Width * srect.Height;
			
			double pbcount = 0;
			for(int y = srect.Top; y < srect.Bottom; y++) {
				for(int x = srect.Left; x < srect.Right; x++) {
					long num = MapXY(x, y, cx, cy, srect.Width);
					int count = Primes.CountFactors(num);
					if (count > s) {
						list.Add((count,x,y));
					}
					else {
						var color = ImageHelpers.BetweenColor(bcolor, fcolor, count * factor);
						drawFunc(frame, x, y, color, count * factor);
						pb2.Report(++pbcount/pbmax);
					}
				}
			}

			//Log.Debug($"leftover count: {list.Count}");
			list.Sort((a,b) => a.Item1 - b.Item1);

			foreach(var item in list) {
				var (count,x,y) = item;
				var color = ImageHelpers.BetweenColor(bcolor, fcolor, count * factor);
				drawFunc(frame, x, y, color, count * factor);
				pb2.Report(++pbcount/pbmax);
			}
		}

		void DrawPrimes(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			var srect = GetSpacedRectangle(rect);
			var color = O.ColorPrime.ToPixel<TPixel>();
			var (cx,cy) = GetCenterXY(srect);
			var drawFunc = GetDrawFunc();
			int maxThreads = O.DotSize > O.Spacing ? 1 : config.MaxDegreeOfParallelism;

			using (var progress = new ProgressBar()) {
				MoreHelpers.ThreadPixels(srect,maxThreads,(x,y) => {
					long num = MapXY(x,y,cx,cy,srect.Width);
					if (Primes.IsPrime(num)) {
						drawFunc(frame,x,y,color,1.0);
					}
				},progress);
			}
		}

		(int,int) GetCenterXY(Rectangle rect)
		{
			int cx = -O.CenterX.GetValueOrDefault(0);
			int cy = -O.CenterY.GetValueOrDefault(0);
			if (O.Mapping == PickMapping.Spiral) {
				cx = (rect.Width / 2) - cx;
				cy = (rect.Height / 2) - cy;
			}
			return (cx,cy);
		}

		long MapXY(int x,int y,int cx,int cy, int w = 0)
		{
			x+=1; //offset to correct x coord
			//these are all 1+ since ulams spiral starts at 1 not 0
			switch(O.Mapping)
			{
			case PickMapping.Linear:
				return 1 + MathHelpers.XYToLinear(x,y,w,cx,cy);
			case PickMapping.Diagonal:
				return 1 + MathHelpers.XYToDiagonal(x,y,cx,cy);
			case PickMapping.Spiral:
				return 1 + MathHelpers.XYToSpiralSquare(x,y,cx,cy);
			}
			return -1;
		}

		Rectangle GetSpacedRectangle(Rectangle rect)
		{
			Rectangle srect = new Rectangle(
				rect.X * O.Spacing,
				rect.Y * O.Spacing,
				rect.Width / O.Spacing,
				rect.Height / O.Spacing
			);
			return srect;
		}

		Action<ImageFrame<TPixel>, int, int, TPixel, double> GetDrawFunc()
		{
			if (O.DotSize.IsCloseTo(1.0)) {
				return DrawDotPixel;
			}
			else {
				return DrawDotSpere;
			}
		}

		void DrawDotPixel(ImageFrame<TPixel> frame, int x, int y, TPixel color, double factor)
		{
			int s = O.Spacing;
			x = x * s + s/2; y = y * s + s/2;
			frame[x,y] = color;
		}

		//static TPixel _black = Color.Black.ToPixel<TPixel>();
		//static TPixel _white = Color.White.ToPixel<TPixel>();
		void DrawDotSpere(ImageFrame<TPixel> frame, int x, int y, TPixel color, double factor)
		{
			int s = O.Spacing;
			double d = O.DotSize * Math.Pow(factor,2);
			x = x * s + s/2; y = y * s + s/2;

			if (d <= 1.0) {
				//var ec = frame[x,y];
				//if (IsBigger(color,ec)) { frame[x,y] = color; }
				//var c = ImageHelpers.BetweenColor(color,ec,ratio);
				frame[x,y] = color;
				return;
			}

			var bounds = frame.Bounds();
			int d2 = (int)(d/2);
			Rectangle r = new Rectangle(x - d2, y - d2, (int)d, (int)d);
			for(int dy = r.Top; dy < r.Bottom; dy++) {
				for(int dx = r.Left; dx < r.Right; dx++) {
					if (!bounds.Contains(dx,dy)) { continue; }
					double ratio = MetricHelpers.DistanceEuclidean(dx,dy,x,y) * 2.0 / d;
					if (ratio > 0.9) { continue; }
					//var ec = frame[dx,dy];
					//var c = ImageHelpers.BetweenColor(color,ec,ratio);
					var c = color;
					//if (IsBigger(c,ec)) { frame[dx,dy] = c; }
					frame[dx,dy] = c;
				}
			}
		}

		static ColorSpaceConverter ColorConverter = new ColorSpaceConverter();
		bool IsBigger(TPixel a, TPixel b)
		{
			Rgba32 ra = default(Rgba32);
			a.ToRgba32(ref ra);
			Rgba32 rb = default(Rgba32);
			b.ToRgba32(ref rb);

			var ha = ColorConverter.ToHsl(ra);
			var hb = ColorConverter.ToHsl(rb);
			return ha.L > hb.L;
		}
	}
}
