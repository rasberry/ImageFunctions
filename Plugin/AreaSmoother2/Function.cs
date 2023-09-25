using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.AreaSmoother2;

[InternalRegisterFunction(nameof(AreaSmoother2))]
public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb);
	}

	public bool Run(IRegister register, ILayers layers, string[] args)
	{
		if (layers == null) {
			throw Core.Squeal.ArgumentNull(nameof(layers));
		}
		if (!Options.ParseArgs(args, register)) {
			return false;
		}

		if (layers.Count < 1) {
			Tell.LayerMustHaveOne();
			return false;
		}

		var origCanvas = layers.Last();
		using var progress = new ProgressBar();
		using var canvas = layers.NewCanvasFromLayers(); //temporary canvas

		if (!Options.VOnly) {
			MoreTools.ThreadRun(origCanvas.Height - 1, (int y) => {
				HashSet<int> visited = new HashSet<int>();
				for(int x = 0; x < origCanvas.Width; x++) {
					if (visited.Contains(x)) { continue; }
					DrawGradientH(visited,origCanvas,canvas,x,y);
				}
			},progress);
		}

		if (!Options.HOnly) {
			MoreTools.ThreadRun(origCanvas.Width - 1, (int x) => {
				HashSet<int> visited = new HashSet<int>();
				for(int y = 0; y < origCanvas.Height; y++ ) {
					if (visited.Contains(y)) { continue; }
					DrawGradientV(visited,origCanvas,canvas,x,y,!Options.VOnly);
				}
			},progress);
		}

		origCanvas.CopyFrom(canvas);
		return true;
	}

	void DrawGradientH(HashSet<int> visited, ICanvas origCanvas, ICanvas canvas, int x, int y)
	{
		ColorRGBA seed = origCanvas[x,y];

		int lx = x;
		int rx = x;
		while(lx > 0) {
			if (!origCanvas[lx,y].Equals(seed)) { break; }
			lx--;
		}
		while(rx < origCanvas.Width - 2) {
			if (!origCanvas[rx,y].Equals(seed)) { break; }
			rx++;
		}

		int len = rx - lx - 1;
		if (len <= 2) {
			// color span is to small so just use colors as-is
			visited.Add(x);
			canvas[x,y] = seed;
			return;
		}

		var lColor = MoreTools.BetweenColor(origCanvas[lx,y],seed,0.5);
		var rColor = MoreTools.BetweenColor(origCanvas[rx,y],seed,0.5);

		for(int gi=0; gi<len; gi++)
		{
			double ratio = (gi + 1) / (double)len;
			ColorRGBA nc;
			if (ratio > 0.5) {
				nc = MoreTools.BetweenColor(seed,rColor,(ratio - 0.5) * 2.0);
			} else {
				nc = MoreTools.BetweenColor(lColor,seed,ratio * 2.0);
			}
			int gx = lx + gi + 1;
			canvas[gx,y] = nc;
			visited.Add(gx);
		}
	}

	void DrawGradientV(HashSet<int> visited, ICanvas frame, ICanvas canvas, int x, int y, bool blend)
	{
		ColorRGBA seed = frame[x,y];
		int ty = y;
		int by = y;
		while(ty > 0) {
			if (!frame[x,ty].Equals(seed)) { break; }
			ty--;
		}
		while(by < frame.Height - 2) {
			if (!frame[x,by].Equals(seed)) { break; }
			by++;
		}

		int len = by - ty - 1;
		if (len <= 2) {
			// color span is to small so just use colors as-is
			visited.Add(y);
			var fc = blend ? MoreTools.BetweenColor(seed,canvas[x,y],0.5) : seed;
			canvas[x,y] = fc;
			return;
		}

		var tColor = MoreTools.BetweenColor(frame[x,ty],seed,0.5);
		var bColor = MoreTools.BetweenColor(frame[x,by],seed,0.5);

		for(int gi=0; gi<len; gi++)
		{
			double ratio = (gi + 1) / (double)len;
			ColorRGBA nc;
			if (ratio > 0.5) {
				nc = MoreTools.BetweenColor(seed,bColor,(ratio - 0.5) * 2.0);
			} else {
				nc = MoreTools.BetweenColor(tColor,seed,ratio * 2.0);
			}
			int gy = ty + gi + 1;
			var fc = blend ? MoreTools.BetweenColor(nc,canvas[x,gy],0.5) : nc;
			canvas[x,gy] = fc;
			visited.Add(gy);
		}
	}
}


/*
		public override void Apply()
		{
			var frame = Source;
			var rect = Bounds;
			var Iis = Registry.GetImageEngine();

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

		void DrawGradientH(HashSet<int> visited, IImage frame, IImage canvas,
			Rectangle rect, int x, int y)
		{
			IColor seed = frame[x,y];

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
				IColor nc;
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

		void DrawGradientV(HashSet<int> visited, IImage frame, IImage canvas,
			Rectangle rect, int x, int y, bool blend)
		{
			IColor seed = frame[x,y];
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
				IColor nc;
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
*/