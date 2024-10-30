using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;
using System.Drawing;
using CoreColors = ImageFunctions.Core.Aides.ColorAide;

namespace ImageFunctions.Plugin.Functions.AreaSmoother;

[InternalRegisterFunction(nameof(AreaSmoother))]
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

		var frame = Context.Layers.First().Canvas;
		using var progress = new ProgressBar();
		using var canvas = Context.Options.Engine.Item.Value.NewCanvasFromLayers(Context.Layers);
		var maxThreads = Context.Options.MaxDegreeOfParallelism.GetValueOrDefault(1);
		frame.ThreadPixels((x, y) => {
			var nc = SmoothPixel(frame, x, y);
			canvas[x, y] = nc;
		}, maxThreads, progress);

		frame.CopyFrom(canvas);
		return true;
	}

	ColorRGBA SmoothPixel(ICanvas frame, int px, int py)
	{
		ColorRGBA start = frame[px, py];

		//Log.Debug("px="+px+" py="+py+" start = "+start);
		double bestlen = double.MaxValue;
		double bestang = double.NaN;
		double bestratio = 1;
		ColorRGBA bestfc = start;
		ColorRGBA bestbc = start;
		double ahigh = Math.PI;
		double alow = 0;

		for(int tries = 1; tries <= O.TotalTries; tries++) {
			double dang = (ahigh - alow) / 3;
			for(double a = alow; a < ahigh; a += dang) {
				Point fp = FindColorAlongRay(frame, a, px, py, false, start, out ColorRGBA fc);
				Point bp = FindColorAlongRay(frame, a, px, py, true, start, out ColorRGBA bc);

				double len = O.Measurer.Value.Measure(fp.X, fp.Y, bp.X, bp.Y);

				if(len < bestlen) {
					bestang = a;
					bestlen = len;
					bestfc = CoreColors.BetweenColor(fc, start, 0.5);
					bestbc = CoreColors.BetweenColor(bc, start, 0.5);
					double flen = O.Measurer.Value.Measure(px, py, fp.X, fp.Y);
					bestratio = flen / len;
					//Log.Debug("bestratio="+bestratio+" bestfc = "+bestfc+" bestbc="+bestbc);
				}
			}

			alow = bestang - Math.PI / 3 / tries;
			ahigh = bestang + Math.PI / 3 / tries;
		}

		if(O.DrawRatio) {
			return CoreColors.BetweenColor(CoreColors.Black, CoreColors.White, bestratio);
		}

		ColorRGBA final;
		// Log.Debug("bestfc = "+bestfc+" bestbc="+bestbc);
		if(bestfc.Equals(start) && bestbc.Equals(start)) {
			final = start;
		}
		else if(bestratio > 0.5) {
			final = CoreColors.BetweenColor(start, bestbc, (bestratio - 0.5) * 2);
		}
		else {
			final = CoreColors.BetweenColor(bestfc, start, bestratio * 2);
		}
		return final;
	}

	Point FindColorAlongRay(ICanvas canvas, double a, int px, int py, bool back, ColorRGBA start, out ColorRGBA c)
	{
		double r = 1;
		c = start;
		bool done = false;
		double cosa = Math.Cos(a) * (back ? -1 : 1);
		double sina = Math.Sin(a) * (back ? -1 : 1);
		int maxx = canvas.Width - 1;
		int maxy = canvas.Height - 1;

		while(true) {
			int fx = (int)(cosa * r) + px;
			int fy = (int)(sina * r) + py;
			if(fx < 0 || fy < 0 || fx > maxx || fy > maxy) {
				done = true;
			}
			if(!done) {
				ColorRGBA f = O.Sampler.Value.GetSample(canvas, fx, fy);
				if(!f.Equals(start)) {
					c = f;
					done = true;

				}
			}
			if(done) {
				int ix = fx;
				int iy = fy;
				return new Point(
					ix < 0 ? 0 : ix > maxx ? maxx : ix
					, iy < 0 ? 0 : iy > maxy ? maxy : iy
				);
			}
			r += 1;
		}
	}
}
