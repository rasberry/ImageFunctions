using ImageFunctions.Core;
using ImageFunctions.Core.ColorSpace;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.AllColors;

public static class DrawOriginal
{
	internal static void Draw(ICanvas image, int? maxThreads, Options O)
	{
		List<Color> colorList = null;
		var bounds = image.Bounds();

		if(O.WhichSpace != Space.None) {
			colorList = ConvertBySpace(bounds, O.WhichSpace, O.Order, maxThreads, O);
		}
		else {
			colorList = ConvertByPattern(bounds, O.SortBy, maxThreads, O);
		}

		var work = (int x, int y) => {
			int coff = y * bounds.Width + x;
			var nc = coff < colorList.Count
				? colorList[coff]
				: Color.Transparent;

			image[x, y] = ColorRGBA.FromRGBA255(nc.R, nc.G, nc.B, nc.A);
		};

		using var progress = new ProgressBar();
		progress.Prefix = "Rendering...";
		Tools.ThreadPixels(bounds, work, maxThreads, progress);
	}

	static List<Color> ConvertByPattern(Rectangle bounds, Pattern p, int? maxThreads, Options O)
	{
		Func<Color, double> converter = null;
		switch(p) {
		default:
		case Pattern.BitOrder: return PatternBitOrder(bounds, O).ToList();
		case Pattern.AERT: converter = ConvertAERT; break;
		case Pattern.HSP: converter = ConvertHSP; break;
		case Pattern.WCAG2: converter = ConvertWCAG2; break;
		case Pattern.Luminance601: converter = ConvertLuminance601; break;
		case Pattern.Luminance709: converter = ConvertLuminance709; break;
		case Pattern.Luminance2020: converter = ConvertLuminance2020; break;
		case Pattern.SMPTE240M: converter = ConvertSmpte1999; break;
		}

		return ConvertAndSort(bounds, converter, ComparersLuminance(), null, maxThreads, O);
	}

	static List<Color> ConvertBySpace(Rectangle bounds, Space space, int[] order, int? maxThreads, Options O)
	{
		switch(space) {
		case Space.RGB:
			return ConvertAndSort(bounds, c => c, ComparersRgba32(), order, maxThreads, O);
		//case Space.CieLab:
		//	return ConvertAndSort(c => _Converter.ToCieLab(c),ComparersCieLab(),rect,order,maxThreads);
		//case Space.CieLch:
		//	return ConvertAndSort(c => _Converter.ToCieLch(c),ComparersCieLch(),rect,order,maxThreads);
		//case Space.CieLchuv:
		//	return ConvertAndSort(c => _Converter.ToCieLchuv(c),ComparersCieLchuv(),rect,order,maxThreads);
		//case Space.CieLuv:
		//	return ConvertAndSort(c => _Converter.ToCieLuv(c),ComparersCieLuv(),rect,order,maxThreads);
		//case Space.CieXyy:
		//	return ConvertAndSort(c => _Converter.ToCieXyy(c),ComparersCieXyy(),rect,order,maxThreads);
		case Space.CieXyz:
			return ConvertAndSort(bounds, GetConverter(new ColorSpaceCie1931()), CompareIColor3(), order, maxThreads, O);
		case Space.Cmyk:
			return ConvertAndSort(bounds, GetConverter(new ColorSpaceCmyk()), CompareIColor4(), order, maxThreads, O);
		case Space.HSI:
			return ConvertAndSort(bounds, GetConverter(new ColorSpaceHsi()), CompareIColor3(), order, maxThreads, O);
		case Space.HSL:
			return ConvertAndSort(bounds, GetConverter(new ColorSpaceHsl()), CompareIColor3(), order, maxThreads, O);
		case Space.HSV:
			return ConvertAndSort(bounds, GetConverter(new ColorSpaceHsv()), CompareIColor3(), order, maxThreads, O);
		//case Space.HunterLab:
		//	return ConvertAndSort(c => _Converter.ToHunterLab(c),ComparersHunterLab(),rect,order,maxThreads);
		//case Space.LinearRgb:
		//	return ConvertAndSort(c => _Converter.ToLinearRgb(c),ComparersLinearRgb(),rect,order,maxThreads);
		//case Space.Lms:
		//	return ConvertAndSort(c => _Converter.ToLms(c),ComparersLms(),rect,order,maxThreads);
		case Space.YCbCr:
			return ConvertAndSort(bounds, GetConverter(new ColorSpaceYCbCrJpeg()), CompareIColor3(), order, maxThreads, O);
		}

		throw PlugSqueal.NotImplementedSpace(space);
	}

	//for some reason this offset produces the closest colors to the original
	// version of AllColors - not sure what changed
	const int magickOffset = 427296640;

	//return every color in numeric order
	static List<Color> PatternBitOrder(Rectangle rect, Options O)
	{
		bool isEmpty = rect == Rectangle.Empty;
		int num = isEmpty
			? Function.NumberOfColors
			: rect.Width * rect.Height;
		int offset = O.ColorOffset;
		//Log.Debug($"ColorOffset = {offset}");

		var cList = new List<Color>(num);
		for(int i = 0; i < Function.NumberOfColors; i++) {
			if(!isEmpty) {
				int y = i / Options.FourKWidth;
				int x = i % Options.FourKWidth;
				if(!rect.Contains(x, y)) { continue; }
			}

			int oi = (i + magickOffset + offset) % int.MaxValue;
			var color = ToColor(oi);
			cList.Add(color);
		}
		return cList;
	}

	//const int offset = 128 * 13421771; //(int)(2147483648.0 * 8/9);
	static Color ToColor(int color)
	{
		int r = (color >> 00) & 255;
		int g = (color >> 08) & 255;
		int b = (color >> 16) & 255;
		return Color.FromArgb(255, r, g, b);
	}

	static List<Color> ConvertAndSort<T>(Rectangle bounds, Func<Color, T> conv, Func<T, T, int>[] compList,
		int[] order, int? maxThreads, Options O)
	{
		var colorList = PatternBitOrder(bounds, O).ToList();
		var tempList = new List<(Color, T)>(colorList.Count);
		if(order != null) {
			//make sure order is at least as long as the colorList
			if(order.Length < compList.Length) {
				int[] fullOrder = new int[compList.Length];
				order.CopyTo(fullOrder, 0);
				for(int i = order.Length - 1; i < compList.Length; i++) {
					fullOrder[i] = int.MaxValue;
				}
				order = fullOrder;
			}
			//sort compList using order as the guide
			Array.Sort(order, compList);
		}

		using(var progress = new ProgressBar()) {
			progress.Prefix = "Converting...";
			for(int t = 0; t < colorList.Count; t++) {
				Color c = colorList[t];
				T next = conv(c);
				tempList.Add((c, next));
				progress.Report((double)t / colorList.Count);
			}
		}

		using(var progress = new ProgressBar()) {
			progress.Prefix = "Sorting...";

			int count = 0;
			var progressSorter = new Comparison<(Color, T)>((a, b) => {
				count++;
				progress.Report(count / Function.SortMax);
				return MultiSort(compList, a.Item2, b.Item2);
			});

			if(O.ParallelSort) {
				//parallel version seems to works best on 4+ cores
				var comp = Comparer<(Color, T)>.Create(
					new Comparison<(Color, T)>((a, b) => {
						return MultiSort(compList, a.Item2, b.Item2);
					})
				);
				PlugTools.ParallelSort(tempList, comp, progress, maxThreads);
			}
			else {
				//seems to be a lot faster than Array.Sort(key,collection)
				//single threaded version for machines with a low number of cores
				tempList.Sort(progressSorter);
			}
		}

		for(int t = 0; t < colorList.Count; t++) {
			colorList[t] = tempList[t].Item1;
		}

		return colorList;
	}

	static int MultiSort<T>(Func<T, T, int>[] compers, T a, T b)
	{
		for(int c = 0; c < compers.Length; c++) {
			var comp = compers[c];
			int d = comp(a, b);
			if(d != 0) { return d; }
		}
		return 0;
	}

	static Func<Color, IColor3> GetConverter(IColor3Space space)
	{
		var c = new Converter3Helper() { Space = space };
		return c.Convert;
	}

	class Converter3Helper
	{
		public IColor3Space Space;
		public IColor3 Convert(Color c)
		{
			var nc = ColorRGBA.FromRGBA255(c.R, c.G, c.B, c.A);
			return Space.ToSpace(nc);
		}
	}

	static Func<Color, IColor4> GetConverter(IColor4Space space)
	{
		var c = new Converter4Helper() { Space = space };
		return c.Convert;
	}

