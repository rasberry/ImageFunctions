
using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions
{
	public static class Helpers
	{
		public static void Assert(bool isTrue, string message = null)
		{
			if (!isTrue) {
				//throw new System.ApplicationException(message ?? "Assertion Failed");
				Log.Debug(message ?? "Assertion Failed");
			}
		}

		public static string DebugString(this Rectangle r)
		{
			return "X="+r.X+" Y="+r.Y+" W="+r.Width+" H="+r.Height
				+" T="+r.Top+" B="+r.Bottom+" L="+r.Left+" R="+r.Right
			;
		}

		public static void SetMaxDegreeOfParallelism<TPixel>(this Image<TPixel> image,int? max)
			where TPixel : struct, IPixel<TPixel>
		{
			if (max.HasValue) {
				var config = image.GetConfiguration();
				config.MaxDegreeOfParallelism = max.Value;
			}
		}

		public static void ThreadPixels(Rectangle rect,int maxThreads,Action<int,int> callback)
		{
			long max = (long)rect.Width * rect.Height;
			var po = new ParallelOptions {
				MaxDegreeOfParallelism = maxThreads
			};
			Parallel.For(0,max,po,num => {
				int y = (int)(num / (long)rect.Width);
				int x = (int)(num % (long)rect.Width);
				callback(x + rect.Left,y + rect.Top);
			});
		}

		public static void ThreadRows(Rectangle rect, int maxThreads, Action<int> callback)
		{
			var po = new ParallelOptions {
				MaxDegreeOfParallelism = maxThreads
			};
			Parallel.For(rect.Top,rect.Bottom,po,callback);
		}

		public static void ThreadColumns(Rectangle rect, int maxThreads, Action<int> callback)
		{
			var po = new ParallelOptions {
				MaxDegreeOfParallelism = maxThreads
			};
			Parallel.For(rect.Left,rect.Right,po,callback);
		}

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

		public static double DistanceManhattan(double x1, double y1, double x2, double y2)
		{
			return Math.Abs(y2 - y1) + Math.Abs(x2 - x1);
		}

		public static double DistanceEuclidean(double x1, double y1, double x2, double y2)
		{
			double dx = x2 - x1;
			double dy = y2 - y1;
			return Math.Sqrt(dx * dx + dy * dy);
		}

		public static double DistanceChebyshev(double x1, double y1, double x2, double y2, bool invert = false)
		{
			double dx = Math.Abs(x2 - x1);
			double dy = Math.Abs(y2 - y1);

			return invert ? Math.Min(dx,dy) : Math.Max(dx,dy);
		}

		public static double DistanceCanberra(double x1, double y1, double x2, double y2)
		{
			double xden = x2 + x1;
			double yden = y2 + y1;
			//deal with singularities
			double dx = Math.Abs(xden) < double.Epsilon ? 0.0 : (x2-x1)/xden;
			double dy = Math.Abs(yden) < double.Epsilon ? 0.0 : (y2-y1)/yden;
			return dx + dy;
		}

		public static double DistanceMinkowski(double x1, double y1, double x2, double y2, double p)
		{
			if (double.IsPositiveInfinity(p)) {
				return DistanceChebyshev(x1,y1,x2,y2);
			}
			else if (double.IsNegativeInfinity(p)) {
				return DistanceChebyshev(x1,y1,x2,y2,true);
			}
			else if (Math.Abs(p - 1.0) < double.Epsilon) {
				return DistanceManhattan(x1,y1,x2,y2);
			}
			else if (Math.Abs(p - 2.0) < double.Epsilon) {
				return DistanceEuclidean(x1,y1,x2,y2);
			}
			else {
				double dx = Math.Abs(x2 - x1);
				double dy = Math.Abs(y2 - y1);
				return Math.Pow(Math.Pow(dx,p) + Math.Pow(dy,p),1/p);
			}
		}
	}
}
