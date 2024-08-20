using ImageFunctions.Core;
using ImageFunctions.Plugin.Aides;

namespace ImageFunctions.Plugin.Functions.ProbableImg;

//This method uses extra dictionaries to map colors to indices (original code)
class MethodOne : MethodBase
{
	protected override void UpdateCounts(ColorRGBA oc, ICanvas frame, (ColorRGBA?, ColorRGBA?, ColorRGBA?, ColorRGBA?) fourSides)
	{
		long ix = CtoI(oc);
		UpdateCountsBase(ix, frame, Profile, fourSides, AddUpdateCount);
	}

	void AddUpdateCount(IDictionary<long, long> dict, ColorRGBA color)
	{
		long ix = CtoI(color);
		if(dict.ContainsKey(ix)) {
			dict[ix]++;
		}
		else {
			dict.Add(ix, 1);
		}
	}

	protected override void PickAndVisitFour(ICanvas img, int x, int y, List<(int, int)> pixStack)
	{
		long cx = CtoI(img[x, y]);
		var profile = Profile[cx];
		PickAndVisitFour(img, x, y, pixStack, profile, SetPicked);
	}

	protected override void SetStartColor(ICanvas img, int sx, int sy)
	{
		long ix = Rnd.RandomLong(max: LastIndex);
		img[sx, sy] = ItoC(ix);
	}

	void SetPicked(ICanvas img, Dictionary<long, long> dict, int x, int y)
	{
		long nc = 0;
		FindNextBucket(dict, ref nc);
		img[x, y] = ItoC(nc);
	}

	public long CtoI(ColorRGBA c)
	{
		if(!CToIndex.TryGetValue(c, out long i)) {
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

	readonly Dictionary<long, ColorProfile<long>> Profile = new();
	readonly Dictionary<ColorRGBA, long> CToIndex = new();
	readonly Dictionary<long, ColorRGBA> IToColor = new();
	long LastIndex = 0;
}
