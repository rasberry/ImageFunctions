using System;
using System.IO;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.Primitives;
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
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string curr = args[a];
				if (curr == "-p" && ++a < len) {
					if (!OptionsHelpers.TryParse<Pattern>(args[a],out Pattern pat)) {
						Log.Error("invalid pattern "+args[a]);
						return false;
					}
					O.SortBy = pat;
				}
				else if (curr == "-s" && ++a < len) {
					if (!OptionsHelpers.TryParse<Space>(args[a],out Space choose)) {
						Log.Error("invalid color space "+args[a]);
						return false;
					}
					O.WhichSpace = choose;
				}
				else if (curr == "-so" && ++a < len) {
					string[] items = args[a].Split(',');
					if (items.Length < 1) {
						Log.Error("You must provide at least one priority");
						return false;
					}
					int[] priorities = new int[items.Length];
					for(int i=0; i<items.Length; i++) {
						if (!int.TryParse(items[i],out var num)) {
							Log.Error("Each priority must be a number");
							return false;
						}
						priorities[i] = num;
					}
					O.Order = priorities;
				}
				else if (curr == "-np") {
					O.NoParallelSort = true;
				}
				else if (OutImage == null) {
					OutImage = curr;
				}
			}

			if (String.IsNullOrEmpty(OutImage)) {
				OutImage = OptionsHelpers.CreateOutputFileName(nameof(AllColors));
			}

			if (O.SortBy == Pattern.None && O.WhichSpace == Space.None) {
				O.SortBy = Pattern.BitOrder;
			}
			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.AllColors);
			sb.AppendLine();
			sb.AppendLine(name + " [options] [output image]");
			sb.AppendLine(" Creates an image with every possible 24-bit color ordered by chosen pattern.");
			sb.AppendLine(" -p (pattern)                Sort by Pattern (default BitOrder)");
			sb.AppendLine(" -s (space)                  Sort by color space components (instead of pattern)");
			sb.AppendLine(" -so (c,...)                 Change priority order of components (default 1,2,3,4)");
			sb.AppendLine(" -np                         Use single threaded sort function instead of parallel sort");
			sb.AppendLine();
			sb.AppendLine(" Available Patterns");

			OptionsHelpers.PrintEnum<Pattern>(sb,true,(e) => {
				return GetPatternDescription(e);
			});

			sb.AppendLine();
			sb.AppendLine(" Available Spaces");
			OptionsHelpers.PrintEnum<Space>(sb,true);
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

		static IEnumerable<Pattern> AllPatterns(out int max)
		{
			var list = new List<Pattern>();
			max = int.MinValue;
			foreach(Pattern p in OptionsHelpers.EnumAll<Pattern>()) {
				list.Add(p);
				if ((int)p > max) { max = (int)p; }
			}
			list.Sort((a,b) => (int)a - (int)b);
			return list;
		}

		public override IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(Image<TPixel> source, Rectangle sourceRectangle)
		{
			var proc = new Processor<TPixel>();
			proc.O = O;
			proc.Source = source;
			proc.Bounds = sourceRectangle;
			return proc;
		}

		public override void Main()
		{
			Main<Rgba32>();
		}

		Options O = new Options();
	}
}