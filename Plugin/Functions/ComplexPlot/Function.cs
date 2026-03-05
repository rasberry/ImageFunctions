using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.ColorSpace;
using Rasberry.MathEval.MathComplex;
using System.Numerics;

namespace ImageFunctions.Plugin.Functions.ComplexPlot;

// turns out this is basically the same thing as ComplexPlot
// https://reference.wolfram.com/language/ref/ComplexPlot.html
// https://www.wolframalpha.com/input?i=ComplexPlot%5Bz%5E3%2Bz%5E2%2Bz%2B1%2C+%7Bz%2C+-4+-+4+I%2C+4+%2B+4+I%7D%5D
// and this https://github.com/nschloe/cplot
// https://en.wikipedia.org/wiki/Domain_coloring

[InternalRegisterFunction(nameof(ComplexPlot))]
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

	public bool Run(string[] args)
	{
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!Core.ParseArgs(args, Context.Register)) {
			return false;
		}

		Func<double, double, ColorRGBA> colorFunc = Local.Gradient == null
			? Local.MagColorOnly ? ColorHLMag : Local.PhaColorOnly ? ColorHLPha : ColorHL
			: Local.MagColorOnly ? ColorGradMag : Local.PhaColorOnly ? ColorGradPha : ColorGrad
		;

		// Func<double, double, ColorRGBA> colorFunc = Local.UseHueLightness
		// 	? (Local.Gradient == null ? HLColorPure : HLColorGrad)
		// 	: GradientColor
		// ;

		//Test that the expression works
		var mathEval = new MathComplexCompiled();
		var evalResult = mathEval.PrepareExpression(Local.Expression);
		if (evalResult.ErrorCount > 0) {
			Context.Log.Error($"Failed expression: '{Local.Expression}'");
			Context.Log.Error(evalResult.ErrorMessage);
			return false;
		}
		var evaluator = evalResult.Evaluator;

		//we're creating an image so add a layer
		var engine = Context.Options.Engine.Item.Value;
		var (dfw, dfh) = Context.Options.GetDefaultWidthHeight();
		var image = engine.NewCanvasFromLayersOrDefault(Context.Layers, dfw, dfh);
		Context.Layers.Push(image);

		//double min = double.MaxValue;
		//double max = double.MinValue;

		//find min / max
		// image.ThreadPixels(Context, (x, y) => {
		// 	var calc = CalcComplex(x, y);
		// 	var mag = calc.Magnitude;
		// 	if(mag < min) { Interlocked.Exchange(ref min, mag); }
		// 	if(mag > max) { Interlocked.Exchange(ref max, mag); }
		// });

		//fill in pixels
		image.ThreadPixels(Context, (x, y) => {
			var calc = CalcComplex(x, y);
			if (Complex.IsNaN(calc)) { calc = Complex.Zero; }

			var mag = calc.Magnitude;
			var scaled = 2.0 * Math.Atan(mag)/Math.PI;
			scaled = FlattenMid(scaled, Local.FlatPower);

			var color = colorFunc(scaled, calc.Phase);
			image[x, y] = color;
		});

		Complex CalcComplex(int x, int y)
		{
			double mx = x * (Local.MaxX - Local.MinX) / image.Width + Local.MinX;
			double my = y * (Local.MaxY - Local.MinY) / image.Height + Local.MinY;
			Complex point = new(mx, -my); //flip y to match traditional geometric axes

			var result = evaluator.Evaluate(point);
			return result;
		}

		return true;
	}
		
	//not sure what this function is called (hill function?)
	// x^k / (x^k + (1-x)^k) <=> 1 / (1 + ((1-x)/x)^k)
	static double FlattenMid(double x, double strength)
	{
		if (strength == 1.0) { return x; }
		double s = 1.0 / strength;

		double px = Math.Pow((1.0-x)/x,s);
		return 1.0 / (1.0 + px);
	}

	ColorRGBA ColorGradMag(double mag, double ph)
	{
		mag = (mag * Local.GradientScale + Local.GradOffset) % 1.0;
		return Local.Gradient.Value.GetColor(mag);
	}

	ColorRGBA ColorGradPha(double mag, double ph)
	{
		return ColorGrad(0.5,ph);
	}

	ColorRGBA ColorGrad(double mag, double ph)
	{
		ph = (ph + Math.PI) / (2 * Math.PI);
		double grad = (ph * Local.GradientScale + Local.GradOffset) % 1.0;

		var c = Local.Gradient.Value.GetColor(grad);
		double r = Math.Clamp(c.R * mag * 2.0, 0.0, 1.0);
		double g = Math.Clamp(c.G * mag * 2.0, 0.0, 1.0);
		double b = Math.Clamp(c.B * mag * 2.0, 0.0, 1.0);
		return new ColorRGBA(r, g, b, c.A);
	}

	readonly ColorSpaceHsl HSLSpace = new();
	ColorRGBA ColorHL(double mag, double ph)
	{
		ph = (ph + Math.PI) / (2 * Math.PI);
		var hsl = new ColorSpaceHsl.HSL(ph, 1.0, mag);
		return HSLSpace.ToNative(hsl);
	}

	ColorRGBA ColorHLMag(double mag, double ph)
	{
		return ColorHL(mag,0.0);
	}

	ColorRGBA ColorHLPha(double mag, double ph)
	{
		return ColorHL(0.5,ph);
	}

	public IOptions Core { get { return Local; } }
	public ILayers Layers { get { return Context.Layers; } }
	Options Local;
	IFunctionContext Context;
}
