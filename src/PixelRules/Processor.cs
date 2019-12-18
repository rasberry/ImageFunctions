using System;
using System.Collections.Generic;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions.PixelRules
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		public Options O = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			using (var progress = new ProgressBar())
			using (var canvas = new Image<TPixel>(config,rect.Width,rect.Height))
			{
				for(int p=0; p<O.Passes; p++) {
					progress.Prefix = "Pass "+(p+1)+"/"+O.Passes+" ";
					MoreHelpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
						int cy = y - rect.Top;
						int cx = x - rect.Left;
						TPixel nc = RunRule(frame,rect,x,y);
						int coff = cy * rect.Width + cx;
						canvas.GetPixelSpan()[coff] = nc;
					},progress);
					frame.BlitImage(canvas.Frames.RootFrame,rect);
				}
			}
		}

		TPixel RunRule(ImageFrame<TPixel> frame,Rectangle rect, int x, int y)
		{
			int cx = x, cy = y;
			var history = new List<TPixel>();
			int max = O.MaxIters;

			while(--max >= 0) {
				TPixel ant = frame[cx,cy];
				history.Add(ant);
				if (!IsBetterPixel(frame,rect,ant,cx,cy,out int nx, out int ny)) {
					break;
				}
				cx += nx;
				cy += ny;
			}

			if (history.Count < 2) {
				return history[0];
			} else {
				return FindAverageColor(history);
			}
		}

		TPixel FindAverageColor(IEnumerable<TPixel> list)
		{
			double r = 0.0,g = 0.0,b = 0.0,a = 0.0;
			int count = 0;

			foreach(TPixel px in list) {
				var c = px.ToColor();
				r += c.R;
				g += c.G;
				b += c.B;
				a += c.A;
				count++;
			}

			var avg = new RgbaD(
				r / count,
				g / count,
				b / count,
				a / count
			);
			return avg.FromColor<TPixel>();
		}

		bool IsBetterPixel(ImageFrame<TPixel> frame, Rectangle rect, TPixel? best, int x, int y, out int bx, out int by)
		{
			bx = by = 0;
			TPixel? nn,ne,ee,se,ss,sw,ww,nw;
			nn = ne = ee = se = ss = sw = ww = nw = null;

			//which directions are available ?
			bool bn = y - 1 >= rect.Top;
			bool be = x + 1 < rect.Right;
			bool bs = y + 1 < rect.Bottom;
			bool bw = x - 1 >= rect.Left;

			// grab source pixels
			if (bn)       { nn = frame[x + 0,y - 1]; }
			if (bn && be) { ne = frame[x + 1,y - 1]; }
			if (be)       { ee = frame[x + 1,y + 0]; }
			if (be && bs) { se = frame[x + 1,y + 1]; }
			if (bs)       { ss = frame[x + 0,y + 1]; }
			if (bs && bw) { sw = frame[x - 1,y + 1]; }
			if (bw)       { ww = frame[x - 1,y + 0]; }
			if (bw && bn) { nw = frame[x - 1,y - 1]; }

			//find closest non-identical pixel
			double min = double.MaxValue;
			if (PickBetter(ref best,nn,ref min)) { bx =  0; by = -1; }
			if (PickBetter(ref best,ne,ref min)) { bx =  1; by = -1; }
			if (PickBetter(ref best,ee,ref min)) { bx =  1; by =  0; }
			if (PickBetter(ref best,se,ref min)) { bx =  1; by =  1; }
			if (PickBetter(ref best,ss,ref min)) { bx =  0; by =  1; }
			if (PickBetter(ref best,sw,ref min)) { bx = -1; by =  1; }
			if (PickBetter(ref best,ww,ref min)) { bx = -1; by =  0; }
			if (PickBetter(ref best,nw,ref min)) { bx = -1; by = -1; }

			return bx != 0 && by != 0;
		}

		//pick closest darker color
		bool PickBetter(ref TPixel? best, TPixel? bid, ref double min)
		{
			//if best is null anything is better
			if (best == null) {
				best = bid;
				min = double.MaxValue;
				return true;
			}
			//both are good
			if (bid != null) {
				if (O.WhichMode == Function.Mode.StairCaseAscend
					|| O.WhichMode == Function.Mode.StairCaseDescend)
				{
					//only follow darker colors
					TPixel white = Color.White.ToPixel<TPixel>();
					double bdw = Dist(best.Value,white);
					double ddw = Dist(bid.Value,white);
					if (bdw > ddw) { return false; }
				}

				//follow if color is closer
				double d = Dist(best.Value,bid.Value);
				if (d < min) {
					best = bid;
					min = d;
					return true;
				}
			}
			//else skip empty bids
			return false;
		}

		double Dist(TPixel one, TPixel two)
		{
			//treat identical pixels as very far apart
			if (one.Equals(two)) {
				return double.MaxValue;
			}

			var o = one.ToColor();
			var t = two.ToColor();

			bool normal = O.WhichMode == Function.Mode.StairCaseDescend
				|| O.WhichMode == Function.Mode.StairCaseClosest;
			double[] vo = normal
				? new double[] { o.R, o.G, o.B, o.A }
				: new double[] { 1.0 - o.R, 1.0 - o.G, 1.0 - o.B, 1.0 - o.A }
			;
			double[] vt = new double[] { t.R, t.G, t.B, t.A };

			double dist = O.Measurer.Measure(vo,vt);
			return dist;
		}
	}
}
