using System.Drawing;
using ImageFunctions.Core;

namespace ImageFunctions.Plugin.Functions.FloodFill;

[InternalRegisterFunction(nameof(FloodFill))]
public class Function : IFunction
{
	public static IFunction Create(IRegister register, ILayers layers, ICoreOptions core)
	{
		var f = new Function {
			Register = register,
			Core = core,
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
			Log.Debug($"FillType:{Options.FillType} MapSecondLayer:{Options.MapSecondLayer} MapType:{Options.MapType} Similarity:{Options.Similarity}");
			return false;
		}

		if (Layers.Count < 1) {
			Log.Error(Note.LayerMustHaveAtLeast(1));
			return false;
		}

		ICanvas mapSource = null;
		if (Options.MapSecondLayer && Layers.Count < 2) {
			Log.Error(PlugNote.MapSeconLayerNeedsTwoLayers());
			return false;
		}
		else {
			mapSource = Layers.ElementAt(1).Canvas;
		}

		if (Options.FillType == FillMethodKind.DepthFirst) {
			Storage = new StackWrapper<Point>();
		} else {
			Storage = new QueueWrapper<Point>();
		}

		// add explicitly provided points
		if (Options.StartPoints != null && Options.StartPoints.Count > 0) {
			foreach(var p in Options.StartPoints) {
				Storage.Stow(p);
			}
		}

		MaxDist = ImageComparer.Max(Options.Metric?.Value);
		var surface = Layers.First().Canvas;

		//find and add replaceColor pixel coordinates
		if (Options.ReplaceColor.HasValue) {
			Tools.ThreadPixels(surface,(x,y) => {
				var c = surface[x,y];
				if (IsSimilar(Options.ReplaceColor.Value,c)) {
					Storage.Stow(new Point(x,y));
				}
			});
		}

		if (Storage.Count < 1) {
			throw PlugSqueal.MustProvideAtLeast("starting point",1);
		}

		//need to keep track of points we've visited to stop loops from happening
		// I think this only would happen if the fill color is too similar to the source color
		// but seems worth doing anyways
		var visited = new HashSet<Point>();

		//main loop. basically go until we run out of discovered points
		// the color taken  is assumed to be what we are trying to replace
		// there could be multiple different colors in flight if more than
		// one starting point was provided
		while(Storage.Count > 0) {
			var p = Storage.Take();
			var c = surface[p.X,p.Y];
			visited.Add(p);

			foreach(var sp in GetNearbyPoints(surface,p)) {
				var sc = surface[sp.X,sp.Y];
				if (!visited.Contains(sp) && IsSimilar(c,sc)) {
					Storage.Stow(sp);
				}
			}

			if (Options.MapSecondLayer) {
				surface[p.X,p.Y] = MapPixel(surface, mapSource, p.X, p.Y);
			}
			else {
				surface[p.X,p.Y] = Options.FillColor;
			}
		}

		return true;
	}

	//similarity comparison defining similarity as:
	// 0.0 - not similar at all
	// 1.0 - completely similar (identical)
	bool IsSimilar(ColorRGBA pick, ColorRGBA sample)
	{
		var dist = ImageComparer.ColorDistance(pick,sample,Options.Metric?.Value);
		bool isSimilar = Math.Clamp(1.0 - dist.Total / MaxDist,0.0,1.0) >= Options.Similarity;
		return isSimilar;
	}

	// just doing the orthoganal points for now.
	static IEnumerable<Point> GetNearbyPoints(ICanvas canvas, Point p)
	{
		int x = canvas.Width - 1;
		int y = canvas.Height - 1;
		if (p.Y > 0) { yield return new Point(p.X, p.Y - 1); }
		if (p.Y < y) { yield return new Point(p.X, p.Y + 1); }
		if (p.X > 0) { yield return new Point(p.X - 1, p.Y); }
		if (p.X < x) { yield return new Point(p.X + 1, p.Y); }
	}

	//dest is the final layer, source is the layer being mapped from
	ColorRGBA MapPixel(ICanvas dest, ICanvas source, int x, int y)
	{
		switch(Options.MapType) {
			case PixelMapKind.Horizontal: {
				long spos = (dest.Width * y + x) % ((long)source.Width * source.Height);
				int sx = (int)(spos % source.Width);
				int sy = (int)(spos / source.Width);
				return source[sx,sy];
			}
			case PixelMapKind.Vertical: {
				long spos = (dest.Height * x + y) % ((long)source.Width * source.Height);
				int sx = (int)(spos / source.Height);
				int sy = (int)(spos % source.Height);
				return source[sx,sy];
			}
			case PixelMapKind.Coordinate: {
				int sx = x % source.Width;
				int sy = y % source.Height;
				return source[sx,sy];
			}
			case PixelMapKind.Random: {
				int sx = Options.Rnd.Next(source.Width);
				int sy = Options.Rnd.Next(source.Height);
				return source[sx,sy];
			}
		}
		throw Squeal.InvalidArgument("-m");
	}

	readonly Options Options = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions Core;
	IStowTakeStore<Point> Storage;
	double MaxDist;
}