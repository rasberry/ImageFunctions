using System.Drawing;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.PixelRules;

[InternalRegisterFunction(nameof(PixelRules))]
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
		O.Usage(sb);
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
			Tell.LayerMustHaveAtLeast();
			return false;
		}

		var engine = Core.Engine.Item.Value;
		int maxThreads = Core.MaxDegreeOfParallelism.GetValueOrDefault(1);
		var source = Layers.First();
		using var progress = new ProgressBar();
		using var canvas = engine.NewCanvasFromLayers(Layers);
		var rect = source.Bounds();

		for(int p=0; p<O.Passes; p++) {
			progress.Prefix = "Pass "+(p+1)+"/"+O.Passes+" ";
			Tools.ThreadPixels(canvas, (x,y) => {
				ColorRGBA nc = RunRule(source,rect,x,y);
				canvas[x,y] = nc;
			},maxThreads,progress);
			source.CopyFrom(canvas);
		}

		return true;
	}

	ColorRGBA RunRule(ICanvas frame,Rectangle rect, int x, int y)
	{
		int cx = x, cy = y;
		var history = new List<ColorRGBA>();
		int max = O.MaxIters;

		while(--max >= 0) {
			ColorRGBA ant = frame[cx,cy];
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

	ColorRGBA FindAverageColor(IEnumerable<ColorRGBA> list)
	{
		double r = 0.0,g = 0.0,b = 0.0,a = 0.0;
		int count = 0;

		foreach(ColorRGBA c in list) {
			r += c.R;
			g += c.G;
			b += c.B;
			a += c.A;
			count++;
		}

		var avg = new ColorRGBA(
			r / count,
			g / count,
			b / count,
			a / count
		);
		return avg;
	}

	bool IsBetterPixel(ICanvas frame, Rectangle rect, ColorRGBA? best, int x, int y, out int bx, out int by)
	{
		bx = by = 0;
		ColorRGBA? nn,ne,ee,se,ss,sw,ww,nw;
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
	bool PickBetter(ref ColorRGBA? best, ColorRGBA? bid, ref double min)
	{
		//if best is null anything is better
		if (best == null) {
			best = bid;
			min = double.MaxValue;
			return true;
		}
		//both are good
		if (bid != null) {
			if (O.WhichMode == Mode.StairCaseAscend
				|| O.WhichMode == Mode.StairCaseDescend)
			{
				//only follow darker colors
				ColorRGBA white = PlugColors.White;
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

	double Dist(ColorRGBA one, ColorRGBA two)
	{
		//treat identical pixels as very far apart
		if (one.Equals(two)) {
			return double.MaxValue;
		}

		var o = one;
		var t = two;

		bool normal = O.WhichMode == Mode.StairCaseDescend
			|| O.WhichMode == Mode.StairCaseClosest;
		double[] vo = normal
			? new double[] { o.R, o.G, o.B, o.A }
			: new double[] { 1.0 - o.R, 1.0 - o.G, 1.0 - o.B, 1.0 - o.A }
		;
		double[] vt = new double[] { t.R, t.G, t.B, t.A };

		double dist = O.Metric.Value.Measure(vo,vt);
		return dist;
	}

	readonly Options O = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions Core;
}