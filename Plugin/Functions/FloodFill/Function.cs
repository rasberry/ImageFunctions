using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using System.Drawing;

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

	public IOptions Options { get { return O; }}

	public bool Run(string[] args)
	{
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!O.ParseArgs(args, Register)) {
			return false;
		}
		//Log.Debug($"FillType:{Options.FillType} MapSecondLayer:{Options.MapSecondLayer} MapType:{Options.MapType} Similarity:{Options.Similarity}");

		if(Layers.Count < 1) {
			Log.Error(Note.LayerMustHaveAtLeast(1));
			return false;
		}

		ICanvas mapSource = null;
		if(O.MapSecondLayer) {
			if(Layers.Count < 2) {
				Log.Error(PlugNote.MapSeconLayerNeedsTwoLayers());
				return false;
			}
			else {
				var layerTwo = Layers.ElementAt(1);
				//Log.Debug($"layerTwo={layerTwo.Name}");
				mapSource = layerTwo.Canvas;
			}
		}

		if(O.FillType == FillMethodKind.DepthFirst) {
			Storage = new StackWrapper<(Point, ColorRGBA)>();
		}
		else {
			Storage = new QueueWrapper<(Point, ColorRGBA)>();
		}

		MaxDist = ImageComparer.Max(O.Metric?.Value);
		ICanvas surface;
		if(O.MakeNewLayer) {
			surface = Core.Engine.Item.Value.NewCanvasFromLayers(Layers);
		}
		else {
			surface = Layers.First().Canvas;
		}

		//TODO when using similarity we are adding points that then become the new datum colors
		//i don't think i want to do that so need to find a way to compare new colors against the original datums
		//might need to re-work this to flood fill one point at a time instead of all of them

		// add explicitly provided points
		if(O.StartPoints != null && O.StartPoints.Count > 0) {
			foreach(var p in O.StartPoints) {
				var c = surface[p.X, p.Y];
				Storage.Stow((p, c));
			}
		}

		//find and add replaceColor pixel coordinates
		if(O.ReplaceColor.HasValue) {
			surface.ThreadPixels((x, y) => {
				var c = surface[x, y];
				if(IsSimilar(O.ReplaceColor.Value, c)) {
					Storage.Stow((new Point(x, y), c));
				}
			});
		}

		if(Storage.Count < 1) {
			throw PlugSqueal.MustProvideAtLeast("starting point", 1);
		}

		//need to keep track of points we've visited to stop loops from happening
		// I think this only would happen if the fill color is too similar to the source color
		// but seems worth doing anyways
		var visited = new HashSet<Point>();
		long iteration = 0;

		//main loop. basically go until we run out of discovered points
		// the color taken  is assumed to be what we are trying to replace
		// there could be multiple different colors in flight if more than
		// one starting point was provided
		while(Storage.Count > 0) {
			var (p, color) = Storage.Take();
			if(visited.Contains(p)) { continue; }

			visited.Add(p);
			var c = surface[p.X, p.Y];
			//Log.Debug($"visited = {visited.Count} p={p.X},{p.Y}");

			foreach(var sp in GetNearbyPoints(surface, p)) {
				if(IsSimilar(c, color)) {
					Storage.Stow((sp, color));
				}
			}

			if(O.MapSecondLayer) {
				surface[p.X, p.Y] = MapPixel(mapSource, p.X, p.Y, iteration);
			}
			else {
				surface[p.X, p.Y] = O.FillColor;
			}

			iteration++;
		}

		if(!O.MakeNewLayer && O.MapSecondLayer) {
			//remove second layer since we should only be left with one layer
			Layers.DisposeAt(1);
		}

		return true;
	}

	//similarity comparison defining similarity as:
	// 0.0 - not similar at all
	// 1.0 - completely similar (identical)
	bool IsSimilar(ColorRGBA pick, ColorRGBA sample)
	{
		var dist = ImageComparer.ColorDistance(pick, sample, O.Metric?.Value);
		var amount = Math.Clamp(1.0 - dist.Total / MaxDist, 0.0, 1.0);
		bool isSimilar = amount >= O.Similarity;
		return isSimilar;
	}

	// just doing the orthoganal points for now.
	static IEnumerable<Point> GetNearbyPoints(ICanvas canvas, Point p)
	{
		int x = canvas.Width - 1;
		int y = canvas.Height - 1;
		if(p.Y > 0) { yield return new Point(p.X, p.Y - 1); }
		if(p.Y < y) { yield return new Point(p.X, p.Y + 1); }
		if(p.X > 0) { yield return new Point(p.X - 1, p.Y); }
		if(p.X < x) { yield return new Point(p.X + 1, p.Y); }
	}

	//dest is the final layer, source is the layer being mapped from
	ColorRGBA MapPixel(ICanvas source, int x, int y, long pos)
	{
		switch(O.MapType) {
		case PixelMapKind.Horizontal: {
			long spos = pos % ((long)source.Width * source.Height);
			int sx = (int)(spos % source.Width);
			int sy = (int)(spos / source.Width);
			return source[sx, sy];
		}
		case PixelMapKind.Vertical: {
			long spos = pos % ((long)source.Width * source.Height);
			int sx = (int)(spos / source.Height);
			int sy = (int)(spos % source.Height);
			return source[sx, sy];
		}
		case PixelMapKind.Coordinate: {
			int sx = x % source.Width;
			int sy = y % source.Height;
			return source[sx, sy];
		}
		case PixelMapKind.Random: {
			int sx = O.Rnd.Next(source.Width);
			int sy = O.Rnd.Next(source.Height);
			return source[sx, sy];
		}
		}
		throw Squeal.InvalidArgument("-m");
	}

	readonly Options O = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions Core;
	IStowTakeStore<(Point, ColorRGBA)> Storage;
	double MaxDist;
}
