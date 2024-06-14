using ImageFunctions.Core;

namespace ImageFunctions.Plugin.Functions.Derivatives;

[InternalRegisterFunction(nameof(Derivatives))]
public class Function : IFunction
{
	public static IFunction Create(IRegister register, ILayers layers, ICoreOptions core)
	{
		var f = new Function {
			Register = register,
			Layers = layers
			// Core = core - not used
		};
		return f;
	}

	public IOptions Options { get { return O; }}

	public bool Run(string[] args)
	{
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!O.ParseArgs(args, Register)) {
			return false;
		}

		if(Layers.Count < 1) {
			Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}

		var frame = Layers.First().Canvas;

		if(frame.Width < 2 || frame.Height < 2) {
			return true; //nothing to do
		}

		//using a queue to delay updates instead copying the image
		int qLength = 3 * frame.Width;
		var queue = new Queue<QueueItem>(qLength);
		QueueItem dqi;

		for(int y = 0; y < frame.Height; y++) {
			for(int x = 0; x < frame.Width; x++) {
				ColorRGBA? n = null, e = null, s = null, w = null;
				ColorRGBA c = frame[x, y];

				if(x > 0) { w = frame[x - 1, y]; }
				if(x < frame.Width - 1) { e = frame[x + 1, y]; }
				if(y > 0) { n = frame[x, y - 1]; }
				if(y < frame.Height - 1) { s = frame[x, y + 1]; }

				var color = DoDiff(c, n, e, s, w, O.UseABS);
				var qi = new QueueItem {
					X = x,
					Y = y,
					Color = O.DoGrayscale ? ToGrayScale(color) : color
				};

				if(queue.Count >= qLength) {
					dqi = queue.Dequeue();
					frame[dqi.X, dqi.Y] = dqi.Color;
				}
				queue.Enqueue(qi);
			}
		}

		while(queue.TryDequeue(out dqi)) {
			frame[dqi.X, dqi.Y] = dqi.Color;
		}

		return true;
	}

	static ColorRGBA DoDiff(ColorRGBA? src,
		ColorRGBA? n, ColorRGBA? e, ColorRGBA? s, ColorRGBA? w,
		bool abs)
	{
		if(!src.HasValue) { return PlugColors.Transparent; }
		var rgbaSrc = GetColor(src);
		var rgbaN = GetColor(n);
		var rgbaE = GetColor(e);
		var rgbaS = GetColor(s);
		var rgbaW = GetColor(w);

		double diffR = 0, diffG = 0, diffB = 0;
		int num = 0;

		if(n.HasValue) {
			diffR += DiffOne(abs, rgbaSrc.R, rgbaN.R);
			diffG += DiffOne(abs, rgbaSrc.G, rgbaN.G);
			diffB += DiffOne(abs, rgbaSrc.B, rgbaN.B);
			num++;
		}
		if(e.HasValue) {
			diffR += DiffOne(abs, rgbaSrc.R, rgbaE.R);
			diffG += DiffOne(abs, rgbaSrc.G, rgbaE.G);
			diffB += DiffOne(abs, rgbaSrc.B, rgbaE.B);
			num++;
		}
		if(s.HasValue) {
			diffR += DiffOne(abs, rgbaSrc.R, rgbaS.R);
			diffG += DiffOne(abs, rgbaSrc.G, rgbaS.G);
			diffB += DiffOne(abs, rgbaSrc.B, rgbaS.B);
			num++;
		}
		if(w.HasValue) {
			diffR += DiffOne(abs, rgbaSrc.R, rgbaW.R);
			diffG += DiffOne(abs, rgbaSrc.G, rgbaW.G);
			diffB += DiffOne(abs, rgbaSrc.B, rgbaW.B);
			num++;
		}
		double off = abs ? 0 : 0.5;
		if(abs) { num *= 2; }
		var pix = new ColorRGBA(
			diffR / num + off,
			diffG / num + off,
			diffB / num + off,
			rgbaSrc.A
		);
		return pix;
	}

	static double DiffOne(bool abs, double a, double b)
	{
		double tmp = a - b;
		return abs ? Math.Abs(tmp) : tmp;
	}

	static ColorRGBA GetColor(ColorRGBA? px)
	{
		if(px.HasValue) {
			return px.Value;
		}
		return PlugColors.Transparent;
	}

	struct QueueItem
	{
		public int X;
		public int Y;
		public ColorRGBA Color;
	}

	//TODO replace this with colorspace (maybe?)
	public static ColorRGBA ToGrayScale(ColorRGBA c)
	{
		double val = c.R * 0.2126 + c.G * 0.7152 + c.B * 0.0722;
		var vGray = new ColorRGBA(val, val, val, c.A);
		return vGray;
	}

	readonly Options O = new();
	ILayers Layers;
	IRegister Register;
}
