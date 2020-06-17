using System;
using System.Drawing;
using System.Collections.Generic;
using ImageFunctions.Helpers;

namespace ImageFunctions.AreaSmoother2
{
	public class Processor : IFAbstractProcessor
	{
		public Options O = null;

		public override void Apply()
		{
			var frame = Source;
			var rect = Bounds;
			var Iis = Engines.Engine.GetConfig();

			using (var progress = new ProgressBar())
			using (var canvas = Iis.NewImage(rect.Width,rect.Height))
			{
				if (!O.VOnly) {
					MoreHelpers.ThreadRows(rect,MaxDegreeOfParallelism,(y) => {
						HashSet<int> visited = new HashSet<int>();
						for(int x = rect.Left; x < rect.Right; x++) {
							if (visited.Contains(x)) { continue; }
							DrawGradientH(visited,frame,canvas,rect,x,y);
						}
					},progress);
				}

				if (!O.HOnly) {
					MoreHelpers.ThreadColumns(rect,MaxDegreeOfParallelism,(x) => {
						HashSet<int> visited = new HashSet<int>();
						for(int y = rect.Top; y < rect.Bottom; y++ ) {
							if (visited.Contains(y)) { continue; }
							DrawGradientV(visited,frame,canvas,rect,x,y,!O.VOnly);
						}
					},progress);
				}

				frame.BlitImage(canvas,rect);
			}
		}

		void DrawGradientH(HashSet<int> visited, IFImage frame, IFImage canvas,
			Rectangle rect, int x, int y)
		{
			IFColor seed = frame[x,y];

			int lx = x;
			int rx = x;
			while(lx > rect.Left) {
				if (!frame[lx,y].Equals(seed)) { break; }
				lx--;
			}
			while(rx < rect.Right - 1) {
				if (!frame[rx,y].Equals(seed)) { break; }
				rx++;
			}

			int len = rx - lx - 1;
			if (len <= 2) {
				// color span is to small so just use colors as-is
				visited.Add(x);
				var (ox,oy) = GetOffset(x,y,rect);
				canvas[ox,oy] = seed;
				return;
			}
		
			var lColor = ImageHelpers.BetweenColor(frame[lx,y],seed,0.5);
			var rColor = ImageHelpers.BetweenColor(frame[rx,y],seed,0.5);

			for(int gi=0; gi<len; gi++)
			{
				double ratio = (double)(gi+1)/(double)len;
				IFColor nc;
				if (ratio > 0.5) {
					nc = ImageHelpers.BetweenColor(seed,rColor,(ratio - 0.5) * 2.0);
				} else {
					nc = ImageHelpers.BetweenColor(lColor,seed,ratio * 2.0);
				}
				int gx = lx + gi + 1;
				var (ox,oy) = GetOffset(gx,y,rect);
				canvas[ox,oy] = nc;
				visited.Add(gx);
			}
		}

		void DrawGradientV(HashSet<int> visited, IFImage frame, IFImage canvas,
			Rectangle rect, int x, int y, bool blend)
		{
			IFColor seed = frame[x,y];
			int ty = y;
			int by = y;
			while(ty > rect.Top) {
				if (!frame[x,ty].Equals(seed)) { break; }
				ty--;
			}
			while(by < rect.Bottom - 1) {
				if (!frame[x,by].Equals(seed)) { break; }
				by++;
			}

			int len = by - ty - 1;
			if (len <= 2) {
				// color span is to small so just use colors as-is
				visited.Add(y);
				var (ox,oy) = GetOffset(x,y,rect);
				var fc = blend ? ImageHelpers.BetweenColor(seed,canvas[ox,oy],0.5) : seed;
				canvas[ox,oy] = fc;
				return;
			}
		
			var tColor = ImageHelpers.BetweenColor(frame[x,ty],seed,0.5);
			var bColor = ImageHelpers.BetweenColor(frame[x,by],seed,0.5);

			for(int gi=0; gi<len; gi++)
			{
				double ratio = (double)(gi+1)/(double)len;
				IFColor nc;
				if (ratio > 0.5) {
					nc = ImageHelpers.BetweenColor(seed,bColor,(ratio - 0.5) * 2.0);
				} else {
					nc = ImageHelpers.BetweenColor(tColor,seed,ratio * 2.0);
				}
				int gy = ty + gi + 1;
				var (ox,oy) = GetOffset(x,gy,rect);
				var fc = blend ? ImageHelpers.BetweenColor(nc,canvas[ox,oy],0.5) : nc;
				canvas[ox,oy] = fc;
				visited.Add(gy);
			}
		}

