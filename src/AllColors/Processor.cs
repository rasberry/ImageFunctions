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
		//there doesn't seem to be a sort with progress so take a guess
		// at the maximum number of iterations
		const double SortMax = 5 * 7 * NumberOfColors; //5 * log(n) * n
		public Options O = null;

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rect, Configuration config)
		{
			List<Rgba32> colorList = null;

			if (O.WhichSpace != Space.None) {
				colorList = ConvertBySpace(O.WhichSpace, O.Order);
			}
			else {
				colorList = ConvertByPattern(O.SortBy);
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

		static List<Rgba32> ConvertByPattern(Pattern p)
		{
			Func<Rgba32,double> converter = null;
			switch(p)
			{
			default:
			case Pattern.BitOrder:      return PatternBitOrder();
			case Pattern.AERT:          converter = ConvertAERT; break;
			case Pattern.HSP:           converter = ConvertHSP; break;
			case Pattern.WCAG2:         converter = ConvertWCAG2; break;
			case Pattern.Luminance709:  converter = ConvertLuminance709; break;
			case Pattern.Luminance601:  converter = ConvertLuminance601; break;
			case Pattern.Luminance2020: converter = ConvertLuminance2020; break;
			case Pattern.SMPTE240M:     converter = ConvertSmpte1999; break;
			}

			return ConvertAndSort<double>(converter,ComparersLuminance());
		}

		static List<Rgba32> ConvertBySpace(Space space, int[] order)
		{
			switch(space)
			{
			case Space.RGB:
				return ConvertAndSort(c => c,ComparersRgba32(),order);
			case Space.CieLab:
				return ConvertAndSort(c => _Converter.ToCieLab(c),ComparersCieLab(),order);
			case Space.CieLch:
				return ConvertAndSort(c => _Converter.ToCieLch(c),ComparersCieLch(),order);
			case Space.CieLchuv:
				return ConvertAndSort(c => _Converter.ToCieLchuv(c),ComparersCieLchuv(),order);
			case Space.CieLuv:
				return ConvertAndSort(c => _Converter.ToCieLuv(c),ComparersCieLuv(),order);
			case Space.CieXyy:
				return ConvertAndSort(c => _Converter.ToCieXyy(c),ComparersCieXyy(),order);
			case Space.CieXyz:
				return ConvertAndSort(c => _Converter.ToCieXyz(c),ComparersCieXyz(),order);
			case Space.Cmyk:
				return ConvertAndSort(c => _Converter.ToCmyk(c),ComparersCmyk(),order);
			case Space.HSI:
				return ConvertAndSort(c => ImageHelpers.ConvertToHSI(c),ComparersHsi(),order);
			case Space.HSL:
				return ConvertAndSort(c => _Converter.ToHsl(c),ComparersHsl(),order);
			case Space.HSV:
				return ConvertAndSort(c => _Converter.ToHsv(c),ComparersHsv(),order);
			case Space.HunterLab:
				return ConvertAndSort(c => _Converter.ToHunterLab(c),ComparersHunterLab(),order);
			case Space.LinearRgb:
				return ConvertAndSort(c => _Converter.ToLinearRgb(c),ComparersLinearRgb(),order);
			case Space.Lms:
				return ConvertAndSort(c => _Converter.ToLms(c),ComparersLms(),order);
			case Space.YCbCr:
				return ConvertAndSort(c => _Converter.ToYCbCr(c),ComparersYCbCr(),order);
			}

			throw new NotImplementedException($"Space {space} is not implemented");
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

		static List<Rgba32> ConvertAndSort<T>(Func<Rgba32,T> conv, Func<T,T,int>[] compList, int[] order = null)
			where T : struct
		{
			var colorList = PatternBitOrder();
			var tempList = new List<(Rgba32,T)>(colorList.Count);
			if (order != null) {
				//make sure order is at least as long as the colorList
				if (order.Length < compList.Length) {
					int[] fullOrder = new int[compList.Length];
					order.CopyTo(fullOrder,0);
					for(int i = order.Length - 1; i < compList.Length; i++) {
						order[i] = int.MaxValue;
					}
				}
				//sort compList using order as the guide
				Array.Sort(order,compList);
			}

			using (var progress = new ProgressBar())
			{
				progress.Prefix = "Converting...";
				for(int t=0; t<colorList.Count; t++) {
					Rgba32 c = colorList[t];
					T next = conv(c);
					tempList.Add((c,next));
					progress.Report((double)t / colorList.Count);
				}
			}

			using (var progress = new ProgressBar())
			{
				progress.Prefix = "Sorting...";

				int count = 0;
				var progressSorter = new Comparison<(Rgba32,T)>((a,b) => {
					count++;
					progress.Report(count / SortMax);
					return MultiSort(compList,a.Item2,b.Item2);
				});
				//seems to be a lot faster than Array.Sort(key,collection)
				tempList.Sort(progressSorter);
				
				//TODO this is slower on core2 - test on i3
				//var comp = Comparer<(Rgba32,T)>.Create(
				//	new Comparison<(Rgba32,T)>((a,b) => {
				//		return MultiSort(compList,a.Item2,b.Item2);
				//	})
				//);
				//MoreHelpers.ParalellSort<(Rgba32,T)>(tempList,comp,progress);
			}

			for(int t=0; t<colorList.Count; t++) {
				colorList[t] = tempList[t].Item1;
			}

			return colorList;
		}

		static int MultiSort<T>(Func<T,T,int>[] compers,T a, T b)
		{
			for(int c=0; c<compers.Length; c++)
			{
				var comp = compers[c];
				int d = comp(a,b);
				if (d != 0) { return d; }
			}
			return 0;
		}

		//static List<Rgba32> PatternSorter(Func<Rgba32,Rgba32,int> sorter,ProgressBar progress)
		//{
		//	int count = 0;
		//	var progressSorter = new Comparison<Rgba32>((a,b) => {
		//		count++;
		//		progress.Report(count / SortMax);
		//		return sorter(a,b);
		//	});
		//	var cList = PatternBitOrder();
		//	cList.Sort(progressSorter);
		//	return cList;
		//}

		static ColorSpaceConverter _Converter = new ColorSpaceConverter();

		static Func<double,double,int>[] ComparersLuminance()
		{
			var arr = new Func<double,double,int>[] {
				(a,b) => a > b ? 1 : a < b ? -1 : 0,
			};
			return arr;
		}

		// https://en.wikipedia.org/wiki/Rec._709
		static double ConvertLuminance709(Rgba32 c)
		{
			double l = 0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B;
			return l;
		}

		// https://en.wikipedia.org/wiki/YCbCr#ITU-R_BT.601_conversion
		// https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness
		// https://en.wikipedia.org/wiki/Rec._601
		static double ConvertLuminance601(Rgba32 c)
		{
			double l = 16.0
				+ ( 65.481 * c.R / 255.0)
				+ (128.553 * c.G / 255.0)
				+ ( 24.966 * c.B / 255.0)
			;
			return l;
		}


		// https://en.wikipedia.org/wiki/Rec._2020
		static double ConvertLuminance2020(Rgba32 c)
		{
			double l = 0.2627 * c.R + 0.6780 * c.G + 0.0593 * c.B;
			return l;
		}

		// http://www.w3.org/TR/AERT#color-contrast
		static double ConvertAERT(Rgba32 c)
		{
			double l = 0.299 * c.R + 0.587 * c.G + 0.114 * c.B;
			return l;
		}

		// http://alienryderflex.com/hsp.html
		static double ConvertHSP(Rgba32 c)
		{
			double l = 0.299 * c.R * c.R + 0.587 * c.G * c.G + 0.114 * c.B * c.B;
			return Math.Sqrt(l);
		}

		// http://www.w3.org/TR/WCAG20/#relativeluminancedef
		static double ConvertWCAG2(Rgba32 c)
		{
			double l =
				  0.2126 * WCAG2Normalize(c.R)
				+ 0.7152 * WCAG2Normalize(c.G)
				+ 0.0722 * WCAG2Normalize(c.B);
			return l;
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
		static double ConvertSmpte1999(Rgba32 c)
		{
			double l = 0.212 * c.R + 0.701 * c.G + 0.087 * c.B;
			return l;
		}

		static Func<Rgba32,Rgba32,int>[] ComparersRgba32()
		{
			var arr = new Func<Rgba32,Rgba32,int>[] {
				(a,b) => a.R > b.R ? 1 : a.R < b.R ? -1 : 0,
				(a,b) => a.G > b.G ? 1 : a.G < b.G ? -1 : 0,
				(a,b) => a.B > b.B ? 1 : a.B < b.B ? -1 : 0,
			};
			return arr;
		}

		static Func<Hsv,Hsv,int>[] ComparersHsv()
		{
			var arr = new Func<Hsv,Hsv,int>[] {
				(a,b) => a.H > b.H ? 1 : a.H < b.H ? -1 : 0,
				(a,b) => a.S > b.S ? 1 : a.S < b.S ? -1 : 0,
				(a,b) => a.V > b.V ? 1 : a.V < b.V ? -1 : 0,
			};
			return arr;
		}

		static Func<Hsl,Hsl,int>[] ComparersHsl()
		{
			var arr = new Func<Hsl,Hsl,int>[] {
				(a,b) => a.H > b.H ? 1 : a.H < b.H ? -1 : 0,
				(a,b) => a.S > b.S ? 1 : a.S < b.S ? -1 : 0,
				(a,b) => a.L > b.L ? 1 : a.L < b.L ? -1 : 0,
			};
			return arr;
		}

		static Func<CieLab,CieLab,int>[] ComparersCieLab()
		{
			var arr = new Func<CieLab,CieLab,int>[] {
				(a,b) => a.L > b.L ? 1 : a.L < b.L ? -1 : 0,
				(a,b) => a.A > b.A ? 1 : a.A < b.A ? -1 : 0,
				(a,b) => a.B > b.B ? 1 : a.B < b.B ? -1 : 0,
			};
			return arr;
		}

		static Func<CieLch,CieLch,int>[] ComparersCieLch()
		{
			var arr = new Func<CieLch,CieLch,int>[] {
				(a,b) => a.L > b.L ? 1 : a.L < b.L ? -1 : 0,
				(a,b) => a.C > b.C ? 1 : a.C < b.C ? -1 : 0,
				(a,b) => a.H > b.H ? 1 : a.H < b.H ? -1 : 0,
			};
			return arr;
		}

		static Func<CieLchuv,CieLchuv,int>[] ComparersCieLchuv()
		{
			var arr = new Func<CieLchuv,CieLchuv,int>[] {
				(a,b) => a.L > b.L ? 1 : a.L < b.L ? -1 : 0,
				(a,b) => a.C > b.C ? 1 : a.C < b.C ? -1 : 0,
				(a,b) => a.H > b.H ? 1 : a.H < b.H ? -1 : 0,
			};
			return arr;
		}

		static Func<CieLuv,CieLuv,int>[] ComparersCieLuv()
		{
			var arr = new Func<CieLuv,CieLuv,int>[] {
				(a,b) => a.L > b.L ? 1 : a.L < b.L ? -1 : 0,
				(a,b) => a.U > b.U ? 1 : a.U < b.U ? -1 : 0,
				(a,b) => a.V > b.V ? 1 : a.V < b.V ? -1 : 0,
			};
			return arr;
		}

		static Func<CieXyy,CieXyy,int>[] ComparersCieXyy()
		{
			var arr = new Func<CieXyy,CieXyy,int>[] {
				(a,b) => a.X > b.X ? 1 : a.X < b.X ? -1 : 0,
				(a,b) => a.Y > b.Y ? 1 : a.Y < b.Y ? -1 : 0,
				(a,b) => a.Yl > b.Yl ? 1 : a.Yl < b.Yl ? -1 : 0,
			};
			return arr;
		}

		static Func<CieXyz,CieXyz,int>[] ComparersCieXyz()
		{
			var arr = new Func<CieXyz,CieXyz,int>[] {
				(a,b) => a.X > b.X ? 1 : a.X < b.X ? -1 : 0,
				(a,b) => a.Y > b.Y ? 1 : a.Y < b.Y ? -1 : 0,
				(a,b) => a.Z > b.Z ? 1 : a.Z < b.Z ? -1 : 0,
			};
			return arr;
		}

		static Func<Cmyk,Cmyk,int>[] ComparersCmyk()
		{
			var arr = new Func<Cmyk,Cmyk,int>[] {
				(a,b) => a.C > b.C ? 1 : a.C < b.C ? -1 : 0,
				(a,b) => a.M > b.M ? 1 : a.M < b.M ? -1 : 0,
				(a,b) => a.Y > b.Y ? 1 : a.Y < b.Y ? -1 : 0,
				(a,b) => a.K > b.K ? 1 : a.K < b.K ? -1 : 0,
			};
			return arr;
		}

		static Func<(double,double,double),(double,double,double),int>[] ComparersHsi()
		{
			var arr = new Func<(double,double,double),(double,double,double),int>[] {
				(a,b) => a.Item1 > b.Item1 ? 1 : a.Item1 < b.Item1 ? -1 : 0,
				(a,b) => a.Item2 > b.Item2 ? 1 : a.Item2 < b.Item2 ? -1 : 0,
				(a,b) => a.Item3 > b.Item3 ? 1 : a.Item3 < b.Item3 ? -1 : 0,
			};
			return arr;
		}

		static Func<HunterLab,HunterLab,int>[] ComparersHunterLab()
		{
			var arr = new Func<HunterLab,HunterLab,int>[] {
				(a,b) => a.L > b.L ? 1 : a.L < b.L ? -1 : 0,
				(a,b) => a.A > b.A ? 1 : a.A < b.A ? -1 : 0,
				(a,b) => a.B > b.B ? 1 : a.B < b.B ? -1 : 0,
			};
			return arr;
		}

		static Func<LinearRgb,LinearRgb,int>[] ComparersLinearRgb()
		{
			var arr = new Func<LinearRgb,LinearRgb,int>[] {
				(a,b) => a.R > b.R ? 1 : a.R < b.R ? -1 : 0,
				(a,b) => a.G > b.G ? 1 : a.G < b.G ? -1 : 0,
				(a,b) => a.B > b.B ? 1 : a.B < b.B ? -1 : 0,
			};
			return arr;
		}

		static Func<Lms,Lms,int>[] ComparersLms()
		{
			var arr = new Func<Lms,Lms,int>[] {
				(a,b) => a.L > b.L ? 1 : a.L < b.L ? -1 : 0,
				(a,b) => a.M > b.M ? 1 : a.M < b.M ? -1 : 0,
				(a,b) => a.S > b.S ? 1 : a.S < b.S ? -1 : 0,
			};
			return arr;
		}

		static Func<YCbCr,YCbCr,int>[] ComparersYCbCr()
		{
			var arr = new Func<YCbCr,YCbCr,int>[] {
				(a,b) => a.Y > b.Y ? 1 : a.Y < b.Y ? -1 : 0,
				(a,b) => a.Cb > b.Cb ? 1 : a.Cb < b.Cb ? -1 : 0,
				(a,b) => a.Cr > b.Cr ? 1 : a.Cr < b.Cr ? -1 : 0,
			};
			return arr;
		}
	}
}