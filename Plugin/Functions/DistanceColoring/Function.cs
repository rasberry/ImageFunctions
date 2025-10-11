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

		// keep track of points so we don't have to search for empty spots later
		List<Point> allPoints = new(image.Width * image.Height);
		for(int y = 0; y < image.Height; y++) {
			for(int x = 0; x < image.Width; x++) {
				allPoints.Add(new Point(x, y));
			}
		}

		var Rnd = new Random(); //TODO add option for seed
		var baseGroup = new int[image.Width, image.Height];
		List<bool[,]> groups = new();

		//pick random point
		//place = 1
		//is there a place within place distance + 1 ?
		//if yes : place ++ and repeat
		//if no : add place to location

		int total = image.Width * image.Height;
		while(allPoints.Count > 0) {
			var ix = Rnd.Next(allPoints.Count);
			//speedup - swap and remove it from the end of the array
			var point = allPoints[ix];
			(allPoints[^1], allPoints[ix]) = (allPoints[ix], allPoints[^1]);
			allPoints.RemoveAt(allPoints.Count - 1);

			int place = 0; //place normally starts at 1 but we're making it an index
			while(place < 20) {
				//make sure there's a group for every place
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
				//Context.Log.Debug($"px={px} py={py} mx={mx} my={my} X={point.X} Y={point.Y} p={place}");
				bool hH = plane[px, py];
				if(px > 0) { hL = plane[px - 1, py]; }
				if(px < mx) { hR = plane[px + 1, py]; }
				if(py > 0) { hT = plane[px, py - 1]; }
				if(py < my) { hB = plane[px, py + 1]; }

				if(!hH && !hL && !hR && !hT && !hB) {
					baseGroup[point.X, point.Y] = place + 1;
					plane[px, py] = true;
					break;
				}

				place++;

				// //make sure there's a group for every place
				// if(groups.Count <= place) {
				// 	var tree = new KdTree.KdTree<int, int>(2, new TaxiMath());
				// 	groups.Add(tree);
				// }
				// var treePlace = groups[place];

				// var nearSet = treePlace.GetNearestNeighbours([point.X, point.Y], 1);
				// done = true;
				// if(nearSet.Length > 0) {
				// 	var near = nearSet[0];
				// 	var dist = TaxiMath.TaxiDistance([point.X, point.Y], near.Point);
				// 	if(dist - 1 > place) { //place is an index so take one off of dist
				// 		place++;
				// 		done = false; //can't place it so continue
				// 	}
				// }

				// if(done) {
				// 	treePlace.Add([point.X, point.Y], place);
				// 	if(palette.Count <= place) {
				// 		palette.Add(Rnd.RandomColor(1.0));
				// 	}
				// 	image[point.X, point.Y] = palette[place];
				// }
			}

			Context.Progress.Label = $"G:{groups.Count} P:{place} ";
			Context.Progress.Report(1.0 - ((double)allPoints.Count / total));
		}

		List<ColorRGBA> palette = new();
		for(int y = 0; y < image.Height; y++) {
			for(int x = 0; x < image.Width; x++) {
				int place = baseGroup[x, y];
				while(palette.Count <= place) { //place can be out of order so using a while loop to fill
					palette.Add(Rnd.RandomColor(1.0));
				}
				//if(place == 0) {
					image[x, y] = palette[place];
				//}
			}
		}

		return true;
	}

	// class TaxiMath : KdTree.Math.TypeMath<int>
	// {
	// 	public override int MinValue => int.MinValue;
	// 	public override int MaxValue => int.MaxValue;
	// 	public override int Zero => 0;
	// 	public override int NegativeInfinity => MinValue;
	// 	public override int PositiveInfinity => MaxValue;

	// 	public override int Add(int a, int b) => a + b;
	// 	public override bool AreEqual(int a, int b) => a == b;
	// 	public override int Compare(int a, int b) => a.CompareTo(b);
	// 	public override int Subtract(int a, int b) => a - b;
	// 	//Multiply is used to square the radius so we're just going to return the original value
	// 	// so that it's comparing distance to distance
	// 	public override int Multiply(int a, int b) => a;

	// 	public static int TaxiDistance(int[] a, int[] b)
	// 	{
	// 		int num = 0;
	// 		int len = a.Length;
	// 		for(int i = 0; i < len; i++) {
	// 			int diff = Math.Abs(a[i] - b[i]);
	// 			num += diff;
	// 		}

	// 		return num;
	// 	}

	// 	public override int DistanceSquaredBetweenPoints(int[] a, int[] b)
	// 	{
	// 		return TaxiDistance(a, b);
	// 	}
	// }

	Options Local;
	IFunctionContext Context;
	public IOptions Options { get { return Context.Options; } }
}