		static (int,int) GetOffset(int x,int y,Rectangle rect)
		{
			int oy = y - rect.Top;
			int ox = x - rect.Left;
			return (ox,oy);

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
			using (var progress = new ProgressBar())
			using (var canvas = new Image<TPixel>(config,rect.Width,rect.Height))
			{
				if (!O.VOnly) {
					MoreHelpers.ThreadRows(rect,config.MaxDegreeOfParallelism,(y) => {
						HashSet<int> visited = new HashSet<int>();
						for(int x = rect.Left; x < rect.Right; x++) {
							if (visited.Contains(x)) { continue; }
							DrawGradientH(visited,frame,canvas,rect,x,y);
						}
					},progress);
				}

				if (!O.HOnly) {
					MoreHelpers.ThreadColumns(rect,config.MaxDegreeOfParallelism,(x) => {
						HashSet<int> visited = new HashSet<int>();
						for(int y = rect.Top; y < rect.Bottom; y++ ) {
							if (visited.Contains(y)) { continue; }
							DrawGradientV(visited,frame,canvas,rect,x,y,!O.VOnly);
						}
					},progress);
				}

				frame.BlitImage(canvas.Frames.RootFrame,rect);
			}
		}

		void DrawGradientH(HashSet<int> visited, ImageFrame<TPixel> frame, Image<TPixel> canvas,
			Rectangle rect, int x, int y)
		{
			var cSpan = canvas.GetPixelSpan();
			var fSpan = frame.GetPixelSpan();
			TPixel seed = fSpan[y * frame.Width + x];
			int lx = x;
			int rx = x;
			while(lx > rect.Left) {
				if (!fSpan[y * frame.Width + lx].Equals(seed)) { break; }
				lx--;
			}
			while(rx < rect.Right - 1) {
				if (!fSpan[y * frame.Width + rx].Equals(seed)) { break; }
				rx++;
			}

			int len = rx - lx - 1;
			if (len <= 2) {
				// color span is to small so just use colors as-is
				visited.Add(x);
				int off = GetOffset(x,y,rect);
				cSpan[off] = seed;
				return;
			}
		
			var lColor = ImageHelpers.BetweenColor(fSpan[y * frame.Width + lx],seed,0.5);
			var rColor = ImageHelpers.BetweenColor(fSpan[y * frame.Width + rx],seed,0.5);

			for(int gi=0; gi<len; gi++)
			{
				double ratio = (double)(gi+1)/(double)len;
				TPixel nc;
				if (ratio > 0.5) {
					nc = ImageHelpers.BetweenColor(seed,rColor,(ratio - 0.5) * 2.0);
				} else {
					nc = ImageHelpers.BetweenColor(lColor,seed,ratio * 2.0);
				}
				int gx = lx + gi + 1;
				int off = GetOffset(gx,y,rect);
				cSpan[off] = nc;
				visited.Add(gx);
			}
		}

		void DrawGradientV(HashSet<int> visited, ImageFrame<TPixel> frame, Image<TPixel> canvas,
			Rectangle rect, int x, int y, bool blend)
		{
			var cSpan = canvas.GetPixelSpan();
			var fSpan = frame.GetPixelSpan();
			var seed = fSpan[y * frame.Width + x];
			int ty = y;
			int by = y;
			while(ty > rect.Top) {
				if (!fSpan[ty * frame.Width + x].Equals(seed)) { break; }
				ty--;
			}
			while(by < rect.Bottom - 1) {
				if (!fSpan[by * frame.Width + x].Equals(seed)) { break; }
				by++;
			}

			int len = by - ty - 1;
			if (len <= 2) {
				// color span is to small so just use colors as-is
				visited.Add(y);
				int off = GetOffset(x,y,rect);
				var fc = blend ? ImageHelpers.BetweenColor(seed,cSpan[off],0.5) : seed;
				cSpan[off] = fc;
				return;
			}
		
			var tColor = ImageHelpers.BetweenColor(fSpan[ty * frame.Width + x],seed,0.5);
			var bColor = ImageHelpers.BetweenColor(fSpan[by * frame.Width + x],seed,0.5);

			for(int gi=0; gi<len; gi++)
			{
				double ratio = (double)(gi+1)/(double)len;
				TPixel nc;
				if (ratio > 0.5) {
					nc = ImageHelpers.BetweenColor(seed,bColor,(ratio - 0.5) * 2.0);
				} else {
					nc = ImageHelpers.BetweenColor(tColor,seed,ratio * 2.0);
				}
				int gy = ty + gi + 1;
				int off = GetOffset(x,gy,rect);
				var fc = blend ? ImageHelpers.BetweenColor(nc,cSpan[off],0.5) : nc;
				cSpan[off] = fc;
				visited.Add(gy);
			}
		}

		static (int,int) GetOffset(int x,int y,Rectangle rect)
		{
			int oy = y - rect.Top;
			int ox = x - rect.Left;
			return (ox,oy);
		}
	}
	#endif
}
