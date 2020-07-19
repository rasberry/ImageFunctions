using System;
using System.Collections.Generic;
using System.Numerics;
using ImageFunctions.Helpers;

namespace ImageFunctions.Derivatives
{
	public class Processor : AbstractProcessor
	{
		public Options O = null;
		static IColor DefaultColor = Helpers.ColorHelpers.Transparent;

		public override void Apply()
		{
			var frame = Source;
			var rect = Bounds;
			if (rect.Width < 2 || rect.Height < 2) {
				return; //nothing to do
			}

			//using a queue to delay updates instead copying the image
			int qLength = 3 * frame.Width;
			var queue = new Queue<QueueItem>(qLength);
			QueueItem dqi;

			for(int y = rect.Top; y < rect.Bottom; y++) {
				for(int x = rect.Left; x < rect.Right; x++) {
					IColor? n = null,e = null,s = null,w = null;
					IColor c = frame[x,y];

					if (x > rect.Left)     { w = frame[x-1,y]; }
					if (x < rect.Right-1)  { e = frame[x+1,y]; }
					if (y > rect.Top)      { n = frame[x,y-1]; }
					if (y < rect.Bottom-1) { s = frame[x,y+1]; }

					var color = DoDiff(c,n,e,s,w,O.UseABS);
					var qi = new QueueItem {
						X = x, Y = y,
						Color = O.DoGrayscale ? ImageHelpers.ToGrayScale(color) : color
					};

					if (queue.Count >= qLength) {
						dqi = queue.Dequeue();
						frame[dqi.X,dqi.Y] = dqi.Color;
					}
					queue.Enqueue(qi);
				}
			}

			while(queue.TryDequeue(out dqi)) {
				frame[dqi.X,dqi.Y] = dqi.Color;
			}
		}

		static IColor DoDiff(IColor? src, IColor? n, IColor? e, IColor? s, IColor? w, bool abs)
		{
			if (!src.HasValue) { return DefaultColor; }
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
			var pix = new IColor(
				diffR/num + off,
				diffG/num + off,
				diffB/num + off,
				rgbaSrc.A
			);
			return pix;
		}

		static double DiffOne(bool abs,double a, double b)
		{
			double tmp = a - b;
			return abs ? Math.Abs(tmp) : tmp;
		}

		static IColor GetColor(IColor? px)
		{
			if (px.HasValue) {
				return px.Value;
			}
			return new IColor(0,0,0,0);
		}

		struct QueueItem
		{
			public int X;
			public int Y;
			public IColor Color;
		}

		public override void Dispose() {}
	}
}
