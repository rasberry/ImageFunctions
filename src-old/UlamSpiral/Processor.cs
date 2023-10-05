using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading;
using ImageFunctions.Helpers;

namespace ImageFunctions.UlamSpiral
{
	public class Processor : AbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			InitColors();
			ImageHelpers.FillWithColor(Source,GetColor(PickColor.Back),Bounds);

			var srect = GetSpacedRectangle(Bounds);
			int maxFactor = O.ColorComposites ? FindMaxFactor(Bounds) : 1;
			double factor = 1.0 / maxFactor;
			var drawFunc = GetDrawFunc();
			var (cx, cy) = GetCenterXY(srect);
			bool drawSlow = O.DotSize > O.Spacing; //turn off multithreading since threads might overlap
			var list = new List<(int,int,int)>(); //used to order dots by size
			bool drawPrimes = O.ColorPrimesForce || (!O.ColorComposites && !O.ColorPrimesBy6m);
			double primeFactor = O.ColorComposites ? 0.0 : 1.0;

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
						DrawComposite(Source, count * factor, x, y, drawFunc);
					}
				}
				//the next modes require the number to be prime
				if (!Primes.IsPrime(num)) { return; }

				if (O.ColorPrimesBy6m) {
					var (ism1,isp1) = Primes.IsPrime6m(num);
					var color = GetColor(PickColor.Back);
					if (ism1) { color = GetColor(PickColor.Prime); }
					if (isp1) { color = GetColor(PickColor.Prime2); }
					drawFunc(Source, x, y, color, primeFactor);
				}
				//only one prime coloring mode is allowed
				else if (drawPrimes) {
					var color = GetColor(PickColor.Prime);
					drawFunc(Source, x, y, color, primeFactor);
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
							var (count, x, y) = item;
							DrawComposite(Source, count * factor, x, y, drawFunc);
							pb2.Report(++pbcount / pbmax);
						}
					}
				}
				else {
					MoreHelpers.ThreadPixels(srect, MaxDegreeOfParallelism, (x, y) => {
						drawOne(x,y);
					}, pb2);
				}
			}
		}

		//a little messy, but didn't want the same code in two places
		void DrawComposite(IImage frame, double amount, int x, int y, Action<IImage, int, int, IColor, double> drawFunc)
		{
			var bg = GetColor(PickColor.Back);
			var fg = GetColor(PickColor.Comp);
			var color = ImageHelpers.BetweenColor(bg, fg, amount);
			drawFunc(frame, x, y, color, amount);
		}

		int FindMaxFactor(Rectangle srect)
		{
			//TODO surely there's a way to estimate the max factorcount so we don't have to actually find it
			int maxFactor = int.MinValue;
			object maxLock = new object();
			var (cx, cy) = GetCenterXY(srect);

			var pb1 = new ProgressBar() { Prefix = "Calculating " };
			using (pb1) {
				MoreHelpers.ThreadPixels(srect, MaxDegreeOfParallelism, (x, y) => {
					long num = MapXY(x, y, cx, cy, srect.Width);
					int count = Primes.CountFactors(num);
					if (count > maxFactor) {
						//the lock ensures we don't accidentally miss a larger value
						lock (maxLock) {
							if (count > maxFactor) { maxFactor = count; }
						}
					}
				}, pb1);
			}
			return maxFactor;
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

		Action<IImage, int, int, IColor, double> GetDrawFunc()
		{
			if (O.DotSize.IsCloseTo(1.0)) {
				return DrawDotPixel;
			}
			else {
				return DrawDotSpere;
			}
		}

		void DrawDotPixel(IImage frame, int x, int y, IColor color, double factor)
		{
			int s = O.Spacing;
			//scale up with spacing and center so we get a nice border
			x = x * s + s/2; y = y * s + s/2;
			frame[x,y] = color;
		}

		void DrawDotSpere(IImage frame, int x, int y, IColor color, double factor)
		{
			int s = O.Spacing;
			//circle size = max*f^2 (squared so it feels like a sphere)
			double d = O.DotSize * factor * factor;
			//scale up with spacing and center so we get a nice border
			x = x * s + s/2; y = y * s + s/2;

			if (d <= 1.0) {
				frame[x,y] = color;
				return;
			}

			var bounds = new Rectangle(0,0,frame.Width,frame.Height);
			int d2 = (int)(d/2);
			Rectangle r = new Rectangle(x - d2, y - d2, (int)d, (int)d);
			for(int dy = r.Top; dy < r.Bottom; dy++) {
				for(int dx = r.Left; dx < r.Right; dx++) {
					if (!bounds.Contains(dx,dy)) { continue; }

					switch(O.WhichDot) {
						case PickDot.Square: {
							frame[dx,dy] = color;
							break;
						}
						case PickDot.Circle: {
							double dist = MetricHelpers.DistanceEuclidean(dx,dy,x,y);
							if (dist <= d2) { frame[dx,dy] = color; }
							break;
						}
						case PickDot.Blob: {
							double ratio = MetricHelpers.DistanceEuclidean(dx,dy,x,y) * 2.0 / d;
							var ec = frame[dx,dy]; //merge with background
							var c = ImageHelpers.BetweenColor(color,ec,ratio);
							frame[dx,dy] = c;
							break;
						}
					}
				}
			}
		}

		static IColor[] c_color = new IColor[4];
		void InitColors()
		{
			var def = O.Color1.Value;
			c_color[0] = def;
			c_color[1] = O.Color2.HasValue ? O.Color2.Value : def;
			c_color[2] = O.Color3.HasValue ? O.Color3.Value : def;
			c_color[3] = O.Color4.HasValue ? O.Color4.Value : def;
		}

		IColor GetColor(PickColor pick)
		{
			return c_color[(int)pick-1];
		}

		public override void Dispose()
		{
		}
	}

}
