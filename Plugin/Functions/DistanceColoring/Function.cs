using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.DistanceColoring;

[InternalRegisterFunction(nameof(DistanceColoring))]
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
		Local.Usage(sb, Context.Register);
	}

	public bool Run(string[] args)
	{
		if(Context.Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!Local.ParseArgs(args, Context.Register)) {
			return false;
		}

		//since we're rendering pixels make a new layer each time
		var engine = Context.Options.Engine.Item.Value;
		var (dfw, dfh) = Context.Options.GetDefaultWidthHeight();
		var image = engine.NewCanvasFromLayersOrDefault(Context.Layers, dfw, dfh);
		Context.Layers.Push(image);

		var baseGroup = new int[image.Width, image.Height];
		List<bool[,]> groups = new();

		bool CanTryPlace(Point point, int place)
		{
			if(groups.Count <= place) {
				int w = image.Width / (place + 1);
				int h = image.Height / (place + 1);
				var g = new bool[w, h];
				//Context.Log.Debug($"add group {place} {w}x{h}");
				groups.Add(g);
			}

			//check the orthogonal locations for existing placements
			var plane = groups[place];
			int mx = plane.GetLength(0) - 1;
			int my = plane.GetLength(1) - 1;
			int px = Math.Clamp(point.X / (place + 1), 0, mx);
			int py = Math.Clamp(point.Y / (place + 1), 0, my);
			bool hL = false, hR = false, hT = false, hB = false;

			bool hH = plane[px, py];
			if(px > 0) { hL = plane[px - 1, py]; }
			if(px < mx) { hR = plane[px + 1, py]; }
			if(py > 0) { hT = plane[px, py - 1]; }
			if(py < my) { hB = plane[px, py + 1]; }

			//all tested location need to be empty (false)
			bool canPlace = !hH && !hL && !hR && !hT && !hB;
			//Context.Log.Debug($"C={canPlace} px={px} py={py} mx={mx} my={my} X={point.X} Y={point.Y} p={place}");
			if(canPlace) {
				baseGroup[point.X, point.Y] = place + 1;
				plane[px, py] = true;
			}
			return canPlace;
		}

		var rnd = Local.RandomSeed.HasValue ? new Random(Local.RandomSeed.Value) : new Random();
		Context.Log.Debug($"Seed = {Local.RandomSeed}");
		Context.Progress.Label = $"{Local.Kind}";

		IPointSource generator = Local.Kind switch {
			PlacementKind.Sequential => new SequentialSource(),
			PlacementKind.Spiral => new SpiralSource(),
			_ => new RandomSource()
		};
		generator.Image = image;
		generator.Rnd = rnd;

		//linked list has fast removals and we're running through the list in order
		LinkedList<Point> pointSource = new(generator);
		int total = pointSource.Count;

		int place = 0;  //place normally starts at 1 but we're making it an index
		int maxPlace = Math.Min(image.Width, image.Height) / 2;

		while(pointSource.Count > 0 && place <= maxPlace) {
			var node = pointSource.First;
			while(node != null) {
				var point = node.Value;

				if(CanTryPlace(point, place)) {
					var rem = node;
					node = node.Next;
					pointSource.Remove(rem);
				}
				else {
					node = node.Next;
				}
			}
			// Context.Log.Debug($"place={place} c={pointSource.Count}");
			place++;
			Context.Progress.Report((double)place / maxPlace);
		}

		List<ColorRGBA> palette = new();
		for(int y = 0; y < image.Height; y++) {
			for(int x = 0; x < image.Width; x++) {
				int pl = baseGroup[x, y];
				while(palette.Count <= pl) { //place can be out of order so using a while loop to fill
					palette.Add(rnd.RandomColor(1.0));
				}
				image[x, y] = palette[pl];
			}
		}

		return true;
	}

	Options Local;
	IFunctionContext Context;
	public IOptions Core { get { return Context.Options; } }
}
