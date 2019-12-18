using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ImageFunctions.Derivatives
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Options O = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			if (rect.Width < 2 || rect.Height < 2) {
				return; //nothing to do
			}

			//using a queue to delay updates instead copying the image
			int qLength = 3 * frame.Width;
			var queue = new Queue<QueueItem>(qLength);
			QueueItem dqi;

			for(int y = rect.Top; y < rect.Bottom; y++) {
				for(int x = rect.Left; x < rect.Right; x++) {
					TPixel? n = null,e = null,s = null,w = null;
					TPixel c = frame.GetPixelRowSpan(y)[x];

					if (x > rect.Left)     { w = frame.GetPixelRowSpan(y)[x-1]; }
					if (x < rect.Right-1)  { e = frame.GetPixelRowSpan(y)[x+1]; }
					if (y > rect.Top)      { n = frame.GetPixelRowSpan(y-1)[x]; }
					if (y < rect.Bottom-1) { s = frame.GetPixelRowSpan(y+1)[x]; }

					var color = DoDiff(c,n,e,s,w,O.UseABS);
					var qi = new QueueItem {
						X = x, Y = y,
						Color = O.DoGrayscale ? ImageHelpers.ToGrayScale(color) : color
					};

					if (queue.Count >= qLength) {
						dqi = queue.Dequeue();
						frame.GetPixelRowSpan(dqi.Y)[dqi.X] = dqi.Color;
					}
					queue.Enqueue(qi);
				}
			}

			while(queue.TryDequeue(out dqi)) {
				frame.GetPixelRowSpan(dqi.Y)[dqi.X] = dqi.Color;
			}
		}

		static TPixel DoDiff(TPixel? src, TPixel? n, TPixel? e, TPixel? s, TPixel? w, bool abs)
		{
			if (!src.HasValue) { return default(TPixel); }
			var rgbaSrc = GetColor(src);
			var rgbaN = GetColor(n);
			var rgbaE = GetColor(e);
			var rgbaS = GetColor(s);
			var rgbaW = GetColor(w);

			double diffR = 0,diffG = 0,diffB = 0;
			int num = 0;

			if (n.HasValue) {
				diffR += DiffOne(abs,rgbaSrc.R,rgbaN.R);
				diffG += DiffOne(abs,rgbaSrc.G,rgbaN.G);
				diffB += DiffOne(abs,rgbaSrc.B,rgbaN.B);
				num++;
			}
			if (e.HasValue) {
				diffR += DiffOne(abs,rgbaSrc.R,rgbaE.R);
				diffG += DiffOne(abs,rgbaSrc.G,rgbaE.G);
				diffB += DiffOne(abs,rgbaSrc.B,rgbaE.B);
				num++;
			}
			if (s.HasValue) {
				diffR += DiffOne(abs,rgbaSrc.R,rgbaS.R);
				diffG += DiffOne(abs,rgbaSrc.G,rgbaS.G);
				diffB += DiffOne(abs,rgbaSrc.B,rgbaS.B);
				num++;
			}
			if (w.HasValue) {
				diffR += DiffOne(abs,rgbaSrc.R,rgbaW.R);
				diffG += DiffOne(abs,rgbaSrc.G,rgbaW.G);
				diffB += DiffOne(abs,rgbaSrc.B,rgbaW.B);
				num++;
			}
			double off = abs ? 0 : 0.5;
			if (abs) { num *= 2; }
			var pix = new RgbaD(
				diffR/num + off,
				diffG/num + off,
				diffB/num + off,
				rgbaSrc.A
			);
			return pix.FromColor<TPixel>();
		}

		static double DiffOne(bool abs,double a, double b)
		{
			double tmp = a - b;
			return abs ? Math.Abs(tmp) : tmp;
		}

		static RgbaD GetColor(TPixel? px)
		{
			if (px.HasValue) {
				return px.Value.ToColor();
			}
			return default(RgbaD);
		}

		struct QueueItem
		{
			public int X;
			public int Y;
			public TPixel Color;
		}
	}
}
