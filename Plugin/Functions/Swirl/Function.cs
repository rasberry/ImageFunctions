using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.Swirl;

[InternalRegisterFunction(nameof(Swirl))]
public class Function : IFunction
{
	public static IFunction Create(IFunctionContext context)
	{
		if(context == null) {
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
	ILayers Layers { get { return Context.Layers; } }
	ICoreOptions CoreOptions { get { return Context.Options; } }

	public bool Run(string[] args)
	{
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!O.ParseArgs(args, Context.Register)) {
			return false;
		}
		if(Layers.Count < 1) {
			Context.Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}
		var source = Layers.First().Canvas;

		double swirlRadius;
		double swirlTwists = O.Rotations;
		double swirlx, swirly;
		Rectangle rect = source.Bounds();

		if(O.RadiusPx != null) {
			swirlRadius = O.RadiusPx.Value;
		}
		else {
			double m = O.RadiusPp != null ? O.RadiusPp.Value : 0.9;
			swirlRadius = m * Math.Min(rect.Width, rect.Height);
		}

		if(O.CenterPx != null) {
			swirlx = O.CenterPx.Value.X;
			swirly = O.CenterPx.Value.Y;
		}
		else {
			double px = O.CenterPp != null ? O.CenterPp.Value.X : 0.5;
			double py = O.CenterPp != null ? O.CenterPp.Value.Y : 0.5;
			swirlx = rect.Width * px + rect.Left;
			swirly = rect.Height * py + rect.Top;
		}

		var engine = CoreOptions.Engine.Item.Value;
		using var progress = new ProgressBar();
		using var canvas = engine.NewCanvasFromLayers(Layers);

		rect.ThreadPixels((x, y) => {
			int cy = y - rect.Top;
			int cx = x - rect.Left;
			ColorRGBA nc = SwirlPixel(source, x, y, swirlx, swirly, swirlRadius, swirlTwists);
			canvas[cx, cy] = nc;
		}, CoreOptions.MaxDegreeOfParallelism, progress);

		source.CopyFrom(canvas, rect);
		return true;
	}

	ColorRGBA SwirlPixel(ICanvas frame,
		double x, double y, double swirlx, double swirly,
		double swirlRadius, double swirlTwists)
	{
		double pixelx = x - swirlx;
		double pixely = y - swirly;
		double pixelDist = O.Metric.Value.Measure(x, y, swirlx, swirly);
		double swirlAmount = 1.0 - (pixelDist / swirlRadius);

		if(swirlAmount > 0.0) {
			double twistAngle = swirlTwists * swirlAmount * Math.PI * 2;
			if(!O.CounterClockwise) { twistAngle *= -1.0; }
			double pixelAng = Math.Atan2(pixely, pixelx) + twistAngle;
			pixelx = Math.Cos(pixelAng) * pixelDist;
			pixely = Math.Sin(pixelAng) * pixelDist;
		}

		var c = O.Sampler.Value.GetSample(frame,
			(int)(swirlx + pixelx), (int)(swirly + pixely));
		return c;
	}
}
