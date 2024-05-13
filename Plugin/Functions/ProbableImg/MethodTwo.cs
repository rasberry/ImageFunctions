using ImageFunctions.Core;

namespace ImageFunctions.Plugin.Functions.ProbableImg;

//this method uses color directly as the indices. for some reason that alters the output significantly
class MethodTwo : MethodBase
{
	protected override void UpdateCounts(ColorRGBA oc, ICanvas frame, (ColorRGBA?,ColorRGBA?,ColorRGBA?,ColorRGBA?) fourSides)
	{
		UpdateCountsBase(oc,frame,Profile,fourSides,AddUpdateCount);
	}

	void AddUpdateCount(IDictionary<ColorRGBA,long> dict, ColorRGBA color)
	{
		if (dict.ContainsKey(color)) {
			dict[color]++;
		} else {
			dict.Add(color,1);
		}
	}

	protected override void PickAndVisitFour(ICanvas img, int x, int y, List<(int,int)> pixStack)
	{
		var c = img[x,y];
		var profile = Profile[c];
		PickAndVisitFour(img,x,y,pixStack,profile,SetPicked);
	}

	protected override void SetStartColor(ICanvas img, int sx, int sy)
	{
		int count = Profile.Count;
		int skip = Rnd.Next(count);
		var c = Profile.Skip(skip).First().Key;
		img[sx,sy] = c;

	}

	void SetPicked(ICanvas img, Dictionary<ColorRGBA,long> dict, int x, int y)
	{
		var nc = Profile.First().Key;
		FindNextBucket(dict,ref nc);
		img[x,y] = nc;
	}

	readonly Dictionary<ColorRGBA,ColorProfile<ColorRGBA>> Profile = new();
}