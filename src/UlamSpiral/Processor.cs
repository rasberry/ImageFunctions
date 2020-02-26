using System;
using System.Numerics;
using System.Threading;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
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

		(int,int) GetCenterXY(ImageFrame<TPixel> frame, Rectangle rect)
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
			switch(O.Mapping)
			{
			case PickMapping.Linear:
				return MathHelpers.XYToLinear(x,y,w,cx,cy);
			case PickMapping.Diagonal:
				return MathHelpers.XYToDiagonal(x,y,cx,cy);
			case PickMapping.Spiral:
				return MathHelpers.XYToSpiralSquare(x,y,cx,cy);
			}
			return -1;
		}

		void DrawFactors(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			var (cx,cy) = GetCenterXY(frame,rect);
			int maxFactor = int.MinValue;
			object maxLock = new object();

			//TODO surely there's a way to estimate the max factorcount so we don't have to actually find it
			var pb1 = new ProgressBar() { Prefix = "Step 1 " };
			using (pb1) {
				MoreHelpers.ThreadPixels(rect,config.MaxDegreeOfParallelism,(x,y) => {
					long num = MapXY(x,y,cx,cy,rect.Width);
					int count = Primes.CountFactors(num);
					if (count > maxFactor) {
						//the lock ensures we don't accidentally miss a larger value
						lock(maxLock) {
							if (count > maxFactor) { maxFactor = count; }
						}
					}
				},pb1);
			}
			//Log.Debug($"maxFactor={maxFactor}");

			var fcolor = O.ColorComposite.ToPixel<TPixel>();
			var bcolor = O.ColorBack.ToPixel<TPixel>();
			double factor = 1.0 / maxFactor;
			var pb2 = new ProgressBar() { Prefix = "Step 2 " };
			using (pb2) {
				MoreHelpers.ThreadPixels(rect,config.MaxDegreeOfParallelism,(x,y) => {
					long num = MapXY(x,y,cx,cy,rect.Width);
					int count = Primes.CountFactors(num);
					var color = ImageHelpers.BetweenColor(bcolor,fcolor,count * factor);
					frame[x,y] = color;
				},pb2);
			}
		}

		void DrawPrimes(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			var color = O.ColorPrime.ToPixel<TPixel>();
			var (cx,cy) = GetCenterXY(frame,rect);
			var progress = new ProgressBar();
			using (progress) {
				MoreHelpers.ThreadPixels(rect,config.MaxDegreeOfParallelism,(x,y) => {
					long num = MapXY(x,y,cx,cy,rect.Width);
					if (Primes.IsPrime(num)) {
						frame[x,y] = color;
					}
				},progress);
			}
		}
	}
}
