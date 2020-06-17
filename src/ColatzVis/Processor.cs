using System;
using System.Drawing;
using System.Numerics;
using ImageFunctions.Helpers;

namespace ImageFunctions.ColatzVis
{
	public class Processor : IFAbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			var frame = Source;
			var rect = Bounds;
			var black = Helpers.Colors.Black;
			ImageHelpers.FillWithColor(frame,rect,black);

			for(BigInteger c = 3; c < 100000; c += 2)
			{
				var num = c;
				//var num = MapXtoOdd(x);
				while(num > 1) {
					var next = num * 3 + 1; //core of colatz
					//get x,y and nextOdd
					var (nx,ny,no) = MapNumToXY(next,rect);

					if (rect.Contains(nx,ny)) {
						frame[nx,ny] = IncPixel(frame[nx,ny]);
					}
					num = no;
				}
			}
		}

		static BigInteger MapXtoOdd(int x)
		{
			return x*2+1;
		}

		static int MaxOddtoX(BigInteger odd)
		{
			var x = (odd - 1) / 2;
			if (x < 0) { return 0; }
			if (x > int.MaxValue) { return int.MaxValue; }
			return (int)x;
		}

		static (int,int,BigInteger) MapNumToXY(BigInteger num,Rectangle bounds)
		{
			int count = 0;
			while(num % 2 == 0) {
				num /= 2;
				count++;
			}
			int x = MaxOddtoX(num);

			int cx = bounds.Width / 2;
			int cy = bounds.Width / 2;

			//TODO turn x into a spiral (
			// //https://rechneronline.de/pi/spiral.php
			// //https://math.stackexchange.com/questions/877044/keeping-the-arc-length-constant-between-points-in-a-spiral
			// //https://gamedev.stackexchange.com/questions/16745/moving-a-particle-around-an-archimedean-spiral-at-a-constant-speed
			// //https://www.intmath.com/blog/mathematics/length-of-an-archimedean-spiral-6595
			//double r = Math.Min(cx,cy);
			//double dr = 4.0;
			//double a = dr / 2*Math.PI;

			return ((int)Math.Sqrt(x),count,num);
		}

		// l = a / 2 * [ φ * √ (1 + φ²) + ln( φ + √ (1 + φ²) ) ]
		//solve(l=a/2*(p*sqrt(a+p*p) + log(1+p*p)),p);

		//p = sqrt(-a^2 * b^2 + a^2 * log^2(c^2 + 1) - 4 * a * l * log(c^2 + 1) + 4 * l^2) / (a * b)
		//p*a*b = sqrt(-a^2 * b^2 + a^2 * log^2(c^2 + 1) - 4 * a * l * log(c^2 + 1) + 4 * l^2)
		//p*a*p = sqrt(-a^2 * p^2 + a^2 * log(p^2 + 1)^2 - 4 * a * l * log(p^2 + 1) + 4 * l^2)

		//l = a/2*(sqrt(t^2+1)*t+asinh(t)))
		//asinh(t) = 2*l/a-t*sqrt(t^2+1)
		//asinh(x) = x - (1/2)*(x^3/3)+(1*3/2*4)*(x^5/5) - (1*3*5/2*4*6)*(x^7/7)
		//solve(2*l/a-x*sqrt(x^2+1) = x - (1/2)*(x^3/3)+(1*3/2*4)*(x^5/5) - (1*3*5/2*4*6)*(x^7/7),l);


		static IFColor IncPixel(IFColor pixel)
		{
			pixel.R += 0.001;
			pixel.G += 0.001;
			pixel.B += 0.001;
			return pixel;
		}

		public override void Dispose() {}
	}

	#if false
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Options O = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			var span = frame.GetPixelSpan();
			var black = Color.Black.ToPixel<TPixel>();
			ImageHelpers.FillWithColor(frame,rect,black);

			for(BigInteger c = 3; c < 100000; c += 2)
			{
				var num = c;
				//var num = MapXtoOdd(x);
				while(num > 1) {
					var next = num * 3 + 1; //core of colatz
					//get x,y and nextOdd
					var (nx,ny,no) = MapNumToXY(next,rect);

					if (rect.Contains(nx,ny)) {
						int o = ny*frame.Width + nx;
						span[o] = IncPixel(span[o]);
					}
					num = no;
				}
			}
		}

		static BigInteger MapXtoOdd(int x)
		{
			return x*2+1;
		}

		static int MaxOddtoX(BigInteger odd)
		{
			var x = (odd - 1) / 2;
			if (x < 0) { return 0; }
			if (x > int.MaxValue) { return int.MaxValue; }
			return (int)x;
		}

		static (int,int,BigInteger) MapNumToXY(BigInteger num,Rectangle bounds)
		{
			int count = 0;
			while(num % 2 == 0) {
				num /= 2;
				count++;
			}
			int x = MaxOddtoX(num);

			int cx = bounds.Width / 2;
			int cy = bounds.Width / 2;

			//TODO turn x into a spiral (
			// //https://rechneronline.de/pi/spiral.php
			// //https://math.stackexchange.com/questions/877044/keeping-the-arc-length-constant-between-points-in-a-spiral
			// //https://gamedev.stackexchange.com/questions/16745/moving-a-particle-around-an-archimedean-spiral-at-a-constant-speed
			// //https://www.intmath.com/blog/mathematics/length-of-an-archimedean-spiral-6595
			//double r = Math.Min(cx,cy);
			//double dr = 4.0;
			//double a = dr / 2*Math.PI;

			return ((int)Math.Sqrt(x),count,num);
		}

		// l = a / 2 * [ φ * √ (1 + φ²) + ln( φ + √ (1 + φ²) ) ]
		//solve(l=a/2*(p*sqrt(a+p*p) + log(1+p*p)),p);

		//p = sqrt(-a^2 * b^2 + a^2 * log^2(c^2 + 1) - 4 * a * l * log(c^2 + 1) + 4 * l^2) / (a * b)
		//p*a*b = sqrt(-a^2 * b^2 + a^2 * log^2(c^2 + 1) - 4 * a * l * log(c^2 + 1) + 4 * l^2)
		//p*a*p = sqrt(-a^2 * p^2 + a^2 * log(p^2 + 1)^2 - 4 * a * l * log(p^2 + 1) + 4 * l^2)

		//l = a/2*(sqrt(t^2+1)*t+asinh(t)))
		//asinh(t) = 2*l/a-t*sqrt(t^2+1)
		//asinh(x) = x - (1/2)*(x^3/3)+(1*3/2*4)*(x^5/5) - (1*3*5/2*4*6)*(x^7/7)
		//solve(2*l/a-x*sqrt(x^2+1) = x - (1/2)*(x^3/3)+(1*3/2*4)*(x^5/5) - (1*3*5/2*4*6)*(x^7/7),l);


		static TPixel IncPixel(TPixel pixel)
		{
			var rgba = ImageHelpers.ToColor(pixel);
			rgba.R += 0.001;
			rgba.G += 0.001;
			rgba.B += 0.001;
			return ImageHelpers.FromColor<TPixel>(rgba);
		}
	}
	#endif
}
