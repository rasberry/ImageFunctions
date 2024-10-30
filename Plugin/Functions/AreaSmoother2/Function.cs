using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;
using CoreColors = ImageFunctions.Core.Aides.ColorAide;

namespace ImageFunctions.Plugin.Functions.AreaSmoother2;

[InternalRegisterFunction(nameof(AreaSmoother2))]
public class Function : IFunction
{
	public static IFunction Create(IFunctionContext context)
	{
		if (context == null) {
			throw Squeal.ArgumentNull(nameof(context));
		}

		var f = new Function {
			Context = context,
			O = new(context)
		};
		return f;
	}
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb, Context.Register);
	}

	public IOptions Options { get { return O; } }
	IFunctionContext Context;
	Options O;

	public bool Run(string[] args)
	{
		if(Context.Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!O.ParseArgs(args, Context.Register)) {
			return false;
		}

		if(Context.Layers.Count < 1) {
			Context.Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}

		var engine = Context.Options.Engine.Item.Value;
		var origCanvas = Context.Layers.First().Canvas;
		using var progress = new ProgressBar();
		using var canvas = engine.NewCanvasFromLayers(Context.Layers); //temporary canvas

		if(!O.VOnly) {
			MoreAide.ThreadRun(origCanvas.Height, (int y) => {
				HashSet<int> visited = new HashSet<int>();
				for(int x = 0; x < origCanvas.Width; x++) {
					if(visited.Contains(x)) { continue; }
					DrawGradientH(visited, origCanvas, canvas, x, y);
				}
			}, Context.Options.MaxDegreeOfParallelism, progress);
		}

		if(!O.HOnly) {
			MoreAide.ThreadRun(origCanvas.Width, (int x) => {
				HashSet<int> visited = new HashSet<int>();
				for(int y = 0; y < origCanvas.Height; y++) {
					if(visited.Contains(y)) { continue; }
					DrawGradientV(visited, origCanvas, canvas, x, y, !O.VOnly);
				}
			}, Context.Options.MaxDegreeOfParallelism, progress);
		}

		origCanvas.CopyFrom(canvas);
		return true;
	}

	void DrawGradientH(HashSet<int> visited, ICanvas origCanvas, ICanvas canvas, int x, int y)
	{
		ColorRGBA seed = origCanvas[x, y];

		int lx = x;
		int rx = x;
		while(lx > 0) {
			if(!origCanvas[lx, y].Equals(seed)) { break; }
			lx--;
		}
		while(rx < origCanvas.Width - 2) {
			if(!origCanvas[rx, y].Equals(seed)) { break; }
			rx++;
		}

		int len = rx - lx;
		if(len <= 2) {
			// color span is to small so just use colors as-is
			visited.Add(x);
			canvas[x, y] = seed;
			return;
		}

		var lColor = CoreColors.BetweenColor(origCanvas[lx, y], seed, 0.5);
		var rColor = CoreColors.BetweenColor(origCanvas[rx, y], seed, 0.5);

		for(int gi = 0; gi <= len; gi++) {
			double ratio = (gi + 1) / (double)len;
			ColorRGBA nc;
			if(ratio > 0.5) {
				nc = CoreColors.BetweenColor(seed, rColor, (ratio - 0.5) * 2.0);
			}
			else {
				nc = CoreColors.BetweenColor(lColor, seed, ratio * 2.0);
			}
			int gx = lx + gi;
			canvas[gx, y] = nc;
			visited.Add(gx);
		}
	}

	void DrawGradientV(HashSet<int> visited, ICanvas frame, ICanvas canvas, int x, int y, bool blend)
	{
		ColorRGBA seed = frame[x, y];
		int ty = y;
		int by = y;
		while(ty > 0) {
			if(!frame[x, ty].Equals(seed)) { break; }
			ty--;
		}
		while(by < frame.Height - 2) {
			if(!frame[x, by].Equals(seed)) { break; }
			by++;
		}

		int len = by - ty;
		if(len <= 2) {
			// color span is to small so just use colors as-is
			visited.Add(y);
			var fc = blend ? CoreColors.BetweenColor(seed, canvas[x, y], 0.5) : seed;
			canvas[x, y] = fc;
			return;
		}

		var tColor = CoreColors.BetweenColor(frame[x, ty], seed, 0.5);
		var bColor = CoreColors.BetweenColor(frame[x, by], seed, 0.5);

		for(int gi = 0; gi <= len; gi++) {
			double ratio = (gi + 1) / (double)len;
			ColorRGBA nc;
			if(ratio > 0.5) {
				nc = CoreColors.BetweenColor(seed, bColor, (ratio - 0.5) * 2.0);
			}
			else {
				nc = CoreColors.BetweenColor(tColor, seed, ratio * 2.0);
			}
			int gy = ty + gi;
			var fc = blend ? CoreColors.BetweenColor(nc, canvas[x, gy], 0.5) : nc;
			canvas[x, gy] = fc;
			visited.Add(gy);
		}
	}
}
