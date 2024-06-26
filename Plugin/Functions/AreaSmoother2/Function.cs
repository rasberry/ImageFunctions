using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.AreaSmoother2;

[InternalRegisterFunction(nameof(AreaSmoother2))]
public class Function : IFunction
{
	public static IFunction Create(IRegister register, ILayers layers, ICoreOptions core)
	{
		var f = new Function {
			Register = register,
			Core = core,
			Layers = layers
		};
		return f;
	}

	public void Usage(StringBuilder sb)
	{
		O.Usage(sb, Register);
	}

	public bool Run(string[] args)
	{
		if (Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if (!O.ParseArgs(args, Register)) {
			return false;
		}

		if (Layers.Count < 1) {
			Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}

		var engine = Core.Engine.Item.Value;
		var origCanvas = Layers.First().Canvas;
		using var progress = new ProgressBar();
		using var canvas = engine.NewCanvasFromLayers(Layers); //temporary canvas

		if (!O.VOnly) {
			PlugTools.ThreadRun(origCanvas.Height, (int y) => {
				HashSet<int> visited = new HashSet<int>();
				for(int x = 0; x < origCanvas.Width; x++) {
					if (visited.Contains(x)) { continue; }
					DrawGradientH(visited,origCanvas,canvas,x,y);
				}
			}, Core.MaxDegreeOfParallelism, progress);
		}

		if (!O.HOnly) {
			PlugTools.ThreadRun(origCanvas.Width, (int x) => {
				HashSet<int> visited = new HashSet<int>();
				for(int y = 0; y < origCanvas.Height; y++ ) {
					if (visited.Contains(y)) { continue; }
					DrawGradientV(visited,origCanvas,canvas,x,y,!O.VOnly);
				}
			}, Core.MaxDegreeOfParallelism, progress);
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

		int len = rx - lx;
		if (len <= 2) {
			// color span is to small so just use colors as-is
			visited.Add(x);
			canvas[x,y] = seed;
			return;
		}

		var lColor = PlugTools.BetweenColor(origCanvas[lx,y],seed,0.5);
		var rColor = PlugTools.BetweenColor(origCanvas[rx,y],seed,0.5);

		for(int gi=0; gi<=len; gi++)
		{
			double ratio = (gi + 1) / (double)len;
			ColorRGBA nc;
			if (ratio > 0.5) {
				nc = PlugTools.BetweenColor(seed,rColor,(ratio - 0.5) * 2.0);
			} else {
				nc = PlugTools.BetweenColor(lColor,seed,ratio * 2.0);
			}
			int gx = lx + gi;
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

		int len = by - ty;
		if (len <= 2) {
			// color span is to small so just use colors as-is
			visited.Add(y);
			var fc = blend ? PlugTools.BetweenColor(seed,canvas[x,y],0.5) : seed;
			canvas[x,y] = fc;
			return;
		}

		var tColor = PlugTools.BetweenColor(frame[x,ty],seed,0.5);
		var bColor = PlugTools.BetweenColor(frame[x,by],seed,0.5);

		for(int gi=0; gi<=len; gi++)
		{
			double ratio = (gi + 1) / (double)len;
			ColorRGBA nc;
			if (ratio > 0.5) {
				nc = PlugTools.BetweenColor(seed,bColor,(ratio - 0.5) * 2.0);
			} else {
				nc = PlugTools.BetweenColor(tColor,seed,ratio * 2.0);
			}
			int gy = ty + gi;
			var fc = blend ? PlugTools.BetweenColor(nc,canvas[x,gy],0.5) : nc;
			canvas[x,gy] = fc;
			visited.Add(gy);
		}
	}

	readonly Options O = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions Core;
}