	class Converter4Helper
	{
		public IColor4Space Space;
		public IColor4 Convert(Color c)
		{
			var nc = ColorRGBA.FromRGBA255(c.R, c.G, c.B, c.A);
			return Space.ToSpace(nc);
		}
	}

	static Func<double, double, int>[] ComparersLuminance()
	{
		var arr = new Func<double, double, int>[] {
			(a,b) => a > b ? 1 : a < b ? -1 : 0,
		};
		return arr;
	}

	// https://en.wikipedia.org/wiki/Rec._709
	static double ConvertLuminance709(Color c)
	{
		double l = 0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B;
		return l;
	}

	// https://en.wikipedia.org/wiki/YCbCr#ITU-R_BT.601_conversion
	// https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness
	// https://en.wikipedia.org/wiki/Rec._601
	static double ConvertLuminance601(Color c)
	{
		double l = 0.2989 * c.R + 0.5870 * c.G + 0.1140 * c.B;
		return l;
	}

	// https://en.wikipedia.org/wiki/Rec._2020
	static double ConvertLuminance2020(Color c)
	{
		double l = 0.2627 * c.R + 0.6780 * c.G + 0.0593 * c.B;
		return l;
	}

	// http://www.w3.org/TR/AERT#color-contrast
	static double ConvertAERT(Color c)
	{
		double l = 0.2990 * c.R + 0.5870 * c.G + 0.1140 * c.B;
		return l;
	}

	// http://alienryderflex.com/hsp.html
	static double ConvertHSP(Color c)
	{
		double rr = c.R * c.R, gg = c.G * c.G, bb = c.B * c.B;
		double l = 0.2990 * rr + 0.5870 * gg + 0.1140 * bb;
		return Math.Sqrt(l);
	}

	// http://www.w3.org/TR/WCAG20/#relativeluminancedef
	static double ConvertWCAG2(Color c)
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
			: Math.Pow((val + 0.055) / 1.055, 2.4)
		;
		return c;
	}

	// http://discoverybiz.net/enu0/faq/faq_YUV_YCbCr_YPbPr.html
	static double ConvertSmpte1999(Color c)
	{
		double l = 0.2120 * c.R + 0.7010 * c.G + 0.0870 * c.B;
		return l;
	}

	static Func<Color, Color, int>[] ComparersRgba32()
	{
		var arr = new Func<Color, Color, int>[] {
			(a,b) => a.R > b.R ? 1 : a.R < b.R ? -1 : 0,
			(a,b) => a.G > b.G ? 1 : a.G < b.G ? -1 : 0,
			(a,b) => a.B > b.B ? 1 : a.B < b.B ? -1 : 0,
		};
		return arr;
	}

	static Func<IColor3, IColor3, int>[] CompareIColor3()
	{
		var arr = new Func<IColor3, IColor3, int>[] {
			(a,b) => a.C1 > b.C1 ? 1 : a.C1 < b.C1 ? -1 : 0,
			(a,b) => a.C2 > b.C2 ? 1 : a.C2 < b.C2 ? -1 : 0,
			(a,b) => a.C3 > b.C3 ? 1 : a.C3 < b.C3 ? -1 : 0,
		};
		return arr;
	}

	static Func<IColor4, IColor4, int>[] CompareIColor4()
	{
		var arr = new Func<IColor4, IColor4, int>[] {
			(a,b) => a.C1 > b.C1 ? 1 : a.C1 < b.C1 ? -1 : 0,
			(a,b) => a.C2 > b.C2 ? 1 : a.C2 < b.C2 ? -1 : 0,
			(a,b) => a.C3 > b.C3 ? 1 : a.C3 < b.C3 ? -1 : 0,
			(a,b) => a.C4 > b.C4 ? 1 : a.C4 < b.C4 ? -1 : 0,
		};
		return arr;
	}

	/*
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
	*/

	static Func<(double, double, double), (double, double, double), int>[] ComparersHsi()
	{
		var arr = new Func<(double, double, double), (double, double, double), int>[] {
			(a,b) => a.Item1 > b.Item1 ? 1 : a.Item1 < b.Item1 ? -1 : 0,
			(a,b) => a.Item2 > b.Item2 ? 1 : a.Item2 < b.Item2 ? -1 : 0,
			(a,b) => a.Item3 > b.Item3 ? 1 : a.Item3 < b.Item3 ? -1 : 0,
		};
		return arr;
	}

	/*
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
	*/
}
