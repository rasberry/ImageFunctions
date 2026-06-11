using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.TrimEdge;

[InternalRegisterFunction(nameof(TrimEdge))]
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

		if(Local.TrimType == Options.TrimKind.EdgeStripe) {
			DoEdgeStripe();
		}
		else if(Local.TrimType == Options.TrimKind.DwindleTrim) {
			throw new NotSupportedException(Options.TrimKind.DwindleTrim.ToString());
		}
		else {
			Squeal.NotSupported(Local.TrimType.ToString());
		}

		return true;
	}

	void DoEdgeStripe()
	{
		var orig = Layers.Pop();
		var canvas = orig.Canvas;
		var area = DetermineEdge(canvas);
		Context.Log.Debug($"T={area.Top} B={area.Bottom} L={area.Left} R={area.Right} W={area.Width} H={area.Height}");

		var final = Local.FillTransparent
			? FillTransparent(canvas, area)
			: CropCanvas(canvas, area)
		;
		if(Local.KeepOrigLayer) { Layers.Push(orig); }
		Layers.Push(final, orig.Name);
	}

	ICanvas CropCanvas(ICanvas src, Rectangle area)
	{
		var dest = Context.NewCanvas(area.Width, area.Height);
		dest.ThreadPixels(Context, (x, y) => {
			int sx = x + area.Left;
			int sy = y + area.Top;
			dest[x, y] = src[sx, sy];
		});
		return dest;
	}

	ICanvas FillTransparent(ICanvas src, Rectangle area)
	{
		var dest = Context.NewCanvas(src.Width, src.Height);
		dest.ThreadPixels(Context, (x, y) => {
			bool isInside = area.Contains(new Point(x, y));
			dest[x, y] = isInside ? src[x, y] : ColorAide.Transparent;
		});
		return dest;
	}

	Rectangle DetermineEdge(ICanvas canvas)
	{
		int top = 0, bottom = canvas.Height - 1,
			left = 0, right = canvas.Width - 1;

		int max = Math.Min(canvas.Width, canvas.Height) / 2;
		bool stopT = false, stopL = false, stopR = false, stopB = false;
		//TODO this can be made parallel
		for(int o = 0; o < max; o++) {
			if(!stopT && ShouldTrim(canvas, Side.Top, o)) { top = o; } else { stopT = true; }
			if(!stopL && ShouldTrim(canvas, Side.Left, o)) { left = o; } else { stopL = true; }
			if(!stopR && ShouldTrim(canvas, Side.Right, o)) { right = canvas.Width - o - 1; } else { stopR = true; }
			if(!stopB && ShouldTrim(canvas, Side.Bottom, o)) { bottom = canvas.Height - o - 1; } else { stopB = true; }
			//Context.Log.Debug($"o={o} T={top} B={bottom} L={left} R={right}");
			Context.Progress.Report((double)o / max);
		}

		var rect = Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
		return rect;
	}

	bool ShouldTrim(ICanvas canvas, Side side, int offset)
	{
		var stripe = GetStripe(canvas, side, offset);
		var avg = Local.BackColor != null
			? Local.BackColor.Value.Luma
			: GetStripeAverage(stripe);
		var dev = GetStripeDeviation(stripe, avg) * 2.0; //range 0.0 to 1.0
		Context.Log.Debug($"{offset}\t{side}\t{dev}\t{(dev <= Local.Fuzz ? "Y" : "n")}");
		return dev <= Local.Fuzz;
	}

	double GetStripeAverage(IEnumerable<ColorRGBA> stripe)
	{
		double luminosity = 0.0;
		int count = 0;

		foreach(var color in stripe) {
			count++;
			luminosity += color.Luma;
		}
		return luminosity / count;
	}

	//max deviation is 0.5
	double GetStripeDeviation(IEnumerable<ColorRGBA> stripe, double avg)
	{
		double variance = 0.0;
		int count = 0;
		foreach(var color in stripe) {
			count++;
			variance += Math.Pow(color.Luma - avg, 2);
		}
		return Math.Sqrt(variance / count);
	}

	IEnumerable<ColorRGBA> GetStripe(ICanvas canvas, Side side, int offset)
	{
		int indexFixed = side switch {
			Side.Bottom => canvas.Height - offset - 1,
			Side.Right => canvas.Width - offset - 1,
			_ => offset //top, left
		};

		bool isTopBottom = side == Side.Top || side == Side.Bottom;

		int indexEnd = isTopBottom
			? canvas.Width - 1
			: canvas.Height - 1
		;

		for(int i = 0; i <= indexEnd; i++) {
			yield return isTopBottom
				? canvas[i, indexFixed]
				: canvas[indexFixed, i]
			;
		}
	}

	enum Side
	{
		Top,
		Right,
		Bottom,
		Left
	}

	public IOptions Core { get { return Local; } }
	public ILayers Layers { get { return Context.Layers; } }
	Options Local;
	IFunctionContext Context;
}
