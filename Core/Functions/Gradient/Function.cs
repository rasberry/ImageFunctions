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
			Options.GradientKind.BiLinear => null,
			Options.GradientKind.Radial => null,
			Options.GradientKind.Sqare => null,
			Options.GradientKind.Conical => null,
			Options.GradientKind.BiConical => null,
			_ => CalcLinear
		};

		canvas.ThreadPixels((x, y) => {
			var grad = calcFunc(startPoint, endPoint, new Point(x, y), O.Metric.Value);
			// Context.Log.Debug($"[{x},{y}] s:{startPoint} e:{endPoint}");
			var pos = Math.Clamp((1.0 - O.Offset) * grad, 0.0, 1.0);
			var color = O.Gradient.Value.GetColor((1.0 - O.Offset) * grad);
			// Context.Log.Debug($"[{x},{y}] o:{O.Offset} grad:{grad} pos:{pos} color:{color}");
			//var pos = (double)x / canvas.Width;
			//var color = O.Gradient.Value.GetColor(pos);
			canvas[x, y] = color;
		}, Context.Token, Context.Options.MaxDegreeOfParallelism, Context.Progress);

		return true;
	}

	double CalcLinear(Point start, Point end, Point pos, IMetric metric)
	{
		var len = metric.Measure(start.X, start.Y, end.X, end.Y);
		var dxL = metric.Measure(start.X, 0.0, end.X, 0.0);
		var dyL = metric.Measure(start.Y, 0.0, end.Y, 0.0);
		var dxP = metric.Measure(start.X, 0.0, pos.X, 0.0);
		var dyP = metric.Measure(start.Y, 0.0, pos.Y, 0.0);

		var dp = dxL * dxP + dyL * dyP;
		var t = dp / (len * len);

		var nx = start.X + t * dxL;
		var ny = start.Y + t * dyL;

		var dist = metric.Measure(start.X, start.Y, nx, ny);
		return dist / len;
	}


// import math

// def calculate_distance(x1, y1, x2, y2, x3, y3):
//     # Step 1: Calculate the vectors for the line (from A to B) and the vector from A to P
//     dx1 = x2 - x1
//     dy1 = y2 - y1
//     dx2 = x3 - x1
//     dy2 = y3 - y1

//     # Step 2: Calculate the projection scalar (t) using the dot product formula
//     dot_product = dx1 * dx2 + dy1 * dy2
//     line_length_squared = dx1**2 + dy1**2
//     t = dot_product / line_length_squared

//     # Step 3: Find the projection point coordinates (Px, Py)
//     Px = x1 + t * dx1
//     Py = y1 + t * dy1

//     # Step 4: Calculate the distance from the projection point (Px, Py) to the start point (x1, y1)
//     distance = math.sqrt((Px - x1)**2 + (Py - y1)**2)
    
//     return distance

// # Example usage
// x1, y1 = 1, 2  # Line point A
// x2, y2 = 4, 6  # Line point B
// x3, y3 = 3, 3  # External point P

// distance = calculate_distance(x1, y1, x2, y2, x3, y3)
// print("Distance from the projection to the start of the line:", distance)


		//var dist = Math.Abs(dy * pos.X - dx * pos.Y + end.X * start.Y - end.Y * start.X) / len;

		//var grad = len * (1.0 - offset)

		// // first convert line to normalized unit vector
		// double dx = x2 - x1;
		// double dy = y2 - y1;
		// double mag = sqrt(dx*dx + dy*dy);
		// dx /= mag;
		// dy /= mag;

		// // translate the point and get the dot product
		// double lambda = (dx * (x3 - x1)) + (dy * (y3 - y1));
		// x4 = (dx * lambda) + x1;
		// y4 = (dy * lambda) + y1;
		// ndx = x4 - x1;
		// ndy = y4 - y1;
		// ndist = sqrt(ndx*ndx + ndy*ndy)


		// // first convert line to normalized unit vector
		// dx = x2 - x1
		// dy = y2 - y1
		// mag = sqrt(dx*dx + dy*dy)
		// mdx = dx / mag
		// mdy = dy / mag

		// // translate the point and get the dot product
		// lambda = (mdx * (x3 - x1)) + (mdy * (y3 - y1))
		// x4 = (mdx * lambda) + x1
		// y4 = (mdy * lambda) + y1
		// ndx = x4 - x1
		// ndy = y4 - y1
		// ndist = sqrt(ndx*ndx + ndy*ndy)
	//}
}
