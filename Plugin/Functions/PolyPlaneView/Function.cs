using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.ColorSpace;
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
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!Core.ParseArgs(args, Context.Register)) {
			return false;
		}

		Func<double, double, ColorRGBA> colorFunc = Local.UseHueLightness
			? (Local.Gradient == null ? HLColorPure : HLColorGrad)
			: GradientColor
		;

		//we're creating an image so add a layer
		var engine = Context.Options.Engine.Item.Value;
		var (dfw, dfh) = Context.Options.GetDefaultWidthHeight();
		var image = engine.NewCanvasFromLayersOrDefault(Context.Layers, dfw, dfh);
		Context.Layers.Push(image);

		double min = double.MaxValue;
		double max = double.MinValue;

		//find min / max
		image.ThreadPixels(Context, (x, y) => {
			var sum = CalcComplex(x, y);

			var mag = sum.Magnitude;
			if(mag < min) { Interlocked.Exchange(ref min, mag); }
			if(mag > max) { Interlocked.Exchange(ref max, mag); }
		});

		var mathEval = new MathComplexEvaluator

		//fill in pixels
		image.ThreadPixels(Context, (x, y) => {
			var sum = CalcComplex(x, y);
			var mag = sum.Magnitude;

			var scaled = Local.LogScale == null
				? mag / (max - min)
				: ApplyLogScale(mag) / ApplyLogScale(max - min);

			var color = colorFunc(scaled, sum.Phase);
			image[x, y] = color;
		});

		Complex CalcComplex(int x, int y)
		{
			double mx = x * (Local.MaxX - Local.MinX) / image.Width + Local.MinX;
			double my = y * (Local.MaxY - Local.MinY) / image.Height + Local.MinY;
			Complex point = new(mx, my);

			Complex sum = Complex.Zero;
			for(int c = 0; c < Local.Coefficients.Count; c++) {
				var coeff = Local.Coefficients[c];
				sum += coeff * Complex.Pow(point, c + 1);
			}
			return sum;
		}

		return true;
	}

	ColorRGBA GradientColor(double mag, double ph)
	{
		mag = mag * Local.GradientScale % 1.0;
		return Local.Gradient.Value.GetColor(mag);
	}

	readonly ColorSpaceHsl HSLSpace = new();
	ColorRGBA HLColorPure(double mag, double ph)
	{
		ph = (ph + Math.PI) / (2 * Math.PI);
		mag = mag / 2.0 + 0.5;

		var hsl = new ColorSpaceHsl.HSL(ph, 1.0, mag);
		return HSLSpace.ToNative(hsl);
	}

	ColorRGBA HLColorGrad(double mag, double ph)
	{
		ph = (ph + Math.PI) / (2 * Math.PI);
		double grad = ph * Local.GradientScale % 1.0;

		var c = Local.Gradient.Value.GetColor(grad);
		double r = Math.Clamp(c.R + mag, 0.0, 1.0);
		double g = Math.Clamp(c.G + mag, 0.0, 1.0);
		double b = Math.Clamp(c.B + mag, 0.0, 1.0);
		return new ColorRGBA(r, g, b, c.A);
	}

	double ApplyLogScale(double num)
	{
		if (num < double.Epsilon) { return 0.0; }
		return Math.Log(num / Local.LogScale.Value);
	}

	public IOptions Core { get { return Local; } }
	public ILayers Layers { get { return Context.Layers; } }
	Options Local;
	IFunctionContext Context;
}
