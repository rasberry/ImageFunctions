using System.Drawing;
using System.Text;
using ImageFunctions.Core;
using Rasberry.Cli;
using O = ImageFunctions.Plugin.AllColors.Options;

namespace ImageFunctions.Plugin.AllColors;

public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		O.Usage(sb);
	}

	// Inspired by
	// https://stackoverflow.com/questions/596216/formula-to-determine-brightness-of-rgb-color

	public bool Run(ILayers layers, string[] args)
	{
		if (layers == null) {
			throw new ArgumentNullException("layers");
		}
		if (!O.ParseArgs(args)) {
			return false;
		}

		//since we're rendering pixels make a new layer each time
		var image = Tools.Engine.NewImage(O.FourKWidth,O.FourKHeight);
		Draw(image);
		layers.Add(image);

		return true;

	}

	const int NumberOfColors = 16777216;

	//there doesn't seem to be a sort with progress so take a guess
	// at the maximum number of iterations
	const double SortMax = 24 * NumberOfColors; //log2(n) * n

	void Draw(ICanvas image)
	{
		List<Color> colorList = null;

		if (O.WhichSpace != Space.None) {
			colorList = ConvertBySpace(image, O.WhichSpace, O.Order);
		}
		else {
			colorList = ConvertByPattern(image, O.SortBy);
		}

		var transparent = ImageHelpers.NativeToRgba(ColorHelpers.Transparent);

		int coff = 0;
		var work = (int x, int y) => {
			Color nc;
			if (++coff < colorList.Count) {
				nc = colorList[coff];
			}
			else {
				nc = transparent;
			}
			image[x,y] = ImageHelpers.RgbaToNative(nc);
		};

		using (var progress = new ProgressBar()) {
			progress.Prefix = "Rendering...";
			Tools.ThreadPixels(image, work ,progress);
		}
	}

		List<Color> ConvertByPattern(ICanvas image, Pattern p)
		{
			Func<Color,double> converter = null;
			switch(p)
			{
			default:
			case Pattern.BitOrder:      return PatternBitOrder(image);
			case Pattern.AERT:          converter = ConvertAERT; break;
			case Pattern.HSP:           converter = ConvertHSP; break;
			case Pattern.WCAG2:         converter = ConvertWCAG2; break;
			case Pattern.Luminance601:  converter = ConvertLuminance601; break;
			case Pattern.Luminance709:  converter = ConvertLuminance709; break;
			case Pattern.Luminance2020: converter = ConvertLuminance2020; break;
			case Pattern.SMPTE240M:     converter = ConvertSmpte1999; break;
			}

			return ConvertAndSort<double>(converter,ComparersLuminance(),rect);
		}

		List<Color> ConvertBySpace(ICanvas image, Space space, int[] order)
		{
			switch(space)
			{
			case Space.RGB:
				return ConvertAndSort(image, c => c,CompareColorFun(),order);
			//case Space.CieLab:
			//	return ConvertAndSort(c => _Converter.ToCieLab(c),ComparersCieLab(),rect,order);
			//case Space.CieLch:
			//	return ConvertAndSort(c => _Converter.ToCieLch(c),ComparersCieLch(),rect,order);
			//case Space.CieLchuv:
			//	return ConvertAndSort(c => _Converter.ToCieLchuv(c),ComparersCieLchuv(),rect,order);
			//case Space.CieLuv:
			//	return ConvertAndSort(c => _Converter.ToCieLuv(c),ComparersCieLuv(),rect,order);
			//case Space.CieXyy:
			//	return ConvertAndSort(c => _Converter.ToCieXyy(c),ComparersCieXyy(),rect,order);
			case Space.CieXyz:
				return ConvertAndSort(image, GetConverter(new ColorSpaceCie1931()),CompareI3Color(),order);
			case Space.Cmyk:
				return ConvertAndSort(image, GetConverter(new ColorSpaceCmyk()),CompareI4Color(),order);
			case Space.HSI:
				return ConvertAndSort(image, GetConverter(new ColorSpaceHsi()),CompareI3Color(),order);
			case Space.HSL:
				return ConvertAndSort(image, GetConverter(new ColorSpaceHsl()),CompareI3Color(),order);
			case Space.HSV:
				return ConvertAndSort(image, GetConverter(new ColorSpaceHsv()),CompareI3Color(),order);
			//case Space.HunterLab:
			//	return ConvertAndSort(c => _Converter.ToHunterLab(c),ComparersHunterLab(),rect,order);
			//case Space.LinearRgb:
			//	return ConvertAndSort(c => _Converter.ToLinearRgb(c),ComparersLinearRgb(),rect,order);
			//case Space.Lms:
			//	return ConvertAndSort(c => _Converter.ToLms(c),ComparersLms(),rect,order);
			case Space.YCbCr:
				return ConvertAndSort(GetConverter(new ColorSpaceYCbCrJpeg()),CompareI3Color(),order);
			}

			throw new NotImplementedException($"Space {space} is not implemented");
		}

		//return every color in numeric order
		static IEnumerable<ColorFun> PatternBitOrder(ICanvas image)
		{
			int len = Math.Min(image.Width * image.Height, NumberOfColors);

			for(int i=0; i<len; i++) {
				double r = ((i >> 00) & 255) / 255.0;
				double g = ((i >> 08) & 255) / 255.0;
				double b = ((i >> 16) & 255) / 255.0;
				yield return new ColorFun(r, g, b, 1.0);
			}
		}

		List<Color> ConvertAndSort<T>(ICanvas image, Func<ColorFun,T> conv, Func<T,T,int>[] compList, int[] order = null)
		{
			var colorList = PatternBitOrder(image).ToList();
			var tempList = new List<(ColorFun,T)>(colorList.Count);
			if (order != null) {
				//make sure order is at least as long as the colorList
				if (order.Length < compList.Length) {
					int[] fullOrder = new int[compList.Length];
					order.CopyTo(fullOrder,0);
					for(int i = order.Length - 1; i < compList.Length; i++) {
						fullOrder[i] = int.MaxValue;
					}
					order = fullOrder;
				}
				//sort compList using order as the guide
				Array.Sort(order,compList);
			}

			using (var progress = new ProgressBar())
			{
				progress.Prefix = "Converting...";
				for(int t=0; t<colorList.Count; t++) {
					 var c = colorList[t];
					T next = conv(c);
					tempList.Add((c,next));
					progress.Report((double)t / colorList.Count);
				}
			}

			using (var progress = new ProgressBar())
			{
				progress.Prefix = "Sorting...";

				int count = 0;
				var progressSorter = new Comparison<(ColorFun,T)>((a,b) => {
					count++;
					progress.Report(count / SortMax);
					return MultiSort(compList,a.Item2,b.Item2);
				});

				if (O.NoParallelSort) {
					//seems to be a lot faster than Array.Sort(key,collection)
					//single threaded version for machines with a low number of cores
					tempList.Sort(progressSorter);
				}
				else {
					//parallel version seems to works best on 4+ cores
					var comp = Comparer<(Color,T)>.Create(
						new Comparison<(Color,T)>((a,b) => {
							return MultiSort(compList,a.Item2,b.Item2);
						})
					);
					MoreHelpers.ParalellSort<(Color,T)>(tempList,comp,progress,MaxDegreeOfParallelism);
				}
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

		static Func<ColorFun,IColor3> GetConverter(I3ColorSpace space)
		{
			var c = new Converter3Helper() { Space = space };
			return c.Convert;
		}

		class Converter3Helper
		{
			public I3ColorSpace Space;
			public IColor3 Convert(Color c)
			{
				var ic = ImageHelpers.RgbaToNative(c);
				return Space.ToSpace(ic);
			}
		}

		static Func<Color,IColor4> GetConverter(I4ColorSpace space)
		{
			var c = new Converter4Helper() { Space = space };
			return c.Convert;
		}

		class Converter4Helper
		{
			public I4ColorSpace Space;
			public IColor4 Convert(Color c)
			{
				var ic = ImageHelpers.RgbaToNative(c);
				return Space.ToSpace(ic);
			}
		}

		static Func<double,double,int>[] ComparersLuminance()
		{
			var arr = new Func<double,double,int>[] {
				(a,b) => a > b ? 1 : a < b ? -1 : 0,
			};
			return arr;
		}

		// https://en.wikipedia.org/wiki/Rec._709
		static double ConvertLuminance709(ColorFun c)
		{
			double l = 0.2126 * c.C1 + 0.7152 * c.C2 + 0.0722 * c.C3;
			return l;
		}

		// https://en.wikipedia.org/wiki/YCbCr#ITU-R_BT.601_conversion
		// https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness
		// https://en.wikipedia.org/wiki/Rec._601
		static double ConvertLuminance601(ColorFun c)
		{
			double l = 0.2989 * c.C1 + 0.5870 * c.C2 + 0.1140 * c.C3;
			return l;
		}

		// https://en.wikipedia.org/wiki/Rec._2020
		static double ConvertLuminance2020(ColorFun c)
		{
			double l = 0.2627 * c.C1 + 0.6780 * c.C2 + 0.0593 * c.C3;
			return l;
		}

		// http://www.w3.org/TR/AERT#color-contrast
		static double ConvertAERT(ColorFun c)
		{
			double l = 0.2990 * c.C1 + 0.5870 * c.C2 + 0.1140 * c.C3;
			return l;
		}

		// http://alienryderflex.com/hsp.html
		static double ConvertHSP(ColorFun c)
		{
			double rr = c.C1 * c.C1, gg = c.C2 * c.C2, bb = c.C3 * c.C3;
			double l = 0.2990 * rr + 0.5870 * gg + 0.1140 * bb;
			return Math.Sqrt(l);
		}

		// http://www.w3.org/TR/WCAG20/#relativeluminancedef
		static double ConvertWCAG2(ColorFun c)
		{
			double l =
				  0.2126 * WCAG2Normalize(c.C1)
				+ 0.7152 * WCAG2Normalize(c.C2)
				+ 0.0722 * WCAG2Normalize(c.C3);
			return l;
		}
		static double WCAG2Normalize(double val)
		{
			double c = val <= 0.03928
				? val / 12.92
				: Math.Pow((val + 0.055)/1.055,2.4)
			;
			return c;
		}

		// http://discoverybiz.net/enu0/faq/faq_YUV_YCbCr_YPbPr.html
		static double ConvertSmpte1999(ColorFun c)
		{
			double l = 0.2120 * c.C1 + 0.7010 * c.C2 + 0.0870 * c.C3;
			return l;
		}

		static Func<ColorFun,ColorFun,int>[] CompareColorFun()
		{
			var arr = new Func<ColorFun,ColorFun,int>[] {
				(a,b) => a.C1 > b.C1 ? 1 : a.C1 < b.C1 ? -1 : 0,
				(a,b) => a.C2 > b.C2 ? 1 : a.C2 < b.C2 ? -1 : 0,
				(a,b) => a.C3 > b.C3 ? 1 : a.C3 < b.C3 ? -1 : 0,
			};
			return arr;
		}

		static Func<Color,Color,int>[] ComparersRgba32()
		{
			var arr = new Func<Color,Color,int>[] {
				(a,b) => a.R > b.R ? 1 : a.R < b.R ? -1 : 0,
				(a,b) => a.G > b.G ? 1 : a.G < b.G ? -1 : 0,
				(a,b) => a.B > b.B ? 1 : a.B < b.B ? -1 : 0,
			};
			return arr;
		}

		static Func<IColor,IColor,int>[] CompareIFColor()
		{
			var arr = new Func<IColor,IColor,int>[] {
				(a,b) => a.R > b.R ? 1 : a.R < b.R ? -1 : 0,
				(a,b) => a.G > b.G ? 1 : a.G < b.G ? -1 : 0,
				(a,b) => a.B > b.B ? 1 : a.B < b.B ? -1 : 0,
			};
			return arr;
		}

		static Func<IColor3,IColor3,int>[] CompareI3Color()
		{
			var arr = new Func<IColor3,IColor3,int>[] {
				(a,b) => a._1 > b._1 ? 1 : a._1 < b._1 ? -1 : 0,
				(a,b) => a._2 > b._2 ? 1 : a._2 < b._2 ? -1 : 0,
				(a,b) => a._3 > b._3 ? 1 : a._3 < b._3 ? -1 : 0,
			};
			return arr;
		}

		static Func<IColor4,IColor4,int>[] CompareI4Color()
		{
			var arr = new Func<IColor4,IColor4,int>[] {
				(a,b) => a._1 > b._1 ? 1 : a._1 < b._1 ? -1 : 0,
				(a,b) => a._2 > b._2 ? 1 : a._2 < b._2 ? -1 : 0,
				(a,b) => a._3 > b._3 ? 1 : a._3 < b._3 ? -1 : 0,
				(a,b) => a._4 > b._4 ? 1 : a._4 < b._4 ? -1 : 0,
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

		static Func<(double,double,double),(double,double,double),int>[] ComparersHsi()
		{
			var arr = new Func<(double,double,double),(double,double,double),int>[] {
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
}
