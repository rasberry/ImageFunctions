using System;
using System.Collections.Generic;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
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
			if (O.WhichSpace != Space.None) {
				sorter = SortByColorSpaceComponent;
			}
			else {
				switch(O.SortBy)
				{
				default:
				case Pattern.BitOrder:      sorter = SortByNumber; break;
				case Pattern.AERT:          sorter = SortByAERT; break;
				case Pattern.HSP:           sorter = SortByHSP; break;
				case Pattern.WCAG2:         sorter = SortByWCAG2; break;
				case Pattern.Luminance709:  sorter = SortByLuminance709; break;
				case Pattern.Luminance601:  sorter = SortByLuminance601; break;
				case Pattern.Luminance2020: sorter = SortByLuminance2020; break;
				case Pattern.SMPTE240M:     sorter = SortBySmpte1999; break;
				}
			}

			List<Rgba32> colorList = null;
			using (var progress = new ProgressBar())
			{
				progress.Prefix = "Sorting...";
				colorList = PatternSorter(sorter,progress);
			}
			var transparent = Color.Transparent.ToPixel<TPixel>();

			using (var progress = new ProgressBar())
			{
				progress.Prefix = "Rendering...";
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

		static ColorSpaceConverter _Converter = new ColorSpaceConverter();
		static List<Rgba32> PatternSorter(ListSorter sorter,ProgressBar progress)
		{
			//there doesn't seem to be a sort with progress so take a guess
			// at the maximum number of iterations
			double max = 5 * 7 * NumberOfColors; //5 * log(n) * n
			int count = 0;
			var progressSorter = new Func<Rgba32,Rgba32,int>((a,b) => {
				count++;
				progress.Report(count / max);
				return sorter(a,b);
			});
			var cList = PatternBitOrder();
			cList.Sort(new Comparison<Rgba32>(progressSorter));
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

		// http://discoverybiz.net/enu0/faq/faq_YUV_YCbCr_YPbPr.html
		static int SortBySmpte1999(Rgba32 ca, Rgba32 cb)
		{
			double la = 0.212 * ca.R + 0.701 * ca.G + 0.087 * ca.B;
			double lb = 0.212 * cb.R + 0.701 * cb.G + 0.087 * cb.B;
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		int SortByColorSpaceComponent(Rgba32 ca, Rgba32 cb)
		{
			var space = O.WhichSpace;
			var comp = O.WhichComp;

			var converter = _Converter;
			double va = 0.0;
			double vb = 0.0;

			switch(space)
			{
			case Space.RGB: {
				switch(comp) {
					case Component.First:  va = ca.R / 255.0; vb = cb.R / 255.0; break;
					case Component.Second: va = ca.G / 255.0; vb = cb.G / 255.0; break;
					case Component.Third:  va = ca.B / 255.0; vb = cb.B / 255.0; break;
				} break; }
			case Space.HSV: {
				var ha = converter.ToHsv(ca);
				var hb = converter.ToHsv(cb);
				switch(comp) {
					case Component.First:  va = ha.H; vb = hb.H; break;
					case Component.Second: va = ha.S; vb = hb.S; break;
					case Component.Third:  va = ha.V; vb = hb.V; break;
				} break; }
			case Space.HSL: {
				var ha = converter.ToHsl(ca);
				var hb = converter.ToHsl(cb);
				switch(comp) {
					case Component.First:  va = ha.H; vb = hb.H; break;
					case Component.Second: va = ha.S; vb = hb.S; break;
					case Component.Third:  va = ha.L; vb = hb.L; break;
				} break; }
			case Space.HSI: {
				var ha = ImageHelpers.ConvertToHSI(ca);
				var hb = ImageHelpers.ConvertToHSI(cb);
				switch(comp) {
					case Component.First:  va = ha.Item1; vb = hb.Item1; break;
					case Component.Second: va = ha.Item2; vb = hb.Item2; break;
					case Component.Third:  va = ha.Item3; vb = hb.Item3; break;
				} break; }
			case Space.YCbCr: {
				var ha = converter.ToYCbCr(ca);
				var hb = converter.ToYCbCr(cb);
				switch(comp) {
					case Component.First:  va = ha.Y; vb = hb.Y; break;
					case Component.Second: va = ha.Cb; vb = hb.Cb; break;
					case Component.Third:  va = ha.Cr; vb = hb.Cr; break;
				} break; }
			case Space.CieLab: {
				var ha = converter.ToCieLab(ca);
				var hb = converter.ToCieLab(cb);
				switch(comp) {
					case Component.First:  va = ha.L; vb = hb.L; break;
					case Component.Second: va = ha.A; vb = hb.A; break;
					case Component.Third:  va = ha.B; vb = hb.B; break;
				} break; }
			case Space.CieLch: {
				var ha = converter.ToCieLch(ca);
				var hb = converter.ToCieLch(cb);
				switch(comp) {
					case Component.First:  va = ha.L; vb = hb.L; break;
					case Component.Second: va = ha.C; vb = hb.C; break;
					case Component.Third:  va = ha.H; vb = hb.H; break;
				} break; }
			case Space.CieLchuv: {
				var ha = converter.ToCieLchuv(ca);
				var hb = converter.ToCieLchuv(cb);
				switch(comp) {
					case Component.First:  va = ha.L; vb = hb.L; break;
					case Component.Second: va = ha.C; vb = hb.C; break;
					case Component.Third:  va = ha.H; vb = hb.H; break;
				} break; }
			case Space.CieLuv: {
				var ha = converter.ToCieLuv(ca);
				var hb = converter.ToCieLuv(cb);
				switch(comp) {
					case Component.First:  va = ha.L; vb = hb.L; break;
					case Component.Second: va = ha.U; vb = hb.U; break;
					case Component.Third:  va = ha.V; vb = hb.V; break;
				} break; }
			case Space.CieXyy: {
				var ha = converter.ToCieXyy(ca);
				var hb = converter.ToCieXyy(cb);
				switch(comp) {
					case Component.First:  va = ha.X; vb = hb.X; break;
					case Component.Second: va = ha.Y; vb = hb.Y; break;
					case Component.Third:  va = ha.Yl; vb = hb.Yl; break;
				} break; }
			case Space.CieXyz: {
				var ha = converter.ToCieXyz(ca);
				var hb = converter.ToCieXyz(cb);
				switch(comp) {
					case Component.First:  va = ha.X; vb = hb.X; break;
					case Component.Second: va = ha.Y; vb = hb.Y; break;
					case Component.Third:  va = ha.Z; vb = hb.Z; break;
				} break; }
			case Space.Cmyk: {
				var ha = converter.ToCmyk(ca);
				var hb = converter.ToCmyk(cb);
				switch(comp) {
					case Component.First:  va = ha.C; vb = hb.C; break;
					case Component.Second: va = ha.M; vb = hb.M; break;
					case Component.Third:  va = ha.Y; vb = hb.Y; break;
					case Component.Fourth: va = ha.K; vb = hb.K; break;
				} break; }
			case Space.HunterLab: {
				var ha = converter.ToHunterLab(ca);
				var hb = converter.ToHunterLab(cb);
				switch(comp) {
					case Component.First:  va = ha.L; vb = hb.L; break;
					case Component.Second: va = ha.A; vb = hb.A; break;
					case Component.Third:  va = ha.B; vb = hb.B; break;
				} break; }
			case Space.LinearRgb: {
				var ha = converter.ToLinearRgb(ca);
				var hb = converter.ToLinearRgb(cb);
				switch(comp) {
					case Component.First:  va = ha.R; vb = hb.R; break;
					case Component.Second: va = ha.G; vb = hb.G; break;
					case Component.Third:  va = ha.B; vb = hb.B; break;
				} break; }
			case Space.Lms: {
				var ha = converter.ToLms(ca);
				var hb = converter.ToLms(cb);
				switch(comp) {
					case Component.First:  va = ha.L; vb = hb.L; break;
					case Component.Second: va = ha.M; vb = hb.M; break;
					case Component.Third:  va = ha.S; vb = hb.S; break;
				} break; }
			}
			return va > vb ? 1 : va < vb ? -1 : 0;
		}
	}
}