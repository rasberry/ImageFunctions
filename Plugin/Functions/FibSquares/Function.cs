using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.FibSquares;

[InternalRegisterFunction(nameof(FibSquares))]
public class Function : IFunction
{
	public static IFunction Create(IRegister register, ILayers layers, ICoreOptions core)
	{
		var f = new Function {
			Register = register,
			CoreOptions = core,
			Layers = layers
		};
		return f;
	}
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb, Register);
	}

	public bool Run(string[] args)
	{
		if (Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if (!Options.ParseArgs(args, Register)) {
			return false;
		}

		Rnd = O.Seed == null ? new Random() : new Random(O.Seed.Value);

		//since we're rendering pixels make a new layer each time
		var engine = CoreOptions.Engine.Item.Value;
		var (dfw, dfh) = CoreOptions.GetDefaultWidthHeight(FibSquares.Options.PhiWidth, FibSquares.Options.PhiHeight);
		var image = engine.NewCanvasFromLayersOrDefault(Layers, dfw, dfh);
		Layers.Push(image);

		//calculate boxes and remainders (splits)
		List<(Rectangle Square,Rectangle Split)> splitList = new();
		var area = image.Bounds();
		int seq = 0;
		while(true) {
			var (square,split) = SplitRectangle(area, seq++);
			// Log.Debug($"split = {split}");
			if (split.Width <= 0 || split.Height <= 0) { break; }
			splitList.Add((square,split));
			area = split;
		}

		//pre-select colors since gradients need two colors each iteration
		List<ColorRGBA> colorList = new(splitList.Count + 1);
		for(int c = 0; c < splitList.Count + 1; c++) {
			colorList.Add(RandomColor());
		}

		switch(O.DraMode) {
		case FibSquares.Options.DrawModeKind.Plain:
			DrawSimpleBoxes(image, splitList, colorList); break;
		case FibSquares.Options.DrawModeKind.Gradient:
			DrawGradients(image, splitList, colorList); break;
		case FibSquares.Options.DrawModeKind.Drag:
			DrawDrag(image, splitList, colorList); break;
		}

		if (O.DrawBorders) {
			for(int c = 0; c < splitList.Count - 1; c++) {
				var b = splitList[c].Square;
				DrawBorder(image,b,colorList[c]);
			}
		}
		return true;
	}

	(Rectangle Sqaure, Rectangle Split) SplitRectangle(Rectangle bounds, int seq = 0)
	{
		bool landscape = bounds.Width > bounds.Height;
		bool side = O.UseSpiralOrder ? ((seq++ % 4) & 2) != 0 : Rnd.RandomChoice();
		// Log.Debug($"{nameof(SplitRectangle)} side={side} land={landscape}");
		int L = bounds.Left;
		int T = bounds.Top;
		int W = bounds.Width;
		int H = bounds.Height;
		Rectangle square;
		Rectangle split;

		if (!landscape && !side) {
			split = new Rectangle(L, T + W, W, H - W);
			square = new Rectangle(L, T, W, W);
		}
		else if (landscape && !side) {
			split = new Rectangle(L + H, T, W - H, H);
			square = new Rectangle(L, T, H, H);
		}
		else if (!landscape && side) {
			split = new Rectangle(L, T, W, H - W);
			square = new Rectangle(L, T + H - W, W, W);
		}
		else { //landscape && side
			split = new Rectangle(L, T, W - H, H);
			square = new Rectangle(L + W - H, T, H, H);
		}
		return (square,split);
	}

	void DrawSimpleBoxes(ICanvas image, List<(Rectangle Square,Rectangle Split)> list, List<ColorRGBA> colorList)
	{
		int c = 0;
		DrawArea(image, image.Bounds(), colorList[c]);
		foreach(var item in list) {
			var color = RandomColor();
			DrawArea(image, item.Split, colorList[++c]);
		}
	}

	void DrawGradients(ICanvas image, List<(Rectangle Square,Rectangle Split)> list, List<ColorRGBA> colorList)
	{
		for(int c = 0; c < list.Count - 1; c++) {
			var c1 = colorList[c];
			var c2 = colorList[c + 1];
			var rect = list[c].Square;
			var next = list[c + 1].Square;
			
			int dx = Math.Abs(rect.X - next.X);
			int dy = Math.Abs(rect.Y - next.Y);
			Direction d;
			if (dx > dy && rect.Left < next.Left) {
				d = Direction.Right;
			}
			else if (dx > dy && rect.Left > next.Left) {
				d = Direction.Left;
			}
			else if (dy > dx && rect.Top < next.Top) {
				d = Direction.Down;
			}
			else {
				d = Direction.Up;
			}
			DrawLinearGradient(image, rect, c1, c2, d);
		}
	}

	void DrawDrag(ICanvas image, List<(Rectangle Square,Rectangle Split)> list, List<ColorRGBA> colorList)
	{
		for(int c = 0; c < list.Count - 1; c++) {
			var cb = colorList[c];
			var ce = colorList[c + 1];
			var beg = list[c].Square;
			var end = list[c + 1].Square;

			//calculate the steps
			int dl = Math.Abs(beg.Left - end.Left);
			int dt = Math.Abs(beg.Top - end.Top);
			int dr = Math.Abs(beg.Right - end.Right);
			int db = Math.Abs(beg.Bottom - end.Bottom);

			int steps = Plugin.Aides.MathAide.Max(dl, dt, dr, db);
			//Log.Debug($"steps={steps} beg={beg} end={end}");

			for(int s = 0; s < steps; s++) {
				double ratio = s / (double)steps;
				double l = Plugin.Aides.MathAide.Between(beg.Left, end.Left, ratio);
				double t = Plugin.Aides.MathAide.Between(beg.Top, end.Top, ratio);
				double r = Plugin.Aides.MathAide.Between(beg.Right, end.Right, ratio);
				double b = Plugin.Aides.MathAide.Between(beg.Bottom, end.Bottom, ratio);
				var color = ColorAide.BetweenColor(cb,ce, ratio);

				var rect = Rectangle.FromLTRB(
					(int)Math.Round(l),
					(int)Math.Round(t),
					(int)Math.Round(r),
					(int)Math.Round(b)
				);

				DrawBorder(image, rect, color);
			}
		}
	}

	bool DrawArea(ICanvas canvas, Rectangle rect, ColorRGBA color)
	{
		var inside = canvas.Bounds();
		inside.Intersect(rect);

		if (inside.Width <= 0 || inside.Height <= 0) {
			return false;
		}

		inside.ThreadPixels((px,py) => {
			canvas[px,py] = color;
		},CoreOptions.MaxDegreeOfParallelism);

		return true;
	}

	bool DrawLinearGradient(ICanvas canvas, Rectangle rect, ColorRGBA start, ColorRGBA end, Direction d)
	{
		//Log.Debug($"{nameof(DrawLinearGradient)} {rect} {d}");
		var inside = canvas.Bounds();
		inside.Intersect(rect);

		if (inside.Width <= 0 || inside.Height <= 0) {
			return false;
		}

		inside.ThreadPixels((px,py) => {
			double offset;
			if (d == Direction.Right) {
				offset = (px - inside.Left) / (double)inside.Width;
			}
			else if (d == Direction.Left) {
				offset = (inside.Right - px) / (double)inside.Width;
			}
			else if (d == Direction.Down) {
				offset = (py - inside.Top) / (double)inside.Height;
			}
			else { //Direction.Up
				offset = (inside.Bottom - py) / (double)inside.Height;
			}

			var color = ColorAide.BetweenColor(start, end, offset);
			canvas[px,py] = color;
		},CoreOptions.MaxDegreeOfParallelism);
		return true;
	}

	bool DrawBorder(ICanvas canvas, Rectangle rect, ColorRGBA color)
	{
		var inside = canvas.Bounds();
		inside.Intersect(rect);

		if (inside.Width < 2 || inside.Height < 2) {
			return false;
		}

		//Log.Debug($"{nameof(DrawBorder)} {rect} -> {inside}");
		for(int x = inside.Left; x < inside.Right; x++) {
			// Log.Debug($"set {canvas.Bounds()} [{x},{inside.Bottom - 1}]\t[{x},{inside.Top}]");
			canvas[x, inside.Top] = color;
			canvas[x, inside.Bottom - 1] = color;
		}
		for(int y = inside.Top; y < inside.Bottom; y++) {
			//Log.Debug($"set {canvas.Bounds()} [{inside.Right - 1},{y}]\t[{inside.Left},{y}]");
			canvas[inside.Left, y] = color;
			canvas[inside.Right - 1, y] = color;
		}
		return true;
	}

	// static int NextFib(int fib)
	// {
	// 	if (fib == 0) { return 1; }
	// 	if (fib == 1) { return 2; }
	// 	return (int)Math.Round(fib * Phi);
	// }

	ColorRGBA RandomColor()
	{
		//switching range to make 1.0 inclusive instead of 0.0
		double r = 1.0 - Rnd.NextDouble();
		double g = 1.0 - Rnd.NextDouble();
		double b = 1.0 - Rnd.NextDouble();
		return new ColorRGBA(r,g,b,1.0);
	}

	enum Direction { Up, Down, Left, Right }
	//const double Phi = 1.618033988749895; //(Math.Sqrt(5) + 1) / 2;
	readonly Options O = new();
	public IOptions Options { get { return O; }}

	IRegister Register;
	ILayers Layers;
	ICoreOptions CoreOptions;
	Random Rnd;
}