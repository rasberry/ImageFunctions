using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.PixelBinning;

[InternalRegisterFunction(nameof(PixelBinning))]
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
		if(Layers.Count < 1) {
			Context.Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}

		var canvas = Layers[0].Canvas;
		var rect = new Rectangle(
			0, 0,
			IntCeil(canvas.Width, Local.BinSize.Width),
			IntCeil(canvas.Height, Local.BinSize.Height)
		);

		var output = Local.ResizeLayer
			? Context.NewCanvas(rect.Width, rect.Height)
			: Context.NewCanvas(canvas.Width, canvas.Height)
		;

		CalcFunc selectedFunction = Local.PickCalc switch {
			PixelBinning.Options.Calculation.Add => CalcAdd,
			PixelBinning.Options.Calculation.Average => CalcAverage,
			PixelBinning.Options.Calculation.RMS => CalcRMS,
			PixelBinning.Options.Calculation.Min => CalcMin,
			PixelBinning.Options.Calculation.Max => CalcMax,
			_ => null
		};

		void CombineBins(int binX, int binY)
		{
			int startX = binX * Local.BinSize.Width;
			int startY = binY * Local.BinSize.Height;
			int endX = Math.Min(startX + Local.BinSize.Width, canvas.Width);
			int endY = Math.Min(startY + Local.BinSize.Height, canvas.Height);

			int count = Local.BinSize.Width * Local.BinSize.Height;
			double accR = 0.0, accG = 0.0, accB = 0.0, accA = 0.0, avgA = 0.0;

			if(Local.PickCalc == PixelBinning.Options.Calculation.Min) {
				accR = 1.0; accG = 1.0; accB = 1.0; accA = 1.0; avgA = 1.0;
			}

			for(int y = startY; y < endY; y++) {
				for(int x = startX; x < endX; x++) {
					var pix = canvas[x, y];
					selectedFunction(ref accR, ref accG, ref accB, ref accA, count, pix);
					avgA += pix.A / count;
				}
			}

			if(Local.PickCalc == PixelBinning.Options.Calculation.RMS) {
				accR = Math.Sqrt(accR);
				accG = Math.Sqrt(accG);
				accB = Math.Sqrt(accB);
				accA = Math.Sqrt(accA);
			}

			var replace = new ColorRGBA(
				accR, accG, accB,
				Local.IncludeAlpha ? accA : avgA
			);

			if(Local.ResizeLayer) {
				output[binX, binY] = replace;
			}
			else {
				for(int y = startY; y < endY; y++) {
					for(int x = startX; x < endX; x++) {
						output[x, y] = replace;
					}
				}
			}
		}

		rect.ThreadPixels(Context, CombineBins);

		if(!Local.MakeNewLayer) {
			Layers.Pop();
		}
		Layers.Push(output);

		return true;
	}

	void CalcAdd(ref double r, ref double g, ref double b, ref double a, int count, ColorRGBA color)
	{
		r += color.R;
		g += color.G;
		b += color.B;
		a += color.A;
	}

	void CalcAverage(ref double r, ref double g, ref double b, ref double a, int count, ColorRGBA color)
	{
		r += color.R / count;
		g += color.G / count;
		b += color.B / count;
		a += color.A / count;
	}

	void CalcRMS(ref double r, ref double g, ref double b, ref double a, int count, ColorRGBA color)
	{
		r += color.R * color.R / count;
		g += color.G * color.G / count;
		b += color.B * color.B / count;
		a += color.A * color.A / count;
	}

	void CalcMin(ref double r, ref double g, ref double b, ref double a, int count, ColorRGBA color)
	{
		if(color.R < r) { r = color.R; }
		if(color.G < g) { g = color.G; }
		if(color.B < b) { b = color.B; }
		if(color.A < a) { a = color.A; }
	}

	void CalcMax(ref double r, ref double g, ref double b, ref double a, int count, ColorRGBA color)
	{
		if(color.R > r) { r = color.R; }
		if(color.G > g) { g = color.G; }
		if(color.B > b) { b = color.B; }
		if(color.A > a) { a = color.A; }
	}

	// assumes positive numbers
	static int IntCeil(int num, int den)
	{
		return 1 + ((num - 1) / den);
	}

	delegate void CalcFunc(ref double r, ref double g, ref double b, ref double a, int count, ColorRGBA color);

	public IOptions Core { get { return Local; } }
	public ILayers Layers { get { return Context.Layers; } }
	Options Local;
	IFunctionContext Context;
}
