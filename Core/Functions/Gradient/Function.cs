using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Metrics;
using System.Drawing;

namespace ImageFunctions.Core.Functions.Gradient;

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
		Core.Usage(sb, Context.Register);
	}

	public IOptions Core { get { return O; } }
	IFunctionContext Context;
	Options O;

	public bool Run(string[] args)
	{
		if(Context.Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!Core.ParseArgs(args, Context.Register)) {
			return false;
		}

		var startPoint = O.Start;
		var endPoint = O.End;

		//since we're rendering pixels make a new layer each time
		var engine = Context.Options.Engine.Item.Value;
		var (dfw, dfh) = Context.Options.GetDefaultWidthHeight();
		var canvas = engine.NewCanvasFromLayersOrDefault(Context.Layers, dfw, dfh);
		Context.Layers.Push(canvas);

		//check % points and set defaults if needed
		if(O.StartPct.HasValue) {
			var x = (int)Math.Round(canvas.Width * O.StartPct.Value.X);
			var y = (int)Math.Round(canvas.Height * O.StartPct.Value.Y);
			startPoint = new Point(
				Math.Clamp(x, 0, canvas.Width),
				Math.Clamp(y, 0, canvas.Height)
			);
		}

		if(O.EndPct.HasValue) {
			var x = (int)Math.Round(canvas.Width * O.EndPct.Value.X);
			var y = (int)Math.Round(canvas.Height * O.EndPct.Value.Y);
			endPoint = new Point(
				Math.Clamp(x, 0, canvas.Width),
				Math.Clamp(y, 0, canvas.Height)
			);
		}

		if(!startPoint.HasValue) {
			startPoint = new Point(0, 0);
		}
		if(!endPoint.HasValue) {
			endPoint = new Point(canvas.Width - 1, 0);
		}

		Func<Point, Point, Point, IMetric, (double position, bool isInside)> calcFunc;
		calcFunc = O.Kind switch {
			Options.GradientKind.Linear => CalcLinear,
			Options.GradientKind.Bilinear => CalcLinear,
			Options.GradientKind.Radial => CalcRadial,
			Options.GradientKind.Square => CalcSquare,
			Options.GradientKind.Conical => CalcConical,
			_ => CalcLinear
		};

		Func<double, double> posFunc;
		posFunc = O.Direction switch {
			Options.DirectionKind.Forward => PosForward,
			Options.DirectionKind.Backward => PosBackward,
			Options.DirectionKind.ForBack => PosForBack,
			Options.DirectionKind.BackFor => PosBackFor,
			_ => PosForward
		};

		canvas.ThreadPixels(Context, (x, y) => {
			var (grad, keep) = calcFunc(startPoint.Value, endPoint.Value, new Point(x, y), O.Metric.Value);
			if(O.Restrict && !keep) { return; } //skip pixels outside the gradient area

			var pos = posFunc(grad);
			var color = O.Gradient.Value.GetColor(Math.Clamp(pos, 0.0, 1.0));
			canvas[x, y] = color;
		});

		return true;
	}

	(double, bool) CalcLinear(Point start, Point end, Point pos, IMetric metric)
	{
		var len = metric.Measure(start.X, start.Y, end.X, end.Y);
		if(len < double.Epsilon) { return (0.0, false); } //prevent divide by zero

		var dxL = end.X - start.X;
		var dyL = end.Y - start.Y;
		var dxP = pos.X - start.X;
		var dyP = pos.Y - start.Y;

		//dot product or something - stays positive along the line, otherwise negative
		var dp = dxL * dxP + dyL * dyP;
		var t = dp / (len * len);

		var nx = start.X + t * dxL;
		var ny = start.Y + t * dyL;

		var isBilinear = O.Kind == Options.GradientKind.Bilinear;

		var dist = metric.Measure(start.X, start.Y, nx, ny);
		var keep = dist <= len && (isBilinear || dp >= 0.0);
		return (dist / len, keep);
	}

	(double, bool) CalcRadial(Point start, Point end, Point pos, IMetric metric)
	{
		var len = metric.Measure(start.X, start.Y, end.X, end.Y);
		if(len < double.Epsilon) { return (0.0, false); } //prevent divide by zero

		var dist = metric.Measure(start.X, start.Y, pos.X, pos.Y);
		return (dist / len, dist <= len);
	}

	(double, bool) CalcSquare(Point start, Point end, Point pos, IMetric metric)
	{
		var dx = Math.Abs(start.X - end.X);
		var dy = Math.Abs(start.Y - end.Y);
		var len = Math.Max(dx, dy);
		if(len <= 0) { return (0.0, false); } //prevent divide by zero

		var px = Math.Abs(start.X - pos.X);
		var py = Math.Abs(start.Y - pos.Y);
		var plen = Math.Max(px, py);

		return ((double)plen / len, plen <= len);
	}

	(double, bool) CalcConical(Point start, Point end, Point pos, IMetric metric)
	{
		var dx = end.X - start.X;
		var dy = end.Y - start.Y;
		var px = pos.X - start.X;
		var py = pos.Y - start.Y;

		var ang = Math.Atan2(dy, dx) - Math.Atan2(py, px);
		var norm = (ang < 0 ? ang + 2 * Math.PI : ang) / (2 * Math.PI); //normalzie to [0,1)
		bool isInside = true;
		if(O.Restrict) {
			var len = metric.Measure(start.X, start.Y, end.X, end.Y);
			var dist = metric.Measure(start.X, start.Y, pos.X, pos.Y);
			isInside = dist <= len;
		}
		return (norm, isInside);
	}

	double PosForward(double grad)
	{
		var pos = O.Speed * grad + O.Phase;
		//var circ = ((pos % 1.0) + pos) % 1.0; //negatives go around
		var circ = pos % 1.0;
		return circ;
	}

	double PosBackward(double grad)
	{
		return 1.0 - PosForward(grad);
	}

	double PosForBack(double grad)
	{
		var pos = O.Speed * grad + O.Phase;
		//var circ = ((pos % 2.0) + pos) % 2.0; //negatives go around
		var circ = 2.0 * pos % 2.0;
		return circ > 1.0 ? 2.0 - circ : circ;
	}

	double PosBackFor(double grad)
	{
		return 1.0 - PosForBack(grad);
	}
}
