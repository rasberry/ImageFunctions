using System;
using System.IO;
using System.Text;
using System.Drawing;
using ImageFunctions.Helpers;
using System.Collections.Generic;

namespace ImageFunctions.AllColors
{
	public class Function : AbstractFunction, IGenerator
	{
		public Size StartingSize { get {
			return new Size(Options.FourKWidth,Options.FourKHeight);
		}}

		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);

			if (p.Default("-p",out O.SortBy,Pattern.None).IsInvalid()) {
				return false;
			}
			if (p.Default("-s",out O.WhichSpace,Space.None).IsInvalid()) {
				return false;
			}
			if (p.Has("-np").IsGood()) {
				O.NoParallelSort = true;
			}

			var pso = p.Default("-so",out string pri);
			if (pso.IsInvalid()) {
				return false;
			}
			else if (pso.IsGood()) {
				string[] items = (pri??"").Split(',');
				if (items.Length < 1) {
					Tell.MustHaveOnePriority();
					return false;
				}
				int[] priorities = new int[items.Length];
				for(int i=0; i<items.Length; i++) {
					if (!int.TryParse(items[i],out var num)) {
						Tell.PriorityMustBeNumber();
						return false;
					}
					priorities[i] = num;
				}
				O.Order = priorities;
			}

			if (p.DefaultFile(out OutImage,nameof(AllColors)).IsBad()) {
				return false;
			}

			if (O.SortBy == Pattern.None && O.WhichSpace == Space.None) {
				O.SortBy = Pattern.BitOrder;
			}
			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.AllColors);
			sb.WL();
			sb.WL(0,name + " [options] [output image]");
			sb.WL(1,"Creates an image with every possible 24-bit color ordered by chosen pattern.");
			sb.WL(1,"-p (pattern)","Sort by Pattern (default BitOrder)");
			sb.WL(1,"-s (space)"  ,"Sort by color space components (instead of pattern)");
			sb.WL(1,"-so (n,...)" ,"Change priority order of components (default 1,2,3,4)");
			sb.WL(1,"-np"         ,"Use single threaded sort function instead of parallel sort");
			sb.WL();
			sb.WL(1,"Available Patterns");
			sb.PrintEnum<Pattern>(1,GetPatternDescription);
			sb.WL();
			sb.WL(1,"Available Spaces");
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

		protected override AbstractProcessor CreateProcessor()
		{
			return new Processor { O = O };
		}

		Options O = new Options();

	}

}