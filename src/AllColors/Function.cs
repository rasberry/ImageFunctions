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
			return new Size(FourKWidth,FourKHeigt);
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
				else if (OutImage == null) {
					OutImage = curr;
				}
			}

			if (String.IsNullOrEmpty(OutImage)) {
				OutImage = OptionsHelpers.CreateOutputFileName(nameof(AllColors));
			}
			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.AllColors);
			sb.AppendLine();
			sb.AppendLine(name + " [options] [output image]");
			sb.AppendLine(" Creates an image with every possible 24-bit color ordered by chosen pattern.");
			sb.AppendLine(" -t (number)                 Number of times to run fit function (default 7)");
			sb.AppendLine(" -p (pattern)                Pattern to use (default BitOrder)");
			sb.AppendLine();
			sb.AppendLine(" Available Patterns");

			var allPatterns = AllPatterns(out int max);
			int numLen = 1 + (int)Math.Floor(Math.Log10(max));
			foreach(Pattern p in allPatterns) {
				string pnum = ((int)p).ToString();
				string npad = pnum.Length < numLen ? new string(' ',numLen - pnum.Length) : "";
				string pname = p.ToString();
				string ppad = new string(' ',26 - pname.Length);
				string pdsc = GetPatternDescription(p);
				Log.Debug("npadlen = "+npad.Length);
				sb.AppendLine($"{npad}{pnum}. {pname}{ppad}{pdsc}");
			}
		}

		static string GetPatternDescription(Pattern p)
		{
			switch(p)
			{
			case Pattern.BitOrder:       return "Numeric order";
			case Pattern.AERT:           return "AERT brightness";
			case Pattern.HSP:            return "HSP color model brightness";
			case Pattern.WCAG2:          return "WCAG2 relative luminance";
			case Pattern.VofHSV:         return "Value of HSV";
			case Pattern.IofHSI:         return "Intensity of HSI";
			case Pattern.LofHSL:         return "Lightness of HSL";
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
			foreach(Pattern p in Enum.GetValues(typeof(Pattern))) {
				if (p == Pattern.None) { continue; }
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
			proc.SourceRectangle = sourceRectangle;
			return proc;
		}

		public override void Main()
		{
			Main<Rgba32>();
		}

		Options O = new Options();
		const int FourKWidth = 4096;
		const int FourKHeigt = 4096;
	}
}