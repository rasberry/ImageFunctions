using System.Collections;
using System.Drawing;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.ProbableImg;

[InternalRegisterFunction(nameof(ProbableImg))]
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
		O.Usage(sb);
	}

	public bool Run( string[] args)
	{
		if (Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if (!O.ParseArgs(args, Register)) {
			return false;
		}

		if (Layers.Count < 1) {
			Tell.LayerMustHaveAtLeast();
			return false;
		}

		var source = Layers.First();
		var bounds = source.Bounds();

		using var progress = new ProgressBar();
		MethodBase m = O.UseNonLookup
			? new MethodTwo { O = O }
			: new MethodOne { O = O }
		;
		m.CreateProfile(progress,source,bounds);

		//foreach(var kvp in Profile) {
		//	Log.Debug($"Key = {kvp.Key}");
		//	Log.Debug(kvp.Value.ToString());
		//}

		var engine = Core.Engine.Item.Value;
		using var canvas = engine.NewCanvasFromLayers(Layers);
		m.CreateImage(progress,canvas);
		source.CopyFrom(canvas);

		return true;
	}

	readonly Options O = new();
	IRegister Register;
	ICoreOptions Core;
	ILayers Layers;
}

//TODO trying to consolidate the similarities between one and two
abstract class MethodBase
{
	public Options O { get; set; }
	public abstract void CreateProfile(ProgressBar pbar, ICanvas frame, Rectangle rect);
	public abstract void CreateImage(ProgressBar pbar, ICanvas img);

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
}

//This method uses extra dictionaries to map colors to indices (original code)
class MethodOne : MethodBase
{
	public override void CreateProfile(ProgressBar pbar, ICanvas frame, Rectangle rect)
	{
		Profile = new();
		CToIndex = new();
		IToColor = new();

		pbar.Prefix = "Creating Profile ";

		rect.ThreadPixels((x,y) => {
			int cy = y - rect.Top;
			int cx = x - rect.Left;
			var oc = frame[cx,cy];
			var maxx = frame.Width - 1;
			var maxy = frame.Height - 1;

			long ix = CtoI(oc);
			if (!Profile.ContainsKey(ix)) {
				var ncp = new ColorProfile<long> {
					NColor = new(), WColor = new(), SColor = new(), EColor = new()
				};
				Profile.TryAdd(ix,ncp);
			}

			//Log.Debug($"mx={maxx} my={maxy} w={frame.Width} h={frame.Height} cx={cx} cy={cy}");
			ColorRGBA? cn = null, cw = null, cs = null, ce = null;
			if (cy > 0)    { cn = frame[cx, cy - 1]; }
			if (cx > 0)    { cw = frame[cx - 1, cy]; }
			if (cy < maxy) { cs = frame[cx, cy + 1]; }
			if (cx < maxx) { ce = frame[cx + 1, cy]; }

			var cc = Profile[ix];
			if (cn != null) { AddUpdateCount(cc.NColor,cn.Value); }
			if (cw != null) { AddUpdateCount(cc.WColor,cw.Value); }
			if (cs != null) { AddUpdateCount(cc.SColor,cs.Value); }
			if (ce != null) { AddUpdateCount(cc.EColor,ce.Value); }

		//The above code is not thread-safe so forcing max concurrency to one instead of  Core.MaxDegreeOfParallelism
		},1,pbar);
	}

	void AddUpdateCount(IDictionary<long,long> dict, ColorRGBA color)
	{
		long ix = CtoI(color);
		if (dict.ContainsKey(ix)) {
			dict[ix]++;
		} else {
			dict.Add(ix,1);
		}
	}

	public override void CreateImage(ProgressBar pbar, ICanvas img)
	{
		pbar.Prefix = "Generating Image ";
		int iw = img.Width, ih = img.Height;
		double totalPixels = iw * ih;
		double visitedPixels = 0; //to keep track of progress

		var pixStack = new List<(int,int)>();
		VisitNest = new BitArray[ih];
		for(int ii = 0; ii < ih; ii++) {
			VisitNest[ii] = new BitArray(iw,false);
		}

		Rnd = O.RandomSeed.HasValue
			? new Random(O.RandomSeed.Value)
			: new Random()
		;

		//figure out starting nodes
		int maxNodes = O.StartLoc.Count;
		if (O.TotalNodes != null) {
			maxNodes = O.TotalNodes.Value;
		}
		//we want at least one
		if (maxNodes < 1) { maxNodes = 1; }

		while(O.StartLoc.Count < maxNodes) {
			O.StartLoc.Add(StartPoint.FromLinear(
				Rnd.Next(iw),
				Rnd.Next(ih)
			));
		}

		foreach(var startp in O.StartLoc) {
			int sx = 0,sy = 0;
			if (!startp.IsLinear) {
				sx = (int)(iw * startp.PX);
				sy = (int)(ih * startp.PY);
			}
			else {
				sx = startp.LX;
				sy = startp.LY;
			}

			//make sure the point is actually inside the image
			if (sx < 0 || sy < 0 || sx >= iw || sy >= ih) {
				continue;
			}

			pixStack.Add((sx,sy));
			long ix = PickStartColor();
			img[sx,sy] = ItoC(ix);
			DoVisit(sx,sy);
		}

		if (pixStack.Count < 1) {
			Log.Error("None of the positions given are within the image");
			return;
		}

		int maxy = img.Height - 1;
		int maxx = img.Width - 1;

		while(pixStack.Count > 0) {
			//Log.Debug($"stack count={pixStack.Count}");

			//doing grab and swap so we can use list as a 'random pop' stack
			int rix = Rnd.Next(pixStack.Count);
			var (x,y) = pixStack[rix];
			pixStack[rix] = pixStack[pixStack.Count - 1];
			pixStack.RemoveAt(pixStack.Count - 1);

			pbar.Report(++visitedPixels / totalPixels);
			long cx = CtoI(img[x,y]);
			var profile = Profile[cx];

			if (y > 0)    { PickAndVisit(img, pixStack, profile.NColor, x, y - 1); }
			if (x > 0)    { PickAndVisit(img, pixStack, profile.WColor, x - 1, y); }
			if (y < maxy) { PickAndVisit(img, pixStack, profile.SColor, x, y + 1); }
			if (x < maxx) { PickAndVisit(img, pixStack, profile.EColor, x + 1, y); }
		}
	}

	long PickStartColor()
	{
		long ix = Rnd.RandomLong(max: LastIndex);
		return ix;
	}

	void PickAndVisit(ICanvas img, List<(int,int)> stack, Dictionary<long,long> dict, int x, int y)
	{
		if (IsVisited(x,y)) {
			//Log.Debug($"already visited {x},{y}");
			return;
		}

		//find total number of buckets
		long total = 0;
		foreach(var kvp in dict) {
			total += kvp.Value;
		}

		long next = Rnd.RandomLong(total);
		long nc = 0;
		//this assumes the same order is kept between dictionary loops
		foreach(var kvp in dict) {
			total -= kvp.Value;
			if (total < next) {
				nc = kvp.Key;
			}
		}

		img[x,y] = ItoC(nc);
		DoVisit(x,y);
		stack.Add((x,y));
	}

	Dictionary<long,ColorProfile<long>> Profile;
	Dictionary<ColorRGBA,long> CToIndex;
	Dictionary<long,ColorRGBA> IToColor;
	long LastIndex = 0;

	public long CtoI(ColorRGBA c)
	{
		if (!CToIndex.TryGetValue(c,out long i)) {
			CToIndex[c] = LastIndex;
			IToColor[LastIndex] = c;
			LastIndex++;
		}
		return CToIndex[c];
	}

	public ColorRGBA ItoC(long i)
	{
		return IToColor[i];
	}
}

//this method uses color directly as the indices. for some reason that alters the output significantly
class MethodTwo : MethodBase
{
	public override void CreateProfile(ProgressBar pbar, ICanvas frame, Rectangle rect)
	{
		Profile = new();
		pbar.Prefix = "Creating Profile ";

		rect.ThreadPixels((x,y) => {
			int cy = y - rect.Top;
			int cx = x - rect.Left;
			var oc = frame[cx,cy];
			var maxx = frame.Width - 1;
			var maxy = frame.Height - 1;

			if (!Profile.ContainsKey(oc)) {
				var ncp = new ColorProfile<ColorRGBA> {
					NColor = new(), WColor = new(), SColor = new(), EColor = new()
				};
				Profile.TryAdd(oc,ncp);
			}

			//Log.Debug($"mx={maxx} my={maxy} w={frame.Width} h={frame.Height} cx={cx} cy={cy}");
			ColorRGBA? cn = null, cw = null, cs = null, ce = null;
			if (cy > 0)    { cn = frame[cx, cy - 1]; }
			if (cx > 0)    { cw = frame[cx - 1, cy]; }
			if (cy < maxy) { cs = frame[cx, cy + 1]; }
			if (cx < maxx) { ce = frame[cx + 1, cy]; }

			var cc = Profile[oc];
			if (cn != null) { AddUpdateCount(cc.NColor,cn.Value); }
			if (cw != null) { AddUpdateCount(cc.WColor,cw.Value); }
			if (cs != null) { AddUpdateCount(cc.SColor,cs.Value); }
			if (ce != null) { AddUpdateCount(cc.EColor,ce.Value); }

		//The above code is not thread-safe so forcing max concurrency to one instead of  Core.MaxDegreeOfParallelism
		},1,pbar);
	}

	void AddUpdateCount(IDictionary<ColorRGBA,long> dict, ColorRGBA color)
	{
		if (dict.ContainsKey(color)) {
			dict[color]++;
		} else {
			dict.Add(color,1);
		}
	}

