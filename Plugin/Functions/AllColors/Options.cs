using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.AllColors;

public enum Pattern
{
	None = 0,
	BitOrder = 1,
	AERT = 2,
	HSP = 3,
	WCAG2 = 4,
	SMPTE240M = 5,
	Luminance709 = 6,
	Luminance601 = 7,
	Luminance2020 = 8,
	Spiral16 = 9,
	Spiral4kBuckets = 10,
	Spiral4k = 11,
	Squares4k = 12
}

public enum Space
{
	None = 0,
	RGB = 1,
	HSV = 2,
	HSL = 3,
	HSI = 4,
	YCbCr = 5,
	CieXyz = 6,
	Cmyk = 7,
	//CieLab,
	//CieLch,
	//CieLchuv,
	//CieLuv,
	//CieXyy,
	//HunterLab,
	//LinearRgb,
	//Lms
}

public sealed class Options : IOptions, IUsageProvider
{
	public Options(IFunctionContext context)
	{
		if(context == null) { throw Squeal.ArgumentNull(nameof(context)); }
		Log = context.Log;
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		return new Usage {
			Description = new UsageDescription(1, "Creates an image with every possible 24-bit color ordered by chosen pattern."),
			Parameters = [
				new UsageOne<Pattern>(1, "-p", "Sort by Pattern (default BitOrder)"),
				new UsageOne<Space>(1, "-s", "Sort by color space components (instead of pattern)"),
				new UsageOne<string>(1, "-so", "Change priority order of components (default 1,2,3,4)") { TypeText = "n,..." },
				new UsageOne<bool>(1, "-ps", "Use multi-threaded sort function instead of regular sort"),
				new UsageOne<double>(1, "-o", "Color Offset to use (should be between 0% and 100%") { IsNumberPct = true },
				new UsageOne<int>(1, "-on", $"Absolute Color Offset to use"),
				new UsageOne<bool>(1, "-l / --legacy", "Use original (legacy) algorithm"),
			],
			EnumParameters = [
				new UsageEnum<Pattern>(1, "Available Patterns") { DescriptionMap = GetPatternDescription, ExcludeZero = true },
				new UsageEnum<Space>(1, "Available Spaces") { ExcludeZero = true }
			]
		};
	}

	static string GetPatternDescription(object t)
	{
		if(t is not Pattern p) {
			throw Squeal.InvalidArgument(nameof(t));
		}
		switch(p) {
		case Pattern.BitOrder: return "Numeric order";
		case Pattern.AERT: return "AERT brightness";
		case Pattern.HSP: return "HSP color model brightness";
		case Pattern.WCAG2: return "WCAG2 relative luminance";
		case Pattern.Luminance709: return "Luminance BT.709";
		case Pattern.Luminance601: return "Luminance BT.601";
		case Pattern.Luminance2020: return "Luminance BT.2020";
		case Pattern.SMPTE240M: return "Luminance SMPTE 240M (1999)";
		case Pattern.Spiral16: return "Spiral blocks of 16 x 16";
		case Pattern.Spiral4kBuckets: return "Spiral blocks of 4k x 4k using buckets";
		case Pattern.Spiral4k: return "Spiral blocks of 4k x 4k";
		case Pattern.Squares4k: return "Square blocks of 256 x 256";
		}
		return "";
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if(p.Scan<Pattern>("-p", Pattern.None)
			.WhenGoodOrMissing(r => { SortBy = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<Space>("-s", Space.None)
			.WhenGoodOrMissing(r => { WhichSpace = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Has("-ps").IsGood()) {
			ParallelSort = true;
		}
		if(p.Has("-l").IsGood() || p.Has("--legacy").IsGood()) {
			UseOriginalCode = true;
		}

		var parser = new ParseParams.Parser<double?>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});

		if(p.Scan<double?>("-o", null, parser)
			.WhenGoodOrMissing(r => { ColorOffsetPct = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<int?>("-on", null)
			.WhenGoodOrMissing(r => { ColorOffsetAbs = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<string>("-so")
			.WhenGood(r => {
				string[] items = (r.Value ?? "").Split(',');
				if(items.Length < 1) {
					Log.Error(PlugNote.MustHaveOnePriority());
					return r with { Result = ParseParams.Result.UnParsable };
				}
				int[] priorities = new int[items.Length];
				for(int i = 0; i < items.Length; i++) {
					if(!int.TryParse(items[i], out var num)) {
						Log.Error(PlugNote.PriorityMustBeNumber());
						return r with { Result = ParseParams.Result.UnParsable };
					}
					priorities[i] = num;
				}
				Order = priorities;
				return r;
			})
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(SortBy == Pattern.None && WhichSpace == Space.None) {
			SortBy = Pattern.BitOrder;
		}
		return true;
	}

	public Pattern SortBy;
	public Space WhichSpace;
	public int[] Order;
	public bool ParallelSort = false;
	public bool UseOriginalCode = false;
	public double? ColorOffsetPct;
	public int? ColorOffsetAbs;

	public const int FourKWidth = 4096;
	public const int FourKHeight = 4096;

	internal int ColorOffset {
		get {
			if(ColorOffsetPct.HasValue) {
				return (int)(int.MaxValue * ColorOffsetPct.GetValueOrDefault(0));
			}
			else {
				return ColorOffsetAbs.GetValueOrDefault(0);
			}
		}
	}

	//This fills in the entries if Order is smaller than needed
	//example: user gives 1,2 but length is 3 this will return
	// [1,2,max]
	int[] FixedOrder = null;
	internal int[] GetFixedOrder(int length)
	{
		if(Order == null) { return null; }
		if(FixedOrder == null || FixedOrder.Length < length) {
			int[] fullOrder = new int[length];
			Order.CopyTo(fullOrder, 0);
			for(int i = Order.Length; i < length; i++) {
				fullOrder[i] = int.MaxValue;
			}
			FixedOrder = fullOrder;
		}
		//must be cloned because Array.Sort modifies the keys each time
		return (int[])FixedOrder.Clone();
	}

	readonly ICoreLog Log;
}
