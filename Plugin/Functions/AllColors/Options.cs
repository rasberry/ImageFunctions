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
			sb.ND(1,"-p (pattern)","Sort by Pattern (default BitOrder)");
			sb.ND(1,"-s (space)"  ,"Sort by color space components (instead of pattern)");
			sb.ND(1,"-so (n,...)" ,"Change priority order of components (default 1,2,3,4)");
			sb.ND(1,"-np"         ,"Use single threaded sort function instead of parallel sort");
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
				return false;
			}
			if (p.Default("-s",out WhichSpace,Space.None).IsInvalid()) {
				return false;
			}
			if (p.Has("-np").IsGood()) {
				NoParallelSort = true;
			}

			var pso = p.Default("-so",out string pri);
			if (pso.IsInvalid()) {
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

		public Pattern SortBy = Pattern.None;
		public Space WhichSpace = Space.None;
		public int[] Order = null;
		public bool NoParallelSort = false;
		public const int FourKWidth = 4096;
		public const int FourKHeight = 4096;
	}
}