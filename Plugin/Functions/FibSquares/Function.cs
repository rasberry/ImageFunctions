using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using System.Drawing;
using CoreColors = ImageFunctions.Core.Aides.ColorAide;
using PlugMath = ImageFunctions.Plugin.Aides.MathAide;

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

		var bounds = image.Bounds();
		if (O.Sweep == FibSquares.Options.SweepKind.Split) {
			var (one,two) = SplitSquare(bounds);
			DrawSpiral(image, one);
			DrawSpiral(image, two);
		}
		else if (O.Sweep == FibSquares.Options.SweepKind.Resize) {
			var resized = GetOptimalRatio(bounds);
			DrawSpiral(image, resized);
		}
		else { //SweepKind.Nothing
			DrawSpiral(image, bounds);
		}

		return true;
	}

	Rectangle GetOptimalRatio(Rectangle bounds)
	{
		int a = (int)Math.Round(bounds.Height / Phi - bounds.Width); //close ratio adjust width
		int b = (int)Math.Round(bounds.Width / Phi - bounds.Height); //close ratio adjust height
		int c = (int)Math.Round(bounds.Height * Phi - bounds.Width); //far ratio adjust width
		int d = (int)Math.Round(bounds.Width * Phi - bounds.Height); //far ratio adjust height

		//pick the least negative value below zero
		//only have 4 values to check so doing this manually
		int z = int.MinValue, which = -1;
		if (a <= 0 && a > z) { z = a; which = 0; }
		if (b <= 0 && b > z) { z = b; which = 1; }
		if (c <= 0 && c > z) { z = c; which = 2; }
		if (d <= 0 && d > z) { z = d; which = 3; }

		//Log.Debug($"{nameof(GetOptimalRatio)} {a} {b} {c} {d} [{z}]");
		switch(which) {
		case 0: return new Rectangle(bounds.Left, bounds.Top, bounds.Width + z, bounds.Height);
		case 1: return new Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height + z);
		case 2: return new Rectangle(bounds.Left, bounds.Top, bounds.Width + z, bounds.Height);
		case 3: return new Rectangle(bounds.Left, bounds.Top, bounds.Width, bounds.Height + z);
		}
		return bounds;
	}

	void DrawSpiral(ICanvas image, Rectangle bounds)
	{
		//calculate boxes and remainders (splits)
		List<(Rectangle Square,Rectangle Split)> shapeList = new();
		var area = bounds;
		int seq = 0;
		
		while(true) {
			var (square,split) = SplitRectangle(area, seq++);
			if (split.Width <= 0 || split.Height <= 0) { break; }
			shapeList.Add((square,split));
			area = split;
		}

		//pre-select colors since gradients need two colors each iteration
		List<ColorRGBA> colorList = new(shapeList.Count + 1);
		for(int c = 0; c < shapeList.Count + 1; c++) {
			colorList.Add(RandomColor());
		}

		switch(O.DraMode) {
		case FibSquares.Options.DrawModeKind.Plain:
			DrawSimpleBoxes(image, shapeList, colorList); break;
		case FibSquares.Options.DrawModeKind.Gradient:
			DrawGradients(image, shapeList, colorList); break;
		case FibSquares.Options.DrawModeKind.Drag:
			DrawDrag(image, shapeList, colorList); break;
		//case FibSquares.Options.DrawModeKind.LineDrag:
		//	DrawLinePath(image, shapeList, colorList); break;

		}

		if (O.DrawBorders) {
			for(int c = 0; c < shapeList.Count - 1; c++) {
				var b = shapeList[c].Square;
				DrawBorder(image,b,colorList[c]);
			}
		}
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
		//Log.Debug($"{nameof(SplitRectangle)} b={bounds} q={square} s={split}");
		return (square,split);
	}

	(Rectangle One, Rectangle Two) SplitSquare(Rectangle bounds)
	{
		var values = Enum.GetValues<Direction>();
		var dir = (Direction)values.GetValue(Rnd.Next(values.Length));
		Rectangle one, two;
		double len = dir == Direction.Up || dir == Direction.Down ? bounds.Height : bounds.Width;
		double big = len * Phi / (Phi + 1);
		double sma = len / (Phi + 1);

		switch(dir) {
		case Direction.Up: default:
			one = new Rectangle(bounds.Left, bounds.Top, bounds.Width, (int)sma);
			two = new Rectangle(bounds.Left, (int)sma, bounds.Width, bounds.Height - (int)sma);
			break;
		case Direction.Down:
			one = new Rectangle(bounds.Left, bounds.Top, bounds.Width, (int)big);
			two = new Rectangle(bounds.Left, (int)big, bounds.Width, bounds.Height - (int)big);
			break;
		case Direction.Left:
			one = new Rectangle(bounds.Left, bounds.Top, (int)sma, bounds.Height);
			two = new Rectangle((int)sma, bounds.Top, bounds.Width - (int)sma, bounds.Height);
			break;
		case Direction.Right:
			one = new Rectangle(bounds.Left, bounds.Top, (int)big, bounds.Height);
			two = new Rectangle((int)big, bounds.Top, bounds.Width - (int)big, bounds.Height);
			break;
		}
		return (one,two);
	}

	void DrawSimpleBoxes(ICanvas image, List<(Rectangle Square,Rectangle Split)> list, List<ColorRGBA> colorList)
	{
		int c = 0;
		foreach(var item in list) {
			var color = RandomColor();
			DrawArea(image, item.Square, colorList[c++]);
		}
		DrawArea(image, list[^1].Split, colorList[c++]);

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

			// Direction? d = null;
			// int steps = int.MinValue;
			// if (dl > steps) { steps = dl; d = Direction.Left; }
			// if (dt > steps) { steps = dt; d = Direction.Up; }
			// if (dr > steps) { steps = dr; d = Direction.Right; }
			// if (db > steps) { steps = db; d = Direction.Down; }

			int steps = Plugin.Aides.MathAide.Max(dl, dt, dr, db);
			//Log.Debug($"steps={steps} beg={beg} end={end}");

			for(int s = 0; s < steps; s++) {
				double ratio = s / (double)steps;
				double l = PlugMath.Between(beg.Left, end.Left, ratio);
				double t = PlugMath.Between(beg.Top, end.Top, ratio);
				double r = PlugMath.Between(beg.Right, end.Right, ratio);
				double b = PlugMath.Between(beg.Bottom, end.Bottom, ratio);
				var color = CoreColors.BetweenColor(cb, ce, ratio);

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

	//TODO needs work
	void DrawLinePath(ICanvas image, List<(Rectangle Square,Rectangle Split)> list, List<ColorRGBA> colorList)
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

			Direction d = Direction.Up;
			int steps = int.MinValue;
			if (dl > steps) { steps = dl; d = Direction.Left; }
			if (dt > steps) { steps = dt; d = Direction.Up; }
			if (dr > steps) { steps = dr; d = Direction.Right; }
			if (db > steps) { steps = db; d = Direction.Down; }
			Log.Debug($"d={d}");

			for(int s = 0; s < steps; s++) {
				double ratio = s / (double)steps;
				var color = CoreColors.BetweenColor(cb, ce, ratio);

				double l = PlugMath.Between(beg.Left, end.Left, ratio);
				double t = PlugMath.Between(beg.Top, end.Top, ratio);
				double r = PlugMath.Between(beg.Right, end.Right, ratio);
				double b = PlugMath.Between(beg.Bottom, end.Bottom, ratio);

				int x = (int)Math.Round(l + Math.Abs(l - r) / 2.0);
				int y = (int)Math.Round(t + Math.Abs(t - b) / 2.0);
				image[x,y] = color;
				//Log.Debug($"[{x},{y}] b={beg} e={end}");
			}

			//Point bp1 = Point.Empty, bp2 = Point.Empty;
			//Point ep1 = Point.Empty, ep2 = Point.Empty;

			// switch(d) {
			// case Direction.Right:
			// 	bp1 = new Point(beg.Left, beg.Top); bp2 = new Point(beg.Left, beg.Bottom - 1);
			// 	ep1 = new Point(end.Left, end.Top); ep2 = new Point(end.Left, end.Bottom - 1);
			// 	break;
			// case Direction.Left:
			// 	bp1 = new Point(beg.Right - 1, beg.Top); bp2 = new Point(beg.Right - 1, beg.Bottom - 1);
			// 	ep1 = new Point(end.Right - 1, end.Top); ep2 = new Point(end.Right - 1, end.Bottom - 1);
			// 	break;
			// case Direction.Up:
			// 	bp1 = new Point(beg.Left, beg.Bottom - 1); bp2 = new Point(beg.Right - 1, beg.Bottom - 1);
			// 	ep1 = new Point(end.Left, end.Bottom - 1); ep2 = new Point(end.Right - 1, end.Bottom - 1);
			// 	break;
			// case Direction.Down:
			// 	bp1 = new Point(beg.Left, beg.Top); bp2 = new Point(beg.Right - 1, beg.Top);
			// 	ep1 = new Point(end.Left, end.Top); ep2 = new Point(end.Right - 1, end.Top);
			// 	break;
			// }

			// //Log.Debug($"{nameof(DrawLinePath)} d={d} steps={steps} p1={p1} p2={p2}");
			// for(int s = 0; s < steps; s++) {
			// 	double ratio = s / (double)steps;
			// 	var color = ColorAide.BetweenColor(cb, ce, ratio);

			// 	if (d == Direction.Up || d == Direction.Down) {
			// 		int x = (int)Math.Round(Plugin.Aides.MathAide.Between(bp1.X, ep1.X, ratio));
			// 		int by = (int)Math.Round(Plugin.Aides.MathAide.Between(bp1.Y, bp2.Y, ratio));
			// 		int ey = (int)Math.Round(Plugin.Aides.MathAide.Between(ep1.Y, ep2.Y, ratio));

			// 		int dy = Math.Abs(by - ey);
			// 		// Log.Debug($"x={x} dby={by} ey={ey} dy={dy}");
			// 		//for(int i = 0; i < dy; i++) {
			// 			//int y = (int)Math.Round(Plugin.Aides.MathAide.Between(by, ey, (double)i / dy));
			// 			int y = (int)Math.Round(Plugin.Aides.MathAide.Between(by, ey, ratio));
			// 			image[x,y] = color;
			// 		//}
			// 	}
			// 	else {
			// 		int y = (int)Math.Round(Plugin.Aides.MathAide.Between(bp1.Y, ep1.Y, ratio));
			// 		int bx = (int)Math.Round(Plugin.Aides.MathAide.Between(bp1.X, bp2.X, ratio));
			// 		int ex = (int)Math.Round(Plugin.Aides.MathAide.Between(ep1.X, ep2.X, ratio));

			// 		int dx = Math.Abs(bx - ex);
			// 		// Log.Debug($"x={x} dby={by} ey={ey} dy={dy}");
			// 		//for(int i = 0; i < dx; i++) {
			// 			//int x = (int)Math.Round(Plugin.Aides.MathAide.Between(bx, ex, (double)i / dx));
			// 			int x = (int)Math.Round(Plugin.Aides.MathAide.Between(bx, ex, ratio));
			// 			image[x,y] = color;
			// 		//}
			// 	}
			//}

			//int steps = Plugin.Aides.MathAide.Max(dl, dt, dr, db);
			//Log.Debug($"steps={steps} beg={beg} end={end}");
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

			var color = CoreColors.BetweenColor(start, end, offset);
			canvas[px,py] = color;
		},CoreOptions.MaxDegreeOfParallelism);
		return true;
	}

	bool DrawBorder(ICanvas canvas, Rectangle rect, ColorRGBA color, Direction? d = null)
	{
		var inside = canvas.Bounds();
		inside.Intersect(rect);

		if (inside.Width < 2 || inside.Height < 2) {
			return false;
		}

		//Log.Debug($"{nameof(DrawBorder)} {rect} -> {inside}");
		if (d == null || d.Value == Direction.Up || d.Value == Direction.Down) {
			for(int x = inside.Left; x < inside.Right; x++) {
				if (d == null || d.Value == Direction.Up) {
					canvas[x, inside.Top] = color;
				}
				if (d == null || d.Value == Direction.Down) {
					canvas[x, inside.Bottom - 1] = color;
				}
			}
		}
		if (d == null || d.Value == Direction.Left || d.Value == Direction.Right) {
			for(int y = inside.Top; y < inside.Bottom; y++) {
				if (d == null || d.Value == Direction.Left) {
					canvas[inside.Left, y] = color;
				}
				if (d == null || d.Value == Direction.Right) {
					canvas[inside.Right - 1, y] = color;
				}
			}
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

	enum Direction { Up = 0, Down = 1, Left, Right }
	const double Phi = 1.618033988749895; //(Math.Sqrt(5) + 1) / 2;
	readonly Options O = new();
	public IOptions Options { get { return O; }}

	IRegister Register;
	ILayers Layers;
	ICoreOptions CoreOptions;
	Random Rnd;
}