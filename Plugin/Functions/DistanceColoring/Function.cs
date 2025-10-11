using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using System.Drawing;
using System.Text.RegularExpressions;

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
		Context.Options.Usage(sb, Context.Register);
	}

	public bool Run(string[] args)
	{
		if(Context.Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!Context.Options.ParseArgs(args, Context.Register)) {
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

		var rnd = new Random(); //TODO add option for seed
		var runner = new RandomPlacement {
			CanTryPlace = CanTryPlace,
			Image = image,
			Progress = Context.Progress,
			Rnd = rnd
		};
		runner.Run();

		List<ColorRGBA> palette = new();
		for(int y = 0; y < image.Height; y++) {
			for(int x = 0; x < image.Width; x++) {
				int pl = baseGroup[x, y];
				while(palette.Count <= pl) { //place can be out of order so using a while loop to fill
					palette.Add(rnd.RandomColor(1.0));
				}
				//if(pl == 1) {
				image[x, y] = palette[pl];
				//}
			}
		}

		return true;
	}

	class RandomPlacement
	{
		//#1 place = 1
		//#2 pick random point (don't remove)
		//#3 if can be placed - remove
		//#4 if can't be placed pick another point (repeat #2)
		//#5 remove placed point and place++ (repeat #1)

		void RemovePoint(int ix)
		{
			//speedup - swap and remove it from the end of the array
			(allPoints[^1], allPoints[ix]) = (allPoints[ix], allPoints[^1]);
			allPoints.RemoveAt(allPoints.Count - 1);
		}

		public void Run()
		{
			// keep track of points so we don't have to search for empty spots later
			allPoints = new(Image.Width * Image.Height);
			for(int y = 0; y < Image.Height; y++) {
				for(int x = 0; x < Image.Width; x++) {
					allPoints.Add(new Point(x, y));
				}
			}

			int total = Image.Width * Image.Height;
			int place = 0;  //place normally starts at 1 but we're making it an index
			int maxPlace = Math.Min(Image.Width, Image.Height) / 2;
			int MaxTries = maxPlace; //TODO make optional ?

			while(allPoints.Count > 0 && place <= maxPlace) {
				int tries = 0;
				while(tries < MaxTries) {
					var ix = Rnd.Next(allPoints.Count);
					var point = allPoints[ix];

					if(CanTryPlace(point, place)) {
						RemovePoint(ix);
						tries = 0;
					}
					else {
						tries++;
					}
				}
				place++;
				Progress.Report(1.0 - ((double)allPoints.Count / total));
			}
		}

		List<Point> allPoints;
		public ICanvas Image;
		public Func<Point, int, bool> CanTryPlace;
		public Rasberry.Cli.IProgressWithLabel<double> Progress;
		public Random Rnd;
	}

	Options Local;
	IFunctionContext Context;
	public IOptions Options { get { return Context.Options; } }
}
