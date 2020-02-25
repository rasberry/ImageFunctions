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
			int cx = (rect.Width / 2) - O.CenterX.GetValueOrDefault(0);
			int cy = (rect.Height / 2) - O.CenterY.GetValueOrDefault(0);
			return (cx,cy);
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
					long num = MathHelpers.XYToSpiralSquare(x,y,cx,cy);
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
					long num = MathHelpers.XYToSpiralSquare(x,y,cx,cy);
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
					long num = MathHelpers.XYToSpiralSquare(x,y,cx,cy);
					if (Primes.IsPrime(num)) {
						frame[x,y] = color;
					}
				},progress);
			}
		}
	}
}
