using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System.Collections.Generic;

namespace ImageFunctions.Derivatives
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			if (rect.Width < 2 || rect.Height < 2) {
				return; //nothing to do
			}

			int qLength = 3 * frame.Width;
			var queue = new Queue<QueueItem>(qLength);
			QueueItem dqi;

			for(int y = rect.Top; y < rect.Bottom; y++) {
				for(int x = rect.Left; x < rect.Right; x++) {
					TPixel? c = null,n = null,e = null,s = null,w = null;
					c = frame.GetPixelRowSpan(y)[x];
					if (x > rect.Left)     { w = frame.GetPixelRowSpan(y)[x-1]; }
					if (x < rect.Right-1)  { e = frame.GetPixelRowSpan(y)[x+1]; }
					if (y > rect.Top)      { n = frame.GetPixelRowSpan(y-1)[x]; }
					if (y < rect.Bottom-1) { s = frame.GetPixelRowSpan(y+1)[x]; }

					var qi = new QueueItem {
						X = x, Y = y, Color = DoDiff(c,n,e,s,w)
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

		static TPixel DoDiff(TPixel? src, TPixel? n, TPixel? e, TPixel? s, TPixel? w)
		{
			if (!src.HasValue) { return default(TPixel); }
			Rgba32 rgbaSrc = ToRgba32(src);
			Rgba32 rgbaN = ToRgba32(n);
			Rgba32 rgbaE = ToRgba32(e);
			Rgba32 rgbaS = ToRgba32(s);
			Rgba32 rgbaW = ToRgba32(w);
			
			double diffR=0.0,diffG=0.0,diffB=0.0;
			int num = 0;

			if (n.HasValue) {
				diffR += Math.Abs(rgbaSrc.R - rgbaN.R);
				diffG += Math.Abs(rgbaSrc.G - rgbaN.G);
				diffB += Math.Abs(rgbaSrc.B - rgbaN.B);
				num++;
			}
			if (e.HasValue) {
				diffR += Math.Abs(rgbaSrc.R - rgbaE.R);
				diffG += Math.Abs(rgbaSrc.G - rgbaE.G);
				diffB += Math.Abs(rgbaSrc.B - rgbaE.B);
				num++;
			}
			if (s.HasValue) {
				diffR += Math.Abs(rgbaSrc.R - rgbaS.R);
				diffG += Math.Abs(rgbaSrc.G - rgbaS.G);
				diffB += Math.Abs(rgbaSrc.B - rgbaS.B);
				num++;
			}
			if (w.HasValue) {
				diffR += Math.Abs(rgbaSrc.R - rgbaW.R);
				diffG += Math.Abs(rgbaSrc.G - rgbaW.G);
				diffB += Math.Abs(rgbaSrc.B - rgbaW.B);
				num++;
			}

			var pix = ToPixel(diffR/num, diffG/num, diffB/num, rgbaSrc.A);
			return pix;
		}

		static Rgba32 ToRgba32(TPixel? px)
		{
			Rgba32 rgba = default(Rgba32);
			if (px.HasValue) {
				px.Value.ToRgba32(ref rgba);
				return rgba;
			}
			return default(Rgba32);
		}

		static TPixel ToPixel(double R,double G,double B,byte A)
		{
			var pix = default(TPixel);
			Rgba32 c = new Rgba32(
				(byte)Math.Clamp(R,0.0,255.0),
				(byte)Math.Clamp(G,0.0,255.0),
				(byte)Math.Clamp(B,0.0,255.0),
				A
			);
			pix.FromRgba32(c);
			return pix;
		}

		struct QueueItem
		{
			public int X;
			public int Y;
			public TPixel Color;
		}

		enum Component
		{
			None = 0,
			R = 1,
			G = 2,
			B = 3,
			A = 4,
			Gray = 5
		}
		
	}
}
