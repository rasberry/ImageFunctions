using System.Drawing;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.ZoomBlur;

[InternalRegisterFunction(nameof(ZoomBlur))]
public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		O.Usage(sb);
	}

	public bool Run(IRegister register, ILayers layers, ICoreOptions core, string[] args)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if (!O.ParseArgs(args, register)) {
			return false;
		}

		if (layers.Count < 1) {
			Tell.LayerMustHaveAtLeast();
			return false;
		}

		var engine = core.Engine.Item.Value;
		int maxThreads = core.MaxDegreeOfParallelism.GetValueOrDefault(1);

		var source = layers.First();
		using var progress = new ProgressBar();
		using var canvas = engine.NewCanvasFromLayers(layers);
		var bounds = canvas.Bounds();

		double w2 = bounds.Width / 2.0;
		double h2 = bounds.Height / 2.0;

		if (O.CenterPx.HasValue) {
			w2 = O.CenterPx.Value.X;
			h2 = O.CenterPx.Value.Y;
		}
		else if (O.CenterRt.HasValue) {
			w2 = bounds.Width * O.CenterRt.Value.X;
			h2 = bounds.Height * O.CenterRt.Value.Y;
		}

		Tools.ThreadPixels(bounds, (x,y) => {
			var d = source[x,y];
			//Log.Debug($"pixel1 [{x},{y}] = ({d.R} {d.G} {d.B} {d.A})");
			ColorRGBA nc = ZoomPixel(source,bounds,x,y,w2,h2);
			int cy = y - bounds.Top;
			int cx = x - bounds.Left;
			//Log.Debug($"pixel2 [{cx},{cy}] = ({nc.R} {nc.G} {nc.B} {nc.A})");
			canvas[cx,cy] = nc;
		},maxThreads,progress);

		source.CopyFrom(canvas, bounds);
		return true;
	}

	ColorRGBA ZoomPixel(ICanvas frame, Rectangle rect, int x, int y,double cx, double cy)
	{
		var sampler = O.Sampler.Value;
		double dist = O.Measurer.Value.Measure(x,y,cx,cy);
		int idist = (int)Math.Ceiling(dist);

		List<ColorRGBA> vector = new List<ColorRGBA>(idist);
		double ang = Math.Atan2(y - cy, x - cx);
		double sd = dist;
		double ed = dist * O.ZoomAmount;

		for (double d = sd; d < ed; d++)
		{
			double px = Math.Cos(ang) * d + cx;
			double py = Math.Sin(ang) * d + cy;
			ColorRGBA c = sampler.GetSample(frame,(int)px,(int)py);
			vector.Add(c);
		}

		ColorRGBA avg;
		int count = vector.Count;
		if (count < 1) {
			avg  = sampler.GetSample(frame,x,y);
		}
		else if (count == 1) {
			avg = vector[0];
		}
		else {
			double cr = 0, cg = 0, cb = 0, ca = 0;
			foreach (ColorRGBA tpc in vector)
			{
				cr += tpc.R; cg += tpc.G; cb += tpc.B;
				ca += tpc.A;
			}
			avg = new ColorRGBA(
				cr / count,
				cg / count,
				cb / count,
				ca / count
			);
		}
		return avg;
	}

	Options O = new Options();
}