using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.ColorSpace;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;
using PlugMath = ImageFunctions.Plugin.Aides.MathAide;

namespace ImageFunctions.Plugin.Functions.AllColors;

// Inspired by
// https://stackoverflow.com/questions/596216/formula-to-determine-brightness-of-rgb-color
// To count colors use Gimp -> Colors -> Info -> Color cube analysis

[InternalRegisterFunction(nameof(AllColors))]
public class Function : IFunction
{
	public static IFunction Create(IRegister register, ILayers layers, ICoreOptions core)
	{
		var f = new Function {
			Register = register,
			CoreOptions = core,
			Layers = layers
		};
		return f;
	}

	public IOptions Options { get { return O; }}

	public bool Run(string[] args)
	{
		//Trace.WriteLine($"{nameof(AllColors)} Run 1");
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!O.ParseArgs(args, Register)) {
			return false;
		}
		//Trace.WriteLine($"{nameof(AllColors)} Run 2");

		//since we're rendering pixels make a new layer each time
		var engine = CoreOptions.Engine.Item.Value;
		var (dfw, dfh) = CoreOptions.GetDefaultWidthHeight(AllColors.Options.FourKWidth, AllColors.Options.FourKHeight);
		var image = engine.NewCanvasFromLayersOrDefault(Layers, dfw, dfh);
		Layers.Push(image);
		//Trace.WriteLine($"{nameof(AllColors)} Run 3");

		if(O.UseOriginalCode) {
			DrawOriginal.Draw(image, CoreOptions.MaxDegreeOfParallelism, O);
		}
		else {
			Draw(image);
		}

		//Trace.WriteLine($"{nameof(AllColors)} Run 4");
		return true;
	}

	readonly Options O = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions CoreOptions;

	internal const int NumberOfColors = 16777216;
	//there doesn't seem to be a sort with progress so take a guess
	// at the maximum number of iterations
	internal const double SortMax = 24 * NumberOfColors; //log2(n) * n

	void Draw(ICanvas image)
	{
		//Trace.WriteLine($"{nameof(AllColors)} Draw 1");
		List<ColorRGBA> colorList = null;
		using var progress = new ProgressBar();
		progress.Prefix = "Converting... ";

		if(O.WhichSpace != Space.None) {
			colorList = ConvertBySpace(image, O.WhichSpace, progress);
		}
		else {
			colorList = ConvertByPattern(image, O.SortBy, progress);
		}

		void copyColors(int x, int y)
		{
			int coff = y * image.Height + x;
			var nc = coff < colorList.Count
				? colorList[coff]
				: Core.Aides.ColorAide.Transparent;
			//Trace.WriteLine($"{nameof(AllColors)} copyColors {x},{y}");
			image[x, y] = nc;
		}

		//Trace.WriteLine($"{nameof(AllColors)} Draw 2");
		progress.Prefix = "Rendering... ";
		image.ThreadPixels(copyColors, CoreOptions.MaxDegreeOfParallelism, progress);
		//Trace.WriteLine($"{nameof(AllColors)} Draw 3");
	}

	List<ColorRGBA> ConvertByPattern(ICanvas image, Pattern p, ProgressBar progress)
	{
		Func<ColorRGBA, double> converter = null;
		switch(p) {
		default:
		// TODO these patterns could be converted to a colorspace before returning the list ^_^
		case Pattern.BitOrder: return PatternBitOrder(image, progress, true).ToList();
		case Pattern.Spiral16: return Spiral16(image, progress).ToList();
		case Pattern.Spiral4k: return Spiral4k(image, progress).ToList();
		case Pattern.Spiral4kBuckets: return Spiral4kBuckets(image, progress).ToList();
		case Pattern.Squares4k: return Squares4k(image, progress).ToList();
		// the rest of these are custom luminance sorts
		case Pattern.AERT: converter = ConvertAERT; break;
		case Pattern.HSP: converter = ConvertHSP; break;
		case Pattern.WCAG2: converter = ConvertWCAG2; break;
		case Pattern.Luminance601: converter = ConvertLuminance601; break;
		case Pattern.Luminance709: converter = ConvertLuminance709; break;
		case Pattern.Luminance2020: converter = ConvertLuminance2020; break;
		case Pattern.SMPTE240M: converter = ConvertSmpte1999; break;
		}

		if (converter == null) {
			throw Squeal.NotSupported($"Pattern {p}");
		}

		return ConvertAndSort(image, converter, ComparersLuminance(), progress);
	}

	List<ColorRGBA> ConvertBySpace(ICanvas image, Space space, ProgressBar progress)
	{
		switch(space) {
		case Space.RGB:
			return ConvertAndSort(image, c => c, CompareColorRGBA(), progress);
		//case Space.CieLab:
		//	return ConvertAndSort(c => _Converter.ToCieLab(c),ComparersCieLab(),rect);
		//case Space.CieLch:
		//	return ConvertAndSort(c => _Converter.ToCieLch(c),ComparersCieLch(),rect);
		//case Space.CieLchuv:
		//	return ConvertAndSort(c => _Converter.ToCieLchuv(c),ComparersCieLchuv(),rect);
		//case Space.CieLuv:
		//	return ConvertAndSort(c => _Converter.ToCieLuv(c),ComparersCieLuv(),rect);
		//case Space.CieXyy:
		//	return ConvertAndSort(c => _Converter.ToCieXyy(c),ComparersCieXyy(),rect);
		case Space.CieXyz:
			return ConvertAndSort(image, GetConverter(new ColorSpaceCie1931()), CompareIColor3(), progress);
		case Space.Cmyk:
			return ConvertAndSort(image, GetConverter(new ColorSpaceCmyk()), CompareIColor4(), progress);
		case Space.HSI:
			return ConvertAndSort(image, GetConverter(new ColorSpaceHsi()), CompareIColor3(), progress);
		case Space.HSL:
			return ConvertAndSort(image, GetConverter(new ColorSpaceHsl()), CompareIColor3(), progress);
		case Space.HSV:
			return ConvertAndSort(image, GetConverter(new ColorSpaceHsv()), CompareIColor3(), progress);
		//case Space.HunterLab:
		//	return ConvertAndSort(c => _Converter.ToHunterLab(c),ComparersHunterLab(),rect);
		//case Space.LinearRgb:
		//	return ConvertAndSort(c => _Converter.ToLinearRgb(c),ComparersLinearRgb(),rect);
		//case Space.Lms:
		//	return ConvertAndSort(c => _Converter.ToLms(c),ComparersLms(),rect);
		case Space.YCbCr:
			return ConvertAndSort(image, GetConverter(new ColorSpaceYCbCrJpeg()), CompareIColor3(), progress);
		}

		throw PlugSqueal.NotImplementedSpace(space);
	}

	//return every color in numeric order
	//Note: multiple functions use this pattern as their staring point to ordering should not be
	// applied unless it's output is being rendered directly
	IEnumerable<ColorRGBA> PatternBitOrder(ICanvas image, ProgressBar progress, bool applyOrder = false)
	{
		int total = Math.Min(image.Width * image.Height, NumberOfColors);
		for(int i = 0; i < total; i++) {
			int ic = (i + O.ColorOffset) % int.MaxValue;
			double r = ((ic >> 00) & 255) / 255.0;
			double g = ((ic >> 08) & 255) / 255.0;
			double b = ((ic >> 16) & 255) / 255.0;

			const int componentCount = 3;
			if(applyOrder && O.Order != null) {
				var order = O.GetFixedOrder(componentCount);
				var items = new[] { r, g, b };
				Array.Sort(order, items);
				yield return new ColorRGBA(items[0], items[1], items[2], 1.0);
			}
			else {
				yield return new ColorRGBA(r, g, b, 1.0);
			}
		}
	}

	// https://en.wikipedia.org/wiki/Wikipedia:Featured_picture_candidates/All_24-bit_RGB_colors
	IEnumerable<ColorRGBA> Spiral16(ICanvas image, ProgressBar progress)
	{
		//split by 16x16 squares
		//split by row
		//split by col
		const int componentCount = 3;
		int total = Math.Min(image.Width * image.Height, NumberOfColors);
		for(int c = 0; c < total; c++) {
			int ic = (c + O.ColorOffset) % int.MaxValue;
			int ig = ic / 65536;
			int ir = ic % 4096 / 16;

			int x = ic % image.Width;
			int y = ic / image.Width;

			int sx = x % 16 + x / 16;
			int sy = y % 16 + y / 16;
			long ib = 255 - PlugMath.XYToSpiralSquare(sx, sy, x / 16 + 7, y / 16 + 8);
			double r = ir / 255.0, g = ig / 255.0, b = ib / 255.0;

			if(O.Order != null) {
				var order = O.GetFixedOrder(componentCount);
				var items = new[] { r, g, b };
				Array.Sort(order, items);
				yield return new ColorRGBA(items[0], items[1], items[2], 1.0);
			}
			else {
				yield return new ColorRGBA(r, g, b, 1.0);
			}
			progress?.Report((double)c / total);
		}
	}

	IEnumerable<ColorRGBA> Spiral4kBuckets(ICanvas image, ProgressBar progress)
	{
		//split by buckets then arrange in a spiral
		const int componentCount = 3;
		int total = Math.Min(image.Width * image.Height, NumberOfColors);
		int cx = 2047;
		int cy = 2048;

		uint[] bucket = new uint[256];

		for(int i = 0; i < total; i++) {
			int ic = (i + O.ColorOffset) % int.MaxValue;

			int sx = ic % 4096;
			int sy = ic / 4096;
			long pos = PlugMath.XYToSpiralSquare(sx, sy, cx, cy);

			int index = (int)(pos / 65536);
			uint count = bucket[index];
			double r = count % 256 / 255.0;
			double g = count / 256 / 255.0;
			double b = (255 - pos / 65536) / 255.0;
			bucket[index]++;

			if(O.Order != null) {
				var order = O.GetFixedOrder(componentCount);
				var items = new[] { r, g, b };
				Array.Sort(order, items);
				yield return new ColorRGBA(items[0], items[1], items[2], 1.0);
			}
			else {
				yield return new ColorRGBA(r, g, b, 1.0);
			}
			progress?.Report((double)i / total);
		}
	}

	IEnumerable<ColorRGBA> Spiral4k(ICanvas image, ProgressBar progress)
	{
		// split color by bit pattern then arrange in a spiral
		const int componentCount = 3;
		int total = Math.Min(image.Width * image.Height, NumberOfColors);
		int cx = 2047;
		int cy = 2048;

		for(int i = 0; i < total; i++) {
			int ic = (i + O.ColorOffset) % int.MaxValue;

			int sx = ic % 4096;
			int sy = ic / 4096;
			long pos = PlugMath.XYToSpiralSquare(sx, sy, cx, cy);

			double r = ((pos >> 00) & 255) / 255.0;
			double g = ((pos >> 08) & 255) / 255.0;
			double b = ((pos >> 16) & 255) / 255.0;

			if(O.Order != null) {
				var order = O.GetFixedOrder(componentCount);
				var items = new[] { r, g, b };
				Array.Sort(order, items);
				yield return new ColorRGBA(items[0], items[1], items[2], 1.0);
			}
			else {
				yield return new ColorRGBA(r, g, b, 1.0);
			}
			progress?.Report((double)i / total);
		}
	}

	// https://en.wikipedia.org/wiki/Wikipedia:Featured_picture_candidates/All_24-bit_RGB_colors
	IEnumerable<ColorRGBA> Squares4k(ICanvas image, ProgressBar progress)
	{
		//split by 256x256 squares (256 of them)
		//split by rows
		//split by cols

		const int componentCount = 3;
		int total = Math.Min(image.Width * image.Height, NumberOfColors);
		for(int c = 0; c < total; c++) {
			int ic = (c + O.ColorOffset) % int.MaxValue;
			int x = ic % image.Width;
			int y = ic / image.Width;

			int ib = 16 * (y / 256) + (x /256);
			int ir = ic % 256;
			int ig = ic / 4096 % 256;

			//Log.Debug($"ic={ic} x={x} y={y} ig={ig} gx={x/256} gy={y/256}");

			double r = ir / 255.0, g = ig / 255.0, b = ib / 255.0;

			if(O.Order != null) {
				var order = O.GetFixedOrder(componentCount);
				var items = new[] { r, g, b };
				Array.Sort(order, items);
				yield return new ColorRGBA(items[0], items[1], items[2], 1.0);
			}
			else {
				yield return new ColorRGBA(r, g, b, 1.0);
			}
			progress?.Report((double)c / total);
		}
	}


	List<ColorRGBA> ConvertAndSort<T>(ICanvas image, Func<ColorRGBA, T> conv, Func<T, T, int>[] compList, ProgressBar progress)
	{
		var colorList = PatternBitOrder(image, progress).ToList();
		var tempList = new List<(ColorRGBA, T)>(colorList.Count);

		if(O.Order != null) {
			var fixedOrder = O.GetFixedOrder(compList.Length);
			//sort compList using order as the guide
			Array.Sort(fixedOrder, compList);
		}

		for(int t = 0; t < colorList.Count; t++) {
			var c = colorList[t];
			T next = conv(c);
			tempList.Add((c, next));
			progress.Report((double)t / colorList.Count);
		}

		progress.Prefix = "Sorting... ";
		int count = 0;
		var progressSorter = new Comparison<(ColorRGBA, T)>((a, b) => {
			count++;
			progress.Report(count / SortMax);
			return MultiSort(compList, a.Item2, b.Item2);
		});

		if(O.ParallelSort) {
			//parallel version seems to works best on 4+ cores
			var comp = Comparer<(ColorRGBA, T)>.Create(
				new Comparison<(ColorRGBA, T)>((a, b) => {
					return MultiSort(compList, a.Item2, b.Item2);
				})
			);
			MoreAide.ParallelSort(tempList, comp, progress, CoreOptions.MaxDegreeOfParallelism);
		}
		else {
			//seems to be a lot faster than Array.Sort(key,collection)
			//single threaded version for machines with a low number of cores
			tempList.Sort(progressSorter);
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

	static Func<ColorRGBA, IColor3> GetConverter(IColor3Space space)
	{
		var c = new Converter3Helper() { Space = space };
		return c.Convert;
	}

	class Converter3Helper
	{
		public IColor3Space Space;
		public IColor3 Convert(ColorRGBA c)
		{
			return Space.ToSpace(c);
		}
	}

	static Func<ColorRGBA, IColor4> GetConverter(IColor4Space space)
	{
		var c = new Converter4Helper() { Space = space };
		return c.Convert;
	}

	class Converter4Helper
	{
		public IColor4Space Space;
		public IColor4 Convert(ColorRGBA c)
		{
			return Space.ToSpace(c);
		}
	}

	static Func<double, double, int>[] ComparersLuminance()
	{
		var arr = new Func<double, double, int>[] {
			(a,b) => a > b ? 1 : a < b ? -1 : 0,
		};
		return arr;
	}

	static double ConvertRGBA(ColorRGBA c)
	{
		return 1.0;
	}

	// https://en.wikipedia.org/wiki/Rec._709
	static double ConvertLuminance709(ColorRGBA c)
	{
		double l = 0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B;
		return l;
	}

	// https://en.wikipedia.org/wiki/YCbCr#ITU-R_BT.601_conversion
	// https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness
	// https://en.wikipedia.org/wiki/Rec._601
	static double ConvertLuminance601(ColorRGBA c)
	{
		double l = 0.2989 * c.R + 0.5870 * c.G + 0.1140 * c.B;
		return l;
	}

	// https://en.wikipedia.org/wiki/Rec._2020
	static double ConvertLuminance2020(ColorRGBA c)
	{
		double l = 0.2627 * c.R + 0.6780 * c.G + 0.0593 * c.B;
		return l;
	}

	// http://www.w3.org/TR/AERT#color-contrast
	static double ConvertAERT(ColorRGBA c)
	{
		double l = 0.2990 * c.R + 0.5870 * c.G + 0.1140 * c.B;
		return l;
	}

	// http://alienryderflex.com/hsp.html
	static double ConvertHSP(ColorRGBA c)
	{
		double rr = c.R * c.R, gg = c.G * c.G, bb = c.B * c.B;
		double l = 0.2990 * rr + 0.5870 * gg + 0.1140 * bb;
		return Math.Sqrt(l);
	}

	// http://www.w3.org/TR/WCAG20/#relativeluminancedef
	static double ConvertWCAG2(ColorRGBA c)
	{
		double l =
			  0.2126 * WCAG2Normalize(c.R)
			+ 0.7152 * WCAG2Normalize(c.G)
			+ 0.0722 * WCAG2Normalize(c.B);
		return l;
	}
	static double WCAG2Normalize(double val)
	{
		double c = val <= 0.03928
			? val / 12.92
			: Math.Pow((val + 0.055) / 1.055, 2.4)
		;
		return c;
	}

	// http://discoverybiz.net/enu0/faq/faq_YUV_YCbCr_YPbPr.html
	static double ConvertSmpte1999(ColorRGBA c)
	{
		double l = 0.2120 * c.R + 0.7010 * c.G + 0.0870 * c.B;
		return l;
	}

	static Func<ColorRGBA, ColorRGBA, int>[] CompareColorRGBA()
	{
		var arr = new Func<ColorRGBA, ColorRGBA, int>[] {
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

	static Func<(double, double, double), (double, double, double), int>[] ComparersHsi()
	{
		var arr = new Func<(double, double, double), (double, double, double), int>[] {
			(a,b) => a.Item1 > b.Item1 ? 1 : a.Item1 < b.Item1 ? -1 : 0,
			(a,b) => a.Item2 > b.Item2 ? 1 : a.Item2 < b.Item2 ? -1 : 0,
			(a,b) => a.Item3 > b.Item3 ? 1 : a.Item3 < b.Item3 ? -1 : 0,
		};
		return arr;
	}
}
