using System;
using System.Collections.Generic;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace ImageFunctions.AllColors
{
	// Inspired by
	// https://stackoverflow.com/questions/596216/formula-to-determine-brightness-of-rgb-color

	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
	{
		const int NumberOfColors = 16777216;

		public Options O = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			ListSorter sorter = null;
			switch(O.SortBy)
			{
			default:
			case Pattern.BitOrder:      sorter = SortByNumber; break;
			case Pattern.AERT:          sorter = SortByAERT; break;
			case Pattern.HSP:           sorter = SortByHSP; break;
			case Pattern.WCAG2:         sorter = SortByWCAG2; break;
			case Pattern.VofHSV:        sorter = SortByVofHSV; break;
			case Pattern.IofHSI:        sorter = SortByIofHSI; break;
			case Pattern.LofHSL:        sorter = SortByLofHSL; break;
			case Pattern.Luminance709:  sorter = SortByLuminance709; break;
			case Pattern.Luminance601:  sorter = SortByLuminance601; break;
			case Pattern.Luminance2020: sorter = SortByLuminance2020; break;
			case Pattern.SMPTE240M:     sorter = SortBySmpte1999; break;
			}

			List<Rgba32> colorList = PatternSorter(sorter);
			var transparent = Color.Transparent.ToPixel<TPixel>();

			using (var progress = new ProgressBar())
			{
				MoreHelpers.ThreadPixels(rect, config.MaxDegreeOfParallelism, (x,y) => {
					int cy = y - rect.Top;
					int cx = x - rect.Left;
					int coff = cy * rect.Width + cx;

					TPixel nc;
					if (coff < colorList.Count) {
						Rgba32 cc = colorList[coff];
						nc = default(TPixel);
						nc.FromRgba32(cc);
					}
					else {
						nc = transparent;
					}
					frame.GetPixelSpan()[coff] = nc;
				},progress);
			}
		}

		static List<Rgba32> PatternBitOrder()
		{
			var cList = new List<Rgba32>(NumberOfColors);
			for(int i=0; i<NumberOfColors; i++) {
				var color = new Rgba32((uint)i);
				color.A = 255;
				cList.Add(color);
			}
			return cList;
		}

		delegate double ColorSortValue(Rgba32 c);
		delegate int ListSorter(Rgba32 a, Rgba32 b);

		static List<Rgba32> PatternSorter(ListSorter sorter)
		{
			var cList = PatternBitOrder();
			cList.Sort(new Comparison<Rgba32>(sorter));
			return cList;
		}

		static int SortByNumber(Rgba32 ca, Rgba32 cb)
		{
			return (int)(ca.PackedValue - cb.PackedValue);
		}

		// https://en.wikipedia.org/wiki/Rec._709
		static int SortByLuminance709(Rgba32 ca, Rgba32 cb)
		{
			double la = 0.2126 * ca.R + 0.7152 * ca.G + 0.0722 * ca.B;
			double lb = 0.2126 * cb.R + 0.7152 * cb.G + 0.0722 * cb.B;
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// https://en.wikipedia.org/wiki/YCbCr#ITU-R_BT.601_conversion
		// https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness
		// https://en.wikipedia.org/wiki/Rec._601
		static int SortByLuminance601(Rgba32 ca, Rgba32 cb)
		{
			double la = 16.0 + (65.481 * ca.R / 255.0) + (128.553 * ca.G / 255.0) + (24.966 * ca.B / 255.0);
			double lb = 16.0 + (65.481 * cb.R / 255.0) + (128.553 * cb.G / 255.0) + (24.966 * cb.B / 255.0);
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// https://en.wikipedia.org/wiki/Rec._2020
		static int SortByLuminance2020(Rgba32 ca, Rgba32 cb)
		{
			double la = 0.2627 * ca.R + 0.6780 * ca.G + 0.0593 * ca.B;
			double lb = 0.2627 * cb.R + 0.6780 * cb.G + 0.0593 * cb.B;
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// http://www.w3.org/TR/AERT#color-contrast
		static int SortByAERT(Rgba32 ca, Rgba32 cb)
		{
			double la = 0.299 * ca.R + 0.587 * ca.G + 0.114 * ca.B;
			double lb = 0.299 * cb.R + 0.587 * cb.G + 0.114 * cb.B;
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// http://alienryderflex.com/hsp.html
		static int SortByHSP(Rgba32 ca, Rgba32 cb)
		{
			double la = Math.Sqrt(0.299 * ca.R * ca.R + 0.587 * ca.G * ca.G + 0.114 * ca.B * ca.B);
			double lb = Math.Sqrt(0.299 * cb.R * cb.R + 0.587 * cb.G * cb.G + 0.114 * cb.B * cb.B);
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// http://www.w3.org/TR/WCAG20/#relativeluminancedef
		static int SortByWCAG2(Rgba32 ca, Rgba32 cb)
		{
			double la = 0.2126 * WCAG2Normalize(ca.R) + 0.7152 * WCAG2Normalize(ca.G) + 0.0722 * WCAG2Normalize(ca.B);
			double lb = 0.2126 * WCAG2Normalize(cb.R) + 0.7152 * WCAG2Normalize(cb.G) + 0.0722 * WCAG2Normalize(cb.B);
			return la > lb ? 1 : la < lb ? -1 : 0;
		}
		static double WCAG2Normalize(byte component)
		{
			double val = component / 255.0;
			double c = val <= 0.03928
				? val / 12.92
				: Math.Pow((val + 0.055)/1.055,2.4)
			;
			return c;
		}

		// https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness
		static int SortByVofHSV(Rgba32 ca, Rgba32 cb)
		{
			byte la = Math.Max(ca.R,Math.Max(ca.G,ca.B));
			byte lb = Math.Max(cb.R,Math.Max(cb.G,cb.B));
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness
		static int SortByIofHSI(Rgba32 ca, Rgba32 cb)
		{
			double la = ca.R + ca.G + ca.B / 3.0;
			double lb = cb.R + cb.G + cb.B / 3.0;
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness
		static int SortByLofHSL(Rgba32 ca, Rgba32 cb)
		{
			byte xa = Math.Max(ca.R,Math.Max(ca.G,ca.B));
			byte xb = Math.Max(cb.R,Math.Max(cb.G,cb.B));
			byte ma = Math.Min(ca.R,Math.Min(ca.G,ca.B));
			byte mb = Math.Min(cb.R,Math.Min(cb.G,cb.B));
			double la = xa + ma / 2.0;
			double lb = xb + mb / 2.0;
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// http://discoverybiz.net/enu0/faq/faq_YUV_YCbCr_YPbPr.html
		static int SortBySmpte1999(Rgba32 ca, Rgba32 cb)
		{
			double la = 0.212 * ca.R + 0.701 * ca.G + 0.087 * ca.B;
			double lb = 0.212 * cb.R + 0.701 * cb.G + 0.087 * cb.B;
			return la > lb ? 1 : la < lb ? -1 : 0;
		}
	}
}