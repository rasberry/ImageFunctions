
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ImageFunctions.Helpers
{
	public static class MoreHelpers
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

		public static void ThreadPixels(Rectangle rect,int maxThreads,Action<int,int> callback,
			IProgress<double> progress = null)
		{
			long done = 0;
			long max = (long)rect.Width * rect.Height;
			var po = new ParallelOptions {
				MaxDegreeOfParallelism = maxThreads
			};
			Parallel.For(0,max,po,num => {
				int y = (int)(num / (long)rect.Width);
				int x = (int)(num % (long)rect.Width);
				Interlocked.Add(ref done,1);
				progress?.Report((double)done/max);
				callback(x + rect.Left,y + rect.Top);
			});
		}

		public static void ThreadRows(Rectangle rect, int maxThreads, Action<int> callback,
			IProgress<double> progress = null)
		{
			int done = 0;
			int max = rect.Bottom - rect.Top;
			var po = new ParallelOptions {
				MaxDegreeOfParallelism = maxThreads
			};
			Parallel.For(rect.Top,rect.Bottom,po,num => {
				Interlocked.Add(ref done,1);
				progress?.Report((double)done/max);
				callback(num);
			});
		}

		public static void ThreadColumns(Rectangle rect, int maxThreads, Action<int> callback,
			IProgress<double> progress = null)
		{
			int done = 0;
			int max = rect.Right - rect.Left;
			var po = new ParallelOptions {
				MaxDegreeOfParallelism = maxThreads
			};
			Parallel.For(rect.Left,rect.Right,po,num => {
				Interlocked.Add(ref done,1);
				progress?.Report((double)done/max);
				callback(num);
			});
		}

		public static void IteratePixels(Rectangle rect,Action<int,int> callback,
			IProgress<double> progress = null)
		{
			long done = 0;
			long max = (long)rect.Width * rect.Height;

			for(int y = rect.Top; y < rect.Bottom; y++) {
				for (int x = rect.Left; x < rect.Left; x++) {
					done++;
					progress?.Report((double)done/max);
					callback(x,y);
				}
			}
		}

		public static void ParalellSort<T>(IList<T> array, IComparer<T> comp = null, IProgress<double> progress = null, int? MaxDegreeOfParallelism = null)
		{
			var ps = new ParallelSort<T>(array,comp,progress);
			if (MaxDegreeOfParallelism.HasValue && MaxDegreeOfParallelism.Value > 0) {
				ps.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
			}
			ps.Sort();
		}
	}
}
