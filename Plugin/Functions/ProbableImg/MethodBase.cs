using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;
using System.Collections;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.ProbableImg;

//using this to consolidate the similarities between methods one and two
abstract class MethodBase
{
	protected abstract void UpdateCounts(ColorRGBA oc, ICanvas frame, (ColorRGBA?, ColorRGBA?, ColorRGBA?, ColorRGBA?) fourSides);
	protected abstract void PickAndVisitFour(ICanvas img, int x, int y, List<(int, int)> pixStack);
	protected abstract void SetStartColor(ICanvas img, int sx, int sy);

	public Options O { get; set; }
	public ICoreLog Log { get; set; }

	public void CreateProfile(ProgressBar pbar, ICanvas frame, Rectangle rect)
	{
		pbar.Prefix = "Creating Profile ";

		rect.ThreadPixels((x, y) => {
			int cy = y - rect.Top;
			int cx = x - rect.Left;
			var oc = frame[cx, cy];

			var fourSides = GetSurroundColors(frame, cx, cy);
			UpdateCounts(oc, frame, fourSides);

			//The above code is not thread-safe so forcing max concurrency to one instead of  Core.MaxDegreeOfParallelism
		}, 1, pbar);
	}

	protected void UpdateCountsBase<T>(T ix, ICanvas frame, Dictionary<T, ColorProfile<T>> profile,
		(ColorRGBA?, ColorRGBA?, ColorRGBA?, ColorRGBA?) fourSides, Action<Dictionary<T, long>, ColorRGBA> addUpdateMethod)
	{
		if(!profile.ContainsKey(ix)) {
			var ncp = new ColorProfile<T> {
				NColor = new(),
				WColor = new(),
				SColor = new(),
				EColor = new()
			};
			profile.TryAdd(ix, ncp);
		}

		var (cn, cw, cs, ce) = fourSides;

		var cc = profile[ix];
		if(cn != null) { addUpdateMethod(cc.NColor, cn.Value); }
		if(cw != null) { addUpdateMethod(cc.WColor, cw.Value); }
		if(cs != null) { addUpdateMethod(cc.SColor, cs.Value); }
		if(ce != null) { addUpdateMethod(cc.EColor, ce.Value); }
	}

	public void CreateImage(ProgressBar pbar, ICanvas img)
	{
		pbar.Prefix = "Generating Image ";
		int iw = img.Width, ih = img.Height;
		double totalPixels = iw * ih;
		double visitedPixels = 0; //to keep track of progress

		var pixStack = new List<(int, int)>();
		VisitNest = new BitArray[ih];
		for(int ii = 0; ii < ih; ii++) {
			VisitNest[ii] = new BitArray(iw, false);
		}

		Rnd = O.RandomSeed.HasValue
			? new Random(O.RandomSeed.Value)
			: new Random()
		;

		//figure out starting nodes
		int maxNodes = O.StartLoc.Count;
		if(O.TotalNodes != null) {
			maxNodes = O.TotalNodes.Value;
		}
		//we want at least one
		if(maxNodes < 1) { maxNodes = 1; }

		while(O.StartLoc.Count < maxNodes) {
			O.StartLoc.Add(StartPoint.FromLinear(
				Rnd.Next(iw),
				Rnd.Next(ih)
			));
		}

		foreach(var startp in O.StartLoc) {
			int sx = 0, sy = 0;
			if(!startp.IsLinear) {
				sx = (int)(iw * startp.PX);
				sy = (int)(ih * startp.PY);
			}
			else {
				sx = startp.LX;
				sy = startp.LY;
			}

			//make sure the point is actually inside the image
			if(sx < 0 || sy < 0 || sx >= iw || sy >= ih) {
				continue;
			}

			pixStack.Add((sx, sy));
			SetStartColor(img, sx, sy);
			DoVisit(sx, sy);
		}

		if(pixStack.Count < 1) {
			Log.Error("None of the positions given are within the image");
			return;
		}

		while(pixStack.Count > 0) {
			//Log.Debug($"stack count={pixStack.Count}");

			//doing grab and swap so we can use list as a 'random pop' stack
			int rix = Rnd.Next(pixStack.Count);
			var (x, y) = pixStack[rix];
			pixStack[rix] = pixStack[pixStack.Count - 1];
			pixStack.RemoveAt(pixStack.Count - 1);

			pbar.Report(++visitedPixels / totalPixels);
			PickAndVisitFour(img, x, y, pixStack);
		}
	}

	protected void PickAndVisit<T>(ICanvas img, List<(int, int)> stack, Dictionary<T, long> dict,
		int x, int y, Action<ICanvas, Dictionary<T, long>, int, int> setMethod)
	{
		if(IsVisited(x, y)) {
			//Log.Debug($"already visited {x},{y}");
			return;
		}

		setMethod(img, dict, x, y);
		DoVisit(x, y);
		stack.Add((x, y));
	}

	protected void PickAndVisitFour<T>(ICanvas img, int x, int y, List<(int, int)> pixStack,
		ColorProfile<T> profile, Action<ICanvas, Dictionary<T, long>, int, int> setMethod)
	{
		int maxy = img.Height - 1;
		int maxx = img.Width - 1;

		if(y > 0) { PickAndVisit(img, pixStack, profile.NColor, x, y - 1, setMethod); }
		if(x > 0) { PickAndVisit(img, pixStack, profile.WColor, x - 1, y, setMethod); }
		if(y < maxy) { PickAndVisit(img, pixStack, profile.SColor, x, y + 1, setMethod); }
		if(x < maxx) { PickAndVisit(img, pixStack, profile.EColor, x + 1, y, setMethod); }
	}

	protected Random Rnd;
	protected BitArray[] VisitNest;

	protected bool IsVisited(int x, int y)
	{
		var ba = VisitNest[y];
		return ba[x];
	}

	protected void DoVisit(int x, int y)
	{
		var ba = VisitNest[y];
		ba[x] = true;
	}

	(ColorRGBA?, ColorRGBA?, ColorRGBA?, ColorRGBA?) GetSurroundColors(ICanvas frame, int cx, int cy)
	{
		var maxx = frame.Width - 1;
		var maxy = frame.Height - 1;
		//Log.Debug($"mx={maxx} my={maxy} w={frame.Width} h={frame.Height} cx={cx} cy={cy}");

		ColorRGBA? cn = null, cw = null, cs = null, ce = null;
		if(cy > 0) { cn = frame[cx, cy - 1]; }
		if(cx > 0) { cw = frame[cx - 1, cy]; }
		if(cy < maxy) { cs = frame[cx, cy + 1]; }
		if(cx < maxx) { ce = frame[cx + 1, cy]; }

		return (cn, cw, cs, ce);
	}

	protected void FindNextBucket<T>(Dictionary<T, long> dict, ref T nc)
	{
		//find total number of buckets
		long total = 0;
		foreach(var kvp in dict) {
			total += kvp.Value;
		}

		long next = Rnd.RandomLong(total);
		//this assumes the same order is kept between dictionary loops
		foreach(var kvp in dict) {
			total -= kvp.Value;
			if(total < next) {
				nc = kvp.Key;
			}
		}
	}
}
