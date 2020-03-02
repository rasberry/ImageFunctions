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
			InitColors();
			ImageHelpers.FillWithColor(frame,rect,GetColor(PickColor.Back));

			var srect = GetSpacedRectangle(rect);
			int maxFactor = O.ColorComposites ? FindMaxFactor(rect,config) : 1;
			double factor = 1.0 / maxFactor;
			var drawFunc = GetDrawFunc();
			var (cx, cy) = GetCenterXY(srect);
			bool drawSlow = O.DotSize > O.Spacing;
			var list = new List<(int,int,int)>();

			//using a closure is not my favorite way of dong this,
			// but easier than passing tons of argments to a function
			Action<int,int> drawOne = (int x, int y) => {
				long num = MapXY(x, y, cx, cy, srect.Width);
				if (O.ColorComposites) {
					int count = Primes.CountFactors(num);
					if (drawSlow && count > O.Spacing) {
						list.Add((count,x,y));
					}
					else {
						var bg = GetColor(PickColor.Back);
						var fg = GetColor(PickColor.Comp);
						var color = ImageHelpers.BetweenColor(bg, fg, count * factor);
						drawFunc(frame, x, y, color, count * factor);
					}
				}
				if (O.ColorPrimesBy6m) {
					if (!Primes.IsPrime(num)) { return; }
					var (ism1,isp1) = Primes.IsPrime6m(num);
					var color = GetColor(PickColor.Back);
					if (ism1) { color = GetColor(PickColor.Prime); }
					if (isp1) { color = GetColor(PickColor.Prime2); }
					drawFunc(frame, x, y, color, 1.0);
				}
				if (O.ColorPrimesForce || (!O.ColorComposites && !O.ColorPrimesBy6m)) {
					if (!Primes.IsPrime(num)) { return; }
					var color = GetColor(PickColor.Prime);
					drawFunc(frame, x, y, color, 1.0);
				}
			};

			var pb2 = new ProgressBar() { Prefix = "Drawing " };
			using(pb2) {
				if (drawSlow) {
					//have to keep track o progress manually
					double pbmax = srect.Width * srect.Height;
					double pbcount = 0;
					for(int y = srect.Top; y < srect.Bottom; y++) {
						for(int x = srect.Left; x < srect.Right; x++) {
							drawOne(x,y);
							pb2.Report(++pbcount/pbmax);
						}
					}

					//for slow composites need to draw lefovers
					if (O.ColorComposites) {
						//Log.Debug($"leftover count: {list.Count}");
						list.Sort((a,b) => a.Item1 - b.Item1);

						foreach(var item in list) {
							var (count,x,y) = item;
							var bg = GetColor(PickColor.Back);
							var fg = GetColor(PickColor.Comp);
							var color = ImageHelpers.BetweenColor(bg, fg, count * factor);
							drawFunc(frame, x, y, color, count * factor);
							pb2.Report(++pbcount/pbmax);
						}
					}
				}
				else {
					MoreHelpers.ThreadPixels(srect, config.MaxDegreeOfParallelism, (x, y) => {
						drawOne(x,y);
					}, pb2);
				}
			}
		}

		#if false
		void DrawFast(ImageFrame<TPixel> frame, Configuration config, Rectangle srect, double factor, ProgressBar pb2)
		{
			var bcolor = GetColor(1);
			var fcolor = GetColor(2);
			var drawFunc = GetDrawFunc();
			var (cx, cy) = GetCenterXY(srect);
			bool drawSlow = O.DotSize > O.Spacing;

			MoreHelpers.ThreadPixels(srect, config.MaxDegreeOfParallelism, (x, y) => {
				long num = MapXY(x, y, cx, cy, srect.Width);
				if (O.ColorComposites) {
					int count = Primes.CountFactors(num);
					var color = ImageHelpers.BetweenColor(GetColor(1), GetColor(3), count * factor);
					drawFunc(frame, x, y, color, count * factor);
				}
				if (O.ColorPrimesBy6m) {
					var (ism1,isp1) = Primes.IsPrime6m(num);
					var color = GetColor(1);
					if (ism1) { color = GetColor(2); }
					if (isp1) { color = GetColor(4); }
					drawFunc(frame,x,y,color,1.0);
				}
				if (O.ColorPrimesForce || (!O.ColorComposites && !O.ColorPrimesBy6m)) {
					var color = GetColor(2);
					drawFunc(frame,x,y,color,1.0);
				}
			}, pb2);
		}
		#endif

		int FindMaxFactor(Rectangle srect, Configuration config)
		{
			//TODO surely there's a way to estimate the max factorcount so we don't have to actually find it
			int maxFactor = int.MinValue;
			object maxLock = new object();
			var (cx, cy) = GetCenterXY(srect);
			
			var pb1 = new ProgressBar() { Prefix = "Calculating " };
			using (pb1)
			{
				MoreHelpers.ThreadPixels(srect, config.MaxDegreeOfParallelism, (x, y) =>
				{
					long num = MapXY(x, y, cx, cy, srect.Width);
					int count = Primes.CountFactors(num);
					if (count > maxFactor)
					{
						//the lock ensures we don't accidentally miss a larger value
						lock (maxLock) {
							if (count > maxFactor) { maxFactor = count; }
						}
					}
				}, pb1);
			}
			return maxFactor;
		}

		#if false
		void DrawFactors(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			var srect = GetSpacedRectangle(rect);
			var (cx, cy) = GetCenterXY(srect);
			int maxFactor = FindMaxFactor(rect,config);
			//Log.Debug($"maxFactor={maxFactor}");

			double factor = 1.0 / maxFactor;
			var drawFunc = GetDrawFunc();
			//int maxThreads = O.DotSize > O.Spacing ? 1 : config.MaxDegreeOfParallelism;

			var pb2 = new ProgressBar() { Prefix = "Step 2 " };
			using(pb2) {
				if (O.DotSize > O.Spacing) {
					DrawFactorsSlow(frame, config, srect, cx, cy, factor, drawFunc, pb2);
				}
				else {
					DrawFactorsFast(frame, config, srect, cx, cy, factor, drawFunc, pb2);
				}
			}
		}

		void DrawFactorsFast(ImageFrame<TPixel> frame, Configuration config, Rectangle srect, int cx, int cy, double factor, Action<ImageFrame<TPixel>, int, int, TPixel, double> drawFunc, ProgressBar pb2)
		{
			var bcolor = GetColor(1);
			var fcolor = GetColor(2);
			MoreHelpers.ThreadPixels(srect, config.MaxDegreeOfParallelism, (x, y) =>
			{
				long num = MapXY(x, y, cx, cy, srect.Width);
				int count = Primes.CountFactors(num);
				var color = ImageHelpers.BetweenColor(bcolor, fcolor, count * factor);
				drawFunc(frame, x, y, color, count * factor);
			}, pb2);
		}

		void DrawFactorsSlow(ImageFrame<TPixel> frame, Configuration config, Rectangle srect, int cx, int cy, double factor, Action<ImageFrame<TPixel>, int, int, TPixel, double> drawFunc, ProgressBar pb2)
		{
			int s = O.Spacing;
			var list = new List<(int,int,int)>();
			double pbmax = srect.Width * srect.Height;
			var bcolor = GetColor(1);
			var fcolor = GetColor(2);

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
			var color = GetColor(2);
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
		#endif

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
					//TODO add option to switch between sphere and disk
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

		static TPixel[] c_color = new TPixel[4];
		void InitColors()
		{
			var def = O.Color1.Value.ToPixel<TPixel>();
			c_color[0] = def;
			c_color[1] = O.Color2.HasValue ? O.Color2.Value.ToPixel<TPixel>() : def;
			c_color[2] = O.Color3.HasValue ? O.Color3.Value.ToPixel<TPixel>() : def;
			c_color[3] = O.Color4.HasValue ? O.Color4.Value.ToPixel<TPixel>() : def;
		}

		TPixel GetColor(PickColor pick)
		{
			return c_color[(int)pick-1];
		}
	}
}
