namespace ImageFunctions.Core.Functions.Polygon;

public class Function : IFunction
{
	public static IFunction Create(IRegister register, ILayers layers, ICoreOptions options)
	{
		var f = new Function {
			Register = register,
			//CoreOptions = options,
			Layers = layers
		};
		return f;
	}
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb, Register);
	}

	public IOptions Options { get { return O; } }
	readonly Options O = new();
	IRegister Register;
	ILayers Layers;
	//ICoreOptions CoreOptions;

	public bool Run(string[] args)
	{
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!Options.ParseArgs(args, Register)) {
			return false;
		}

		var canvas = Layers.First().Canvas;
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
