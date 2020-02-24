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
			ImageHelpers.FillWithColor(frame,rect,black);

			if (O.UseFactorCount) {
				DrawFactors(frame,rect,config);
			}
			else {
				DrawPrimes(frame,rect,config);
			}
		}

		static void DrawFactors(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			int cx = rect.Width / 2;
			int cy = rect.Height / 2;
			int maxFactor = int.MinValue;
			object maxLock = new object();

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

			double factor = 1.0 / maxFactor;
			var pb2 = new ProgressBar() { Prefix = "Step 2 " };
			using (pb2) {
				MoreHelpers.ThreadPixels(rect,config.MaxDegreeOfParallelism,(x,y) => {
					long num = MathHelpers.XYToSpiralSquare(x,y,cx,cy);
					int count = Primes.CountFactors(num);
					frame[x,y] = IncPixel(frame[x,y], count * factor);
				},pb2);
			}
		}

		static void DrawPrimes(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			int cx = rect.Width / 2 + rect.Left;
			int cy = rect.Height / 2 + rect.Top;
			var white = Color.White.ToPixel<TPixel>();
			var progress = new ProgressBar();
			using (progress) {
				MoreHelpers.ThreadPixels(rect,config.MaxDegreeOfParallelism,(x,y) => {
					long num = MathHelpers.XYToSpiralSquare(x,y,cx,cy);
					if (Primes.IsPrime(num)) {
						frame[x,y] = white;
					}
				},progress);
			}
		}

		static TPixel IncPixel(TPixel pixel, double factor)
		{
			var rgba = ImageHelpers.ToColor(pixel);
			rgba.R += factor;
			rgba.G += factor;
			rgba.B += factor;
			return ImageHelpers.FromColor<TPixel>(rgba);
		}
	}
}
