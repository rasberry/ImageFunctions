using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using System.Numerics;

namespace ImageFunctions.Plugin.Functions.PolyPlaneView;

// turns out this is basically the same thing as ComplexPlot
// https://reference.wolfram.com/language/ref/ComplexPlot.html
// https://www.wolframalpha.com/input?i=ComplexPlot%5Bz%5E3%2Bz%5E2%2Bz%2B1%2C+%7Bz%2C+-4+-+4+I%2C+4+%2B+4+I%7D%5D

[InternalRegisterFunction(nameof(PolyPlaneView))]
public class Function : IFunction
{
	public static IFunction Create(IFunctionContext context)
	{
		if(context == null) {
			throw Squeal.ArgumentNull(nameof(context));
		}

		var f = new Function {
			Context = context,
			Local = new(context),
		};
		return f;
	}

	public void Usage(StringBuilder sb)
	{
		Core.Usage(sb, Context.Register);
	}

	//TODO look at using https://c-ohle.github.io/RationalNumerics/index.html https://www.nuget.org/packages/BigRational
	public bool Run(string[] args)
	{
		if (Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if (!Core.ParseArgs(args, Context.Register)) {
			return false;
		}

		//we're creating an image so add a layer
		var engine = Context.Options.Engine.Item.Value;
		var (dfw, dfh) = Context.Options.GetDefaultWidthHeight();
		var image = engine.NewCanvasFromLayersOrDefault(Context.Layers, dfw, dfh);
		Context.Layers.Push(image);

		image.ThreadPixels(Context, (x,y) => {
			double mx = x * (Local.MaxX - Local.MinX) / image.Width + Local.MinX;
			double my = y * (Local.MaxY - Local.MinY) / image.Height + Local.MinY;

			Complex point = new(mx,my);

			Complex sum = Complex.Zero;
			for(int c = 0; c < Local.Coefficients.Count; c++) {
				var coeff = Local.Coefficients[c];
				sum += coeff * Complex.Pow(point,c + 1);
			}

			// Context.Log.Debug($"sum = {sum.Magnitude}");
			var color = Local.Gradient.Value.GetColor(sum.Magnitude % 1.0);
			image[x,y] = color;
		});

		return true;
	}

	public IOptions Core { get { return Local; } }
	public ILayers Layers { get { return Context.Layers; } }
	Options Local;
	IFunctionContext Context;
}
