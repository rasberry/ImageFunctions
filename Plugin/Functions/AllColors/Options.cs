using Rasberry.Cli;
using ImageFunctions.Core;

namespace ImageFunctions.Plugin.Functions.AllColors
{
	public enum Pattern {
		None = 0,
		BitOrder = 1,
		AERT,
		HSP,
		WCAG2,
		SMPTE240M,
		Luminance709,
		Luminance601,
		Luminance2020,
	}

	public enum Space {
		None = 0,
		RGB,
		HSV,
		HSL,
		HSI,
		YCbCr,
		//CieLab,
		//CieLch,
		//CieLchuv,
		//CieLuv,
		//CieXyy,
		CieXyz,
		Cmyk,
		//HunterLab,
		//LinearRgb,
		//Lms
	}

	public sealed class Options : IOptions
	{
		public void Usage(StringBuilder sb)
		{
			sb.ND(1,"Creates an image with every possible 24-bit color ordered by chosen pattern.");
			sb.ND(1,"-p (pattern)"   ,"Sort by Pattern (default BitOrder)");
			sb.ND(1,"-s (space)"     ,"Sort by color space components (instead of pattern)");
			sb.ND(1,"-so (n,...)"    ,"Change priority order of components (default 1,2,3,4)");
			sb.ND(1,"-ps"            ,"Use multi-threaded sort function instead of regular sort");
			sb.ND(1,"-o (number)[%]" ,"Color Offset to use (should be between 0% nad 100%");
			sb.ND(1,"-on (number)"   ,$"Absoulte Color Offset to use (should be between {int.MinValue} and {int.MaxValue}");
			sb.ND(1,"-l / --legacy"  ,"Use original (legacy) algorithm");
			sb.WT();
			sb.ND(1,"Available Patterns");
			sb.PrintEnum<Pattern>(1,GetPatternDescription);
			sb.WT();
			sb.ND(1,"Available Spaces");
			sb.PrintEnum<Space>(1);
		}

		static string GetPatternDescription(Pattern p)
		{
			switch(p)
			{
			case Pattern.BitOrder:       return "Numeric order";
			case Pattern.AERT:           return "AERT brightness";
			case Pattern.HSP:            return "HSP color model brightness";
			case Pattern.WCAG2:          return "WCAG2 relative luminance";
			case Pattern.Luminance709:   return "Luminance BT.709";
			case Pattern.Luminance601:   return "Luminance BT.601";
			case Pattern.Luminance2020:  return "Luminance BT.2020";
			case Pattern.SMPTE240M:      return "Luminance SMPTE 240M (1999)";
			}
			return "";
		}

		public bool ParseArgs(string[] args, IRegister register)
		{
			var p = new ParseParams(args);

			if (p.Default("-p",out SortBy,Pattern.None).IsInvalid()) {
				Tell.CouldNotParse("-p");
				return false;
			}
			if (p.Default("-s",out WhichSpace,Space.None).IsInvalid()) {
				Tell.CouldNotParse("-s");
				return false;
			}
			if (p.Has("-ps").IsGood()) {
				ParallelSort = true;
			}
			if (p.Has("-l").IsGood() || p.Has("--legacy").IsGood()) {
				UseOriginalCode = true;
			}

			var parser = new ParseParams.Parser<double?>((string n, out double? p) => {
				return ExtraParsers.TryParseNumberPercent(n,out p);
			});

			if (p.Default("-o", out ColorOffsetPct, 0.0, parser).IsInvalid()) {
				Tell.CouldNotParse("-o");
				return false;
			}
			if (p.Default("-on", out ColorOffsetAbs, 0).IsInvalid()) {
				Tell.CouldNotParse("-on");
				return false;
			}

			var pso = p.Default("-so",out string pri);
			if (pso.IsInvalid()) {
				Tell.CouldNotParse("-so");
				return false;
			}
			else if (pso.IsGood()) {
				string[] items = (pri??"").Split(',');
				if (items.Length < 1) {
					PlugTell.MustHaveOnePriority();
					return false;
				}
				int[] priorities = new int[items.Length];
				for(int i=0; i<items.Length; i++) {
					if (!int.TryParse(items[i],out var num)) {
						PlugTell.PriorityMustBeNumber();
						return false;
					}
					priorities[i] = num;
				}
				Order = priorities;
			}

			if (SortBy == Pattern.None && WhichSpace == Space.None) {
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

		internal int ColorOffset { get {
			if (ColorOffsetAbs.HasValue) {
				return ColorOffsetAbs.Value;
			}
			else {
				return (int)(int.MaxValue * ColorOffsetPct.GetValueOrDefault(0));
			}
		}}
	}
}