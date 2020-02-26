using System;

namespace ImageFunctions.Helpers
{
	public static class MathHelpers
	{
		public static int IntCeil(int num, int den)
		{
			int floor = num / den;
			int extra = num % den == 0 ? 0 : 1;
			return floor + extra;
		}

		public static double Fractional(this double number)
		{
			//return number - Math.Truncate(number); //TODO returns negative numbers - don't know why
			return Math.Abs(number % 1.0);
		}
		public static double Integral(this double number)
		{
			return Math.Truncate(number);
		}

		//start: top left
		//run  : right to left
		//fill : top to bottom
		public static (int,int) LinearToXY(long position, int width, int cx = 0, int cy = 0)
		{
			int y = (int)(position / width);
			int x = (int)(position % width);
			return (x + cx,y + cy);
		}

		public static long XYToLinear(int x, int y, int width, int cx = 0, int cy = 0)
		{
			x -= cx; y -= cy;
			return (long)y * width + x;
		}

		//start: top left
		//run  : diagonal right + up
		//fill : top left to bottom right
		public static (int,int) DiagonalToXY(long position, int cx = 0, int cy = 0)
		{
			if (position < 0) {
				throw new ArgumentOutOfRangeException("position must be positive");
			}
			//X 0,0,1,0,1,2,0,1,2,3,0,1,2,3,4,0,1,2,3,4,5
			//Y 0,1,0,2,1,0,3,2,1,0,4,3,2,1,0,5,4,3,2,1,0
			//X https://oeis.org/A002262
			//Y https://oeis.org/A025581

			long t = (long)Math.Floor((1.0 + Math.Sqrt(1.0 + 8.0 * position))/2.0);
			int x = (int)(position - (t * (t - 1)) / 2);
			int y = (int)((t * (t + 1) - 2 * position - 2)/2);
			return (x + cx,y + cy);
		}

		public static long XYToDiagonal(int x, int y, int cx = 0, int cy = 0)
		{
			//solve([x = p - (t * (t - 1)) / 2, y = ((t - 1)*(t / 2 + 1)) - p],[p,t]);
			//p1=(y^2+x*(2*y+3)+y+x^2)/2

			x -= cx; y -= cy;
			long pos = (y*y + x*(2*y + 3) + y + x*x)/2;
			return pos;
		}

		public static (int,int) SpiralSquareToXY(long position, int cx = 0, int cy = 0)
		{
			// https://math.stackexchange.com/questions/163080/on-a-two-dimensional-grid-is-there-a-formula-i-can-use-to-spiral-coordinates-in

			position++; //1 based spiral
			long k = (long)Math.Ceiling((Math.Sqrt(position)-1.0)/2.0);
			long t = 2 * k + 1;
			long m = t * t;
			t-=1;
			/// Console.WriteLine($"p={position} k={k} t={t} m={m} (m-t)={m-t}");
			if (position >= m-t) {
				var x = k-(m-position);
				var y = k;
				return ((int)x+cx,(int)y+cy);
			}
			m -= t;
			if (position >= m-t) {
				var x = -k;
				var y = k-(m-position);
				return ((int)x+cx,(int)y+cy);
			}
			m -= t;
			if (position >= m-t) {
				var x = -k+(m-position);
				var y = -k;
				return ((int)x+cx,(int)y+cy);
			}
			else {
				var x = k;
				var y = -k+(m-position-t);
				return ((int)x+cx,(int)y+cy);
			}
		}

		public static long XYToSpiralSquare(int x,int y, int cx = 0, int cy = 0)
		{
			// https://www.reddit.com/r/dailyprogrammer/comments/3ggli3/20150810_challenge_227_easy_square_spirals/
			x -= cx;
			y -= cy;
			
			y = -y; //original is CW i need CCW
			if (x >= y) {
				if (x > -y) {
					long m = 2*x-1; m *= m;
					return m + Math.Abs(x) + y - 1;
				}
				else {
					long m = 2*y+1; m *= m;
					return m + Math.Abs(7*y) + x - 1;
				}
			}
			else {
				if (x > -y) {
					long m = 2*y-1; m *= m;
					return m + Math.Abs(3*y) - x - 1;
				}
				else {
					long m = 2*x+1; m *= m;
					return m + Math.Abs(5*x) - y - 1;
				}
			}
		}
	}
}
