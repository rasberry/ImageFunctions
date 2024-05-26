using ImageFunctions.Core;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.Life;

[InternalRegisterFunction(nameof(Life))]
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
		Options.Usage(sb, Register);
	}

	public bool Run(string[] args)
	{
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!Options.ParseArgs(args, Register)) {
			return false;
		}

		if(Layers.Count < 1) {
			Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}

		ICanvas canvas;
		if(Options.MakeNewLayer) {
			canvas = Tools.NewCanvasFromLayers(Core.Engine.Item.Value, Layers);
			PlugTools.CopyFrom(canvas, Layers.First().Canvas);
		}
		else {
			canvas = Layers.First().Canvas;
		}

		if(Options.UseChannels) {
			ProgressRatio = 1.0 / 3.0; ProgressOffset = 0.0;
			RunSimulation(canvas, Channel.Red);
			ProgressOffset = 1.0 / 3.0;
			RunSimulation(canvas, Channel.Green);
			ProgressOffset = 2.0 / 3.0;
			RunSimulation(canvas, Channel.Blue);
		}
		else {
			ProgressRatio = 1.0; ProgressOffset = 0.0;
			RunSimulation(canvas, Channel.All);
		}

		return true;
	}

	void RunSimulation(ICanvas canvas, Channel channel)
	{
		HashSet<Point> last = new();
		Dictionary<Point, ulong> history = Options.NoHistory ? null : new();

		//populate world
		for(int y = 0; y < canvas.Height; y++) {
			for(int x = 0; x < canvas.Width; x++) {
				var c = canvas[x, y];
				if(IsAlive(c, channel)) {
					last.Add(new Point(x, y));
				}
			}
		}

		DoSimulation(canvas.Width, canvas.Height, last, history);

		Tools.ThreadPixels(canvas, (x, y) => {
			var p = new Point(x, y);
			if(last.Contains(p)) {
				UpdatePixel(canvas, x, y, PlugColors.White, channel);
			}
			else if(history != null && history.TryGetValue(p, out ulong count)) {
				double pct = (double)count / Options.IterationMax;
				double pctAdjust = Options.Brighten.HasValue ? Math.Pow(pct, 1.0 - Options.Brighten.Value) : pct;
				var c = new ColorRGBA(pctAdjust, pctAdjust, pctAdjust, 1.0);
				UpdatePixel(canvas, x, y, c, channel);
			}
			else {
				UpdatePixel(canvas, x, y, PlugColors.Black, channel);
			}
		});
	}

	void UpdatePixel(ICanvas canvas, int x, int y, ColorRGBA color, Channel channel)
	{
		canvas[x, y] = channel switch {
			Channel.Red => canvas[x, y] with { R = color.R },
			Channel.Green => canvas[x, y] with { G = color.G },
			Channel.Blue => canvas[x, y] with { B = color.B },
			_ => color,
		};
	}

	void DoSimulation(int W, int H, HashSet<Point> last, Dictionary<Point, ulong> history = null)
	{
		HashSet<Point> next = new();
		HashSet<Point> dead = new();

		using var progress = new Rasberry.Cli.ProgressBar();
		ulong iter = 0u;
		int lastPop = last.Count;
		ulong lastStable = 0;

		while(iter < Options.IterationMax) {
			progress.Prefix = $"Population {last.Count} ";
			progress.Report((double)iter / Options.IterationMax * ProgressRatio + ProgressOffset);

			RunIteration(W, H, ref next, ref last, dead, history);
			iter++;

			//Log.Debug($"Iter={iter} Pop={Last.Count}");
			if(Options.StopWhenStable && lastPop == last.Count) {
				lastStable++;
				if(lastStable > 9) {
					Log.Message($"Stopping. Population stabilized at {last.Count}");
					break;
				}
			}
			else {
				lastStable = 0;
			}
			lastPop = last.Count;
		}
	}

	void RunIteration(int W, int H,
		ref HashSet<Point> next, ref HashSet<Point> last, HashSet<Point> dead, Dictionary<Point, ulong> history = null)
	{
		//add neighbors to be checked - need to check these since they could become alive
		foreach(var p in last) {
			next.Add(p);
			foreach(var n in GetNeighbors(p.X, p.Y, W, H)) {
				next.Add(n);
			}
		}

		//check all next points
		foreach(var p in next) {
			int count = 0;
			foreach(var n in GetNeighbors(p.X, p.Y, W, H)) {
				if(last.Contains(n)) {
					count++;
				}
			}

			//Log.Debug($"p={p} count={count}{(Last.Contains(p) ? " *" : "")}");
			//if (Last.Contains(p)) { Log.Debug($"p={p} count={count}"); }

			bool isAlive = count == 3
				|| count == 2 && last.Contains(p);

			if(!isAlive) {
				dead.Add(p);
			}
			else if(history != null) {
				if(!history.ContainsKey(p)) {
					history.Add(p, 1);
				}
				else {
					history[p] += 1;
				}
			}
		}

		//remove dead
		foreach(var p in dead) {
			next.Remove(p);
		}

		(last, next) = (next, last); //swap last and next
		dead.Clear();
		next.Clear();
	}

	IEnumerable<Point> GetNeighbors(int x, int y, int w, int h)
	{
		int L = w - 1;
		int B = h - 1;

		if(Options.Wrap) {
			int xm = x > 0 ? x - 1 : L + x;
			int xp = x < L ? x + 1 : x - L;
			int ym = y > 0 ? y - 1 : B + y;
			int yp = y < B ? y + 1 : y - B;
			//Log.Debug($"GN [{x} {y} {w} {h}] = {xm} {xp} {ym} {yp}");

			yield return new Point(xm, ym);
			yield return new Point(xm, yp);
			yield return new Point(xm, y);

			yield return new Point(xp, ym);
			yield return new Point(xp, yp);
			yield return new Point(xp, y);

			yield return new Point(x, ym);
			yield return new Point(x, yp);
		}
		else {
			if(x > 0) {
				if(y > 0) { yield return new Point(x - 1, y - 1); }
				if(y < B) { yield return new Point(x - 1, y + 1); }
				yield return new Point(x - 1, y);
			}
			if(x < L) {
				if(y > 0) { yield return new Point(x + 1, y - 1); }
				if(y < B) { yield return new Point(x + 1, y + 1); }
				yield return new Point(x + 1, y);
			}
			if(y > 0) { yield return new Point(x, y - 1); }
			if(y < B) { yield return new Point(x, y + 1); }
		}
	}

	bool IsAlive(ColorRGBA color, Channel channel)
	{
		return channel switch {
			Channel.Red => color.R >= Options.Threshhold,
			Channel.Green => color.G >= Options.Threshhold,
			Channel.Blue => color.B >= Options.Threshhold,
			_ => color.Luma >= Options.Threshhold,
		};
	}

	readonly Options Options = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions Core;
	double ProgressRatio;
	double ProgressOffset;
}
