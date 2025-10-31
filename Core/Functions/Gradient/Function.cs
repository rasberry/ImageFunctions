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
		if(!O.StartPct.IsEmpty) {
			var x = (int)Math.Round(canvas.Width * O.StartPct.X);
			var y = (int)Math.Round(canvas.Height * O.StartPct.Y);
			startPoint = new Point(
				Math.Clamp(x, 0, canvas.Width),
				Math.Clamp(y, 0, canvas.Height)
			);
		}

		if(!O.EndPct.IsEmpty) {
			var x = (int)Math.Round(canvas.Width * O.EndPct.X);
			var y = (int)Math.Round(canvas.Height * O.EndPct.Y);
			endPoint = new Point(
				Math.Clamp(x, 0, canvas.Width),
				Math.Clamp(y, 0, canvas.Height)
			);
		}

		if(startPoint.IsEmpty) {
			startPoint = new Point(0, 0);
		}
		if(endPoint.IsEmpty) {
			endPoint = new Point(canvas.Width - 1, 0);
		}

		Func<Point, Point, Point, IMetric, double> calcFunc;
		calcFunc = O.Kind switch {
			Options.GradientKind.Linear => CalcLinear,
			Options.GradientKind.Radial => CalcRadial,
			Options.GradientKind.Square => CalcSquare,
			Options.GradientKind.Conical => CalcConical,
			_ => CalcLinear
		};

		canvas.ThreadPixels((x, y) => {
			var grad = calcFunc(startPoint, endPoint, new Point(x, y), O.Metric.Value);
			//Context.Log.Debug($"[{x},{y}] s:{startPoint} e:{endPoint} grad:{grad}");
			var pos = (O.Speed * grad + O.Phase) % 1.0; //TODO other modes ? back2back front2back
			var color = O.Gradient.Value.GetColor(Math.Clamp(pos,0.0,1.0));
			//Context.Log.Debug($"[{x},{y}] o:{O.Offset} grad:{grad} pos:{pos} color:{color}");
			canvas[x, y] = color;
		}, Context.Token, Context.Options.MaxDegreeOfParallelism, Context.Progress);

		return true;
	}

	double CalcLinear(Point start, Point end, Point pos, IMetric metric)
	{
		var len = metric.Measure(start.X, start.Y, end.X, end.Y);
		if(len < double.Epsilon) { return 0.0; } //prevent divide by zero

		var dxL = end.X - start.X;
		var dyL = end.Y - start.Y;
		var dxP = pos.X - start.X;
		var dyP = pos.Y - start.Y;

		//dot product or something
		var dp = dxL * dxP + dyL * dyP;
		var t = dp / (len * len);

		var nx = start.X + t * dxL;
		var ny = start.Y + t * dyL;

		var dist = metric.Measure(start.X, start.Y, nx, ny);
		return dist / len;
	}

	double CalcRadial(Point start, Point end, Point pos, IMetric metric)
	{
		var len = metric.Measure(start.X, start.Y, end.X, end.Y);
		if(len < double.Epsilon) { return 0.0; } //prevent divide by zero

		var dist = metric.Measure(start.X, start.Y, pos.X, pos.Y);
		return dist / len;
	}

	double CalcSquare(Point start, Point end, Point pos, IMetric metric)
	{
		var dx = Math.Abs(start.X - end.X);
		var dy = Math.Abs(start.Y - end.Y);
		var len = Math.Max(dx, dy);
		if(len <= 0) { return 0.0; } //prevent divide by zero

		var px = Math.Abs(start.X - pos.X);
		var py = Math.Abs(start.Y - pos.Y);
		var plen = Math.Max(px, py);

		return (double)plen / len;
	}

	double CalcConical(Point start, Point end, Point pos, IMetric metric)
	{
		var dx = end.X - start.X;
		var dy = end.Y - start.Y;
		var px = pos.X - start.X;
		var py = pos.Y - start.Y;

		var ang = Math.Atan2(dy, dx) - Math.Atan2(py, px);
		var norm = (ang < 0 ? ang + 2 * Math.PI : ang) / (2 * Math.PI); //normalzie to [0,1)
		return norm;
	}
}
