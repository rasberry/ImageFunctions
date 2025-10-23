using ImageFunctions.Core.Aides;
using System.Drawing;
using ImgAide = ImageFunctions.Core.Aides.ImageAide;

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
		var canvas = Context.Layers.First().Canvas;

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

		switch(O.Kind) {
		case Options.GradientKind.Linear: break;
		case Options.GradientKind.BiLinear: break;
		case Options.GradientKind.Radial: break;
		case Options.GradientKind.Sqare: break;
		case Options.GradientKind.Conical: break;
		case Options.GradientKind.BiConical: break;
		}

		todo continue

		return true;
	}

}