	public override void CreateImage(ProgressBar pbar, ICanvas img)
	{
		pbar.Prefix = "Generating Image ";
		int iw = img.Width, ih = img.Height;
		double totalPixels = iw * ih;
		double visitedPixels = 0; //to keep track of progress

		var pixStack = new List<(int,int)>();
		VisitNest = new BitArray[ih];
		for(int ii = 0; ii < ih; ii++) {
			VisitNest[ii] = new BitArray(iw,false);
		}

		Rnd = O.RandomSeed.HasValue
			? new Random(O.RandomSeed.Value)
			: new Random()
		;

		//figure out starting nodes
		int maxNodes = O.StartLoc.Count;
		if (O.TotalNodes != null) {
			maxNodes = O.TotalNodes.Value;
		}
		//we want at least one
		if (maxNodes < 1) { maxNodes = 1; }

		while(O.StartLoc.Count < maxNodes) {
			O.StartLoc.Add(StartPoint.FromLinear(
				Rnd.Next(iw),
				Rnd.Next(ih)
			));
		}

		foreach(var startp in O.StartLoc) {
			int sx = 0,sy = 0;
			if (!startp.IsLinear) {
				sx = (int)(iw * startp.PX);
				sy = (int)(ih * startp.PY);
			}
			else {
				sx = startp.LX;
				sy = startp.LY;
			}

			//make sure the point is actually inside the image
			if (sx < 0 || sy < 0 || sx >= iw || sy >= ih) {
				continue;
			}

			pixStack.Add((sx,sy));
			//long ix = PickStartColor();
			//img[sx,sy] = ItoC(ix);
			var c = PickStartColor();
			img[sx,sy] = c;
			DoVisit(sx,sy);
		}

		if (pixStack.Count < 1) {
			Log.Error("None of the positions given are within the image");
			return;
		}

		int maxy = img.Height - 1;
		int maxx = img.Width - 1;

		while(pixStack.Count > 0) {
			//Log.Debug($"stack count={pixStack.Count}");

			//doing grab and swap so we can use list as a 'random pop' stack
			int rix = Rnd.Next(pixStack.Count);
			var (x,y) = pixStack[rix];
			pixStack[rix] = pixStack[pixStack.Count - 1];
			pixStack.RemoveAt(pixStack.Count - 1);

			pbar.Report(++visitedPixels / totalPixels);
			//long cx = CtoI(img[x,y]);
			//var profile = Profile[cx];
			var c = img[x,y];
			var profile = Profile[c];

			if (y > 0)    { PickAndVisit(img, pixStack, profile.NColor, x, y - 1); }
			if (x > 0)    { PickAndVisit(img, pixStack, profile.WColor, x - 1, y); }
			if (y < maxy) { PickAndVisit(img, pixStack, profile.SColor, x, y + 1); }
			if (x < maxx) { PickAndVisit(img, pixStack, profile.EColor, x + 1, y); }
		}
	}

	ColorRGBA PickStartColor()
	{
		int count = Profile.Count();
		int skip = Rnd.Next(count);
		return Profile.Skip(skip).First().Key;
	}

	void PickAndVisit(ICanvas img, List<(int,int)> stack, Dictionary<ColorRGBA,long> dict, int x, int y)
	{
		if (IsVisited(x,y)) {
			//Log.Debug($"already visited {x},{y}");
			return;
		}

		//find total number of buckets
		long total = 0;
		foreach(var kvp in dict) {
			total += kvp.Value;
		}

		long next = Rnd.RandomLong(total);
		//long nc = 0;
		var nc = Profile.First().Key;
		//this assumes the same order is kept between dictionary loops
		foreach(var kvp in dict) {
			total -= kvp.Value;
			if (total < next) {
				nc = kvp.Key;
			}
		}

		img[x,y] = nc;
		DoVisit(x,y);
		stack.Add((x,y));
	}

	Dictionary<ColorRGBA,ColorProfile<ColorRGBA>> Profile;
}

class ColorProfile<T>
{
	//dictionaries of index,count
	public Dictionary<T,long> NColor;
	public Dictionary<T,long> WColor;
	public Dictionary<T,long> SColor;
	public Dictionary<T,long> EColor;

	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.AppendLine("\tNorth =====");
		DColorToString(sb,NColor);
		sb.AppendLine("\tWest  =====");
		DColorToString(sb,WColor);
		sb.AppendLine("\tSouth =====");
		DColorToString(sb,SColor);
		sb.AppendLine("\tEast  =====");
		DColorToString(sb,EColor);
		return sb.ToString();
	}

	void DColorToString(StringBuilder sb, Dictionary<T,long> d)
	{
		foreach(var kvp in d) {
			sb.AppendLine($"\t{kvp.Key} #={kvp.Value}");
		}
	}
}