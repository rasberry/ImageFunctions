using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using System.Numerics;

namespace ImageFunctions.Plugin.Functions.PolyPlaneView;

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
				//sum += coeff * ModPowOne(point,c + 1);
			}

			//var sum = Complex.Pow(point,3) + Complex.Pow(point,2) + Complex.Pow(point,1) + 1.0;
			//var sum = ModPow(point, 3, 2.0) + ModPow(point, 2, 2.0) + ModPow(point, 1, 2.0) + 1.0;
			// var sum = Complex.Pow(point,3)
			// 	+ Complex.Pow(point,2)
			// 	+ Complex.Pow(point,1)
			// 	+ 1.0;
			// var sum = 1.0 + Complex.Pow(point,1) + Complex.Pow(point,2) + Complex.Pow(point,3);

			// double sum = Local.Coefficients[0];
			// for(int c = 0; c < Local.Coefficients.Count; c++) {
			// 	var raised = ModPow(point, c + 1, 1.0);
			// 	var coeff = Local.Coefficients[c];
			// 	sum = (sum + coeff * raised.Magnitude) % 1.0;
			// }


			// double phase = point.Phase;
			// double mag = point.Magnitude;
			
			// ModMath phaseMod = new(2*Math.PI);
			// ModMath magMod = new(1.0);

			// phase = 
			// phaseMod.Add(
			// 	phaseMod.Add(
			// 		phaseMod.Multiply(phase, 3),
			// 		phaseMod.Multiply(phase, 2)
			// 	),
			// 	phase
			// );
			
			// mag =
			// phaseMod.Add(
			// 	phaseMod.Add(
			// 		magMod.Pow(mag, 3),
			// 		magMod.Pow(mag, 2)
			// 	),
			// 	mag
			// );

			// var sum = ModPow(mag,phase, 3, 1.0) + ModPow(mag,phase, 2, 1.0) + ModPow(mag,phase, 1, 1.0) + 1.0;
			//phase.Multiply(3);

			Context.Log.Debug($"sum = {sum.Magnitude}");
			var color = Local.Gradient.Value.GetColor(sum.Magnitude);
			image[x,y] = color;
		});

		return true;
	}

	//according to wolfram alpha https://www.wolframalpha.com/input?i=1.2%2B2.5i+mod+1
	static Complex ModOne(Complex number)
	{
		var floor = new Complex(
			Math.Floor(number.Real),
			Math.Floor(number.Imaginary)
		);
		return number - floor;
	}

	static Complex Mod(Complex number, Complex mod)
	{
		var residual = number / mod;

		var floor = new Complex(
			Math.Floor(residual.Real),
			Math.Floor(residual.Imaginary)
		);

		return number - (mod * floor);
	}

	static Complex ModPowOne(Complex number, int power)
	{
		Complex res = Complex.One;

		for(int p = 0; p < power; p++) {
			res = ModOne(res * number);
		}
		return res;
	}

	// https://pressbooks.howardcc.edu/jrip3/chapter/equivalence-classes-of-complex-numbers-modulo-a-natural-number/
	static Complex Mod(Complex number, double mod)
	{
		//double real = (number.Real < 0 ? mod : 0.0) + (number.Real % mod);
		//double imag = (number.Imaginary < 0 ? mod : 0.0) + (number.Imaginary % mod);

		double real = number.Real % mod;
		double imag = number.Imaginary % mod;

		return new Complex(real, imag);
	}

	static Complex ModPow(Complex number, int power, double mod)
	{
		Complex res = Complex.One;

		for(int p = 0; p < power; p++) {
			res = Mod(res * number, mod);
		}
		return res;
	}

	// static (double,double) ModPow(double mag, double phase, int power, double mod)
	// {
	// 	if (power == 0) {
	// 		return (1.0,0.0);
	// 	}
	// 	if (power == 1) {
	// 		return (mag, phase);
	// 	}

	// 	var nphase = phase * power % (2 * Math.PI);
	// 	var nmag = mag;
		
	// 	for(int p = 0; p < power; p++) {
	// 		nmag = nmag * mag % mod;
	// 	}

	// 	return (nmag, nphase);

	// }

	// static Complex ModPow(Complex number, int power, double mod)
	// {
	// 	if (power == 0) {
	// 		return Complex.One;
	// 	}
	// 	if (power == 1) {
	// 		return number;
	// 	}

	// 	//multiplying complex numbers in polar is adding the phase and multiplying the magnitude
	// 	double phase = number.Phase * power % (2 * Math.PI); //phase already self mods
	// 	double mag = 1.0;
	// 	double mul = number.Magnitude;

	// 	for(int p = 0; p < power; p++) {
	// 		mag = mag * mul % mod;
	// 	}

	// 	// while(power > 0) {
	// 	// 	if (power % 2 == 1) {
	// 	// 		mag = mul * mag % mod;
	// 	// 	}
	// 	// 	mul = mul * mul % mod;
	// 	// 	power /= 2;
	// 	// }

	// 	return Complex.FromPolarCoordinates(mag,phase);
	// }

	public IOptions Core { get { return Local; } }
	public ILayers Layers { get { return Context.Layers; } }
	Options Local;
	IFunctionContext Context;

	// class ModMath
	// {
	// 	public ModMath(double mod)
	// 	{
	// 		Mod = mod;
	// 	}

	// 	public double Add(double a, double b)
	// 	{
	// 		return (a + b) % Mod;
	// 	}

	// 	public double Multiply(double a, double b)
	// 	{
	// 		return (a * b) % Mod;
	// 	}

	// 	public double Pow(double a, int power)
	// 	{
	// 		if (power == 0) { return 1.0; }
	// 		if (power == 1) { return a; }

	// 		double final = 1.0;
	// 		for(int p = 0; p < power; p++) {
	// 			final = (final * a) % Mod;
	// 		}
	// 		return final;
	// 	}

	// 	public double Mod { get; private set; }
	// }
}
