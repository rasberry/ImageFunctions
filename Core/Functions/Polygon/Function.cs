namespace ImageFunctions.Core.Functions.Polygon;

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

	public bool Run(string[] args)
	{
		if(Context.Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!Options.ParseArgs(args, Context.Register)) {
			return false;
		}

		var canvas = Context.Layers.First().Canvas;
		if(O.PointList == null || O.PointList.Count < 2) {
			return true; //nothing to do
		}

		// //all Draw function have the same signature, so choose the one to use
		// Action<ICanvas, ColorRGBA, Point, Point> DrawMethod = O.Kind switch {
		// 	Line.Options.LineKind.Bresenham => DrawBresenham,
		// 	Line.Options.LineKind.XiaolinWu => DrawXiaolinWu,
		// 	Line.Options.LineKind.RunLengthSlice => DrawRunLengthSlice,
		// 	_ => DrawLineDDA,
		// };

		// // draw lines until we run out of points
		// int pCount = O.PointList.Count;
		// for(int p = 1; p < pCount; p++) {
		// 	var sp = O.PointList[p-1];
		// 	var ep = O.PointList[p];
		// 	DrawMethod(canvas, O.Color, sp, ep);
		// }

		return true;
	}
}
