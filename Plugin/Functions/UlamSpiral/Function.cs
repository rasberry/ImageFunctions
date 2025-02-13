using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.Metrics;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;
using System.Drawing;
using CoreColor = ImageFunctions.Core.Aides.ColorAide;
using PlugImage = ImageFunctions.Plugin.Aides.ImageAide;
using PlugMath = ImageFunctions.Plugin.Aides.MathAide;

namespace ImageFunctions.Plugin.Functions.UlamSpiral;

[InternalRegisterFunction(nameof(UlamSpiral))]
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
	ILayers Layers { get { return Context.Layers; } }
	IRegister Register { get { return Context.Register; } }
	ICoreOptions CoreOptions { get { return Context.Options; } }

	public bool Run(string[] args)
	{
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!O.ParseArgs(args, Register)) {
			return false;
		}

		var engine = CoreOptions.Engine.Item.Value;
		var (dfw, dfh) = CoreOptions.GetDefaultWidthHeight(UlamSpiral.Options.DefaultWidth, UlamSpiral.Options.DefaultHeight);
		var source = engine.NewCanvasFromLayersOrDefault(Layers, dfw, dfh);
		Layers.Push(source);
		var bounds = source.Bounds();

		Init(Register);
		PlugImage.FillWithColor(source, GetColor(PickColor.Back), bounds);

		var srect = GetSpacedRectangle(bounds);
		int maxFactor = O.ColorComposites ? FindMaxFactor(bounds) : 1;
		double factor = 1.0 / maxFactor;
		var drawFunc = GetDrawFunc();
		var (cx, cy) = GetCenterXY(srect);
		bool drawSlow = O.DotSize > O.Spacing; //turn off multithreading since threads might overlap
		var list = new List<(int, int, int)>(); //used to order dots by size
		bool drawPrimes = O.ColorPrimesForce || (!O.ColorComposites && !O.ColorPrimesBy6m);
		double primeFactor = O.ColorComposites ? 0.0 : 1.0;

		//using a closure is not my favorite way of doing this,
		// but easier than passing tons of arguments to a function
		Action<int, int> drawOne = (int x, int y) => {
			long num = MapXY(x, y, cx, cy, srect.Width);
			if(O.ColorComposites) {
				int count = Primes.CountFactors(num);
				if(drawSlow && count > O.Spacing) {
					list.Add((count, x, y));
				}
				else {
					DrawComposite(source, count * factor, x, y, drawFunc);
				}
			}
			//the next modes require the number to be prime
			if(!Primes.IsPrime(num)) { return; }

			if(O.ColorPrimesBy6m) {
				var (ism1, isp1) = Primes.IsPrime6m(num);
				var color = GetColor(PickColor.Back);
				if(ism1) { color = GetColor(PickColor.Prime); }
				if(isp1) { color = GetColor(PickColor.Prime2); }
				drawFunc(source, x, y, color, primeFactor);
			}
			//only one prime coloring mode is allowed
			else if(drawPrimes) {
				var color = GetColor(PickColor.Prime);
				drawFunc(source, x, y, color, primeFactor);
			}
		};

		using var pb2 = new ProgressBar() { Prefix = "Drawing " };

		if(drawSlow) {
			//have to keep track of progress manually
			double pbmax = srect.Width * srect.Height;
			double pbcount = 0;
			for(int y = srect.Top; y < srect.Bottom; y++) {
				for(int x = srect.Left; x < srect.Right; x++) {
					drawOne(x, y);
					pb2.Report(++pbcount / pbmax);
				}
			}

			//for slow composites need to draw leftovers
			if(O.ColorComposites) {
				//Log.Debug($"leftover count: {list.Count}");
				list.Sort((a, b) => a.Item1 - b.Item1);

				foreach(var item in list) {
					var (count, x, y) = item;
					DrawComposite(source, count * factor, x, y, drawFunc);
					pb2.Report(++pbcount / pbmax);
				}
			}
		}
		else {
			srect.ThreadPixels((x, y) => {
				drawOne(x, y);
			}, CoreOptions.MaxDegreeOfParallelism, pb2);
		}

		return true;
	}

	//a little messy, but didn't want the same code in two places
	void DrawComposite(ICanvas frame, double amount, int x, int y,
		Action<ICanvas, int, int, ColorRGBA, double> drawFunc)
	{
		var bg = GetColor(PickColor.Back);
		var fg = GetColor(PickColor.Comp);
		var color = CoreColor.BetweenColor(bg, fg, amount);
		drawFunc(frame, x, y, color, amount);
	}

	int FindMaxFactor(Rectangle srect)
	{
		//TODO surely there's a way to estimate the max factorcount so we don't have to actually find it
		//TODO yes there is! (i think) the most composite number is always the next lowest power of 2
		// to find it we could just zero all the bits after the most senior '1' bit
		// there's probably a very fast way to do that
		// int maxNum = MapXY(width-1, height-1, cx, cy, srect.Width);
		// int maxFactor = nearlestpowerof2(maxNum);
		// int count = log2(maxFactor);
		int maxFactor = int.MinValue;
		object maxLock = new object();
		var (cx, cy) = GetCenterXY(srect);
		using var pb1 = new ProgressBar() { Prefix = "Calculating " };

		srect.ThreadPixels((x, y) => {
			long num = MapXY(x, y, cx, cy, srect.Width);
			int count = Primes.CountFactors(num);
			if(count > maxFactor) {
				//the lock ensures we don't accidentally miss a larger value
				lock(maxLock) {
					if(count > maxFactor) { maxFactor = count; }
				}
			}
		}, CoreOptions.MaxDegreeOfParallelism, pb1);

		return maxFactor;
	}

	//TODO compare FindMaxFactor and this to see if they match
	// https://www.geeksforgeeks.org/highest-power-2-less-equal-given-number/
	static int HighestPower2(int n)
	{
		// check for the set bits
		n |= n >> 1;
		n |= n >> 2;
		n |= n >> 4;
		n |= n >> 8;
		n |= n >> 16;

		// Then we remove all but the top bit by xor'ing the
		// string of 1's with that string of 1's shifted one to
		// the left, and we end up with just the one top bit
		// followed by 0's.
		return n ^ (n >> 1);
	}

	(int, int) GetCenterXY(Rectangle rect)
	{
		int cx = -O.CenterX.GetValueOrDefault(0);
		int cy = -O.CenterY.GetValueOrDefault(0);
		if(O.Mapping == PickMapping.Spiral) {
			cx = (rect.Width / 2) - cx;
			cy = (rect.Height / 2) - cy;
		}
		return (cx, cy);
	}

	long MapXY(int x, int y, int cx, int cy, int w = 0)
	{
		x += 1; //offset to correct x coord
				//these are all 1+ since ulams spiral starts at 1 not 0
		switch(O.Mapping) {
		case PickMapping.Linear:
			return 1 + PlugMath.XYToLinear(x, y, w, cx, cy);
		case PickMapping.Diagonal:
			return 1 + PlugMath.XYToDiagonal(x, y, cx, cy);
		case PickMapping.Spiral:
			return 1 + PlugMath.XYToSpiralSquare(x, y, cx, cy);
		}
		return -1;
	}

	Rectangle GetSpacedRectangle(Rectangle rect)
	{
		Rectangle srect = new Rectangle(
			rect.X * O.Spacing,
			rect.Y * O.Spacing,
			rect.Width / O.Spacing,
			rect.Height / O.Spacing
		);
		return srect;
	}

	Action<ICanvas, int, int, ColorRGBA, double> GetDrawFunc()
	{
		if(IsCloseTo(O.DotSize, 1.0)) {
			return DrawDotPixel;
		}
		else {
			return DrawDotSpere;
		}
	}

	void DrawDotPixel(ICanvas frame, int x, int y, ColorRGBA color, double factor)
	{
		int s = O.Spacing;
		//scale up with spacing and center so we get a nice border
		x = x * s + s / 2; y = y * s + s / 2;
		frame[x, y] = color;
	}

	void DrawDotSpere(ICanvas frame, int x, int y, ColorRGBA color, double factor)
	{
		int s = O.Spacing;
		//circle size = max*f^2 (squared so it feels like a sphere)
		double d = O.DotSize * factor * factor;
		//scale up with spacing and center so we get a nice border
		x = x * s + s / 2; y = y * s + s / 2;

		if(d <= 1.0) {
			frame[x, y] = color;
			return;
		}

		var bounds = new Rectangle(0, 0, frame.Width, frame.Height);
		int d2 = (int)(d / 2);
		Rectangle r = new Rectangle(x - d2, y - d2, (int)d, (int)d);
		for(int dy = r.Top; dy < r.Bottom; dy++) {
			for(int dx = r.Left; dx < r.Right; dx++) {
				if(!bounds.Contains(dx, dy)) { continue; }

				switch(O.WhichDot) {
				case PickDot.Square: {
					frame[dx, dy] = color;
					break;
				}
				case PickDot.Circle: {
					double dist = Metric.Value.Measure(dx, dy, x, y);
					if(dist <= d2) { frame[dx, dy] = color; }
					break;
				}
				case PickDot.Blob: {
					double ratio = Metric.Value.Measure(dx, dy, x, y) * 2.0 / d;
					var ec = frame[dx, dy]; //merge with background
					var c = CoreColor.BetweenColor(color, ec, ratio);
					frame[dx, dy] = c;
					break;
				}
				}
			}
		}
	}

	readonly ColorRGBA[] c_color = new ColorRGBA[4];
	Lazy<IMetric> Metric;

	void Init(IRegister register)
	{
		var def = O.Color1.Value;
		c_color[0] = def;
		c_color[1] = O.Color2 ?? def;
		c_color[2] = O.Color3 ?? def;
		c_color[3] = O.Color4 ?? def;

		var mr = new MetricRegister(register);
		var m = mr.Get("Euclidean");
		Metric = m.Item;
	}

	ColorRGBA GetColor(PickColor pick)
	{
		return c_color[(int)pick - 1];
	}

	static bool IsCloseTo(double number, double check, double epsilon = double.Epsilon)
	{
		return Math.Abs(number - check) < epsilon;
	}
}
