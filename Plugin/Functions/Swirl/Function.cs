using System.Drawing;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Swirl;

[InternalRegisterFunction(nameof(Swirl))]
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
		var source = layers.First();

		double swirlRadius;
		double swirlTwists = O.Rotations;
		double swirlx, swirly;
		Rectangle rect = source.Bounds();

		if (O.RadiusPx != null) {
			swirlRadius = O.RadiusPx.Value;
		}
		else {
			double m = O.RadiusPp != null ? O.RadiusPp.Value : 0.9;
			swirlRadius = m * Math.Min(rect.Width,rect.Height);
		}

		if (O.CenterPx != null) {
			swirlx = O.CenterPx.Value.X;
			swirly = O.CenterPx.Value.Y;
		}
		else {
			double px = O.CenterPp != null ? O.CenterPp.Value.X : 0.5;
			double py = O.CenterPp != null ? O.CenterPp.Value.Y : 0.5;
			swirlx = rect.Width * px + rect.Left;
			swirly = rect.Height * py + rect.Top;
		}

		var engine = core.Engine.Item.Value;
		int maxThreads = core.MaxDegreeOfParallelism.GetValueOrDefault(1);
		using var progress = new ProgressBar();
		using var canvas = engine.NewCanvasFromLayers(layers);

		rect.ThreadPixels((x,y) => {
			int cy = y - rect.Top;
			int cx = x - rect.Left;
			ColorRGBA nc = SwirlPixel(source,x,y,swirlx,swirly,swirlRadius,swirlTwists);
			canvas[cx,cy] = nc;
		},maxThreads,progress);

		source.CopyFrom(canvas,rect);
		return true;
	}

	ColorRGBA SwirlPixel(ICanvas frame,
		double x, double y, double swirlx, double swirly,
		double swirlRadius, double swirlTwists)
	{
		double pixelx = x - swirlx;
		double pixely = y - swirly;
		double pixelDist = O.Metric.Value.Measure(x,y,swirlx,swirly);
		double swirlAmount = 1.0 - (pixelDist / swirlRadius);

		if (swirlAmount > 0.0) {
			double twistAngle = swirlTwists * swirlAmount * Math.PI * 2;
			if (!O.CounterClockwise) { twistAngle *= -1.0; }
			double pixelAng = Math.Atan2(pixely,pixelx) + twistAngle;
			pixelx = Math.Cos(pixelAng) * pixelDist;
			pixely = Math.Sin(pixelAng) * pixelDist;
		}

		var c = O.Sampler.Value.GetSample(frame,
			(int)(swirlx + pixelx), (int)(swirly + pixely));
		return c;
	}

	Options O = new Options();
}