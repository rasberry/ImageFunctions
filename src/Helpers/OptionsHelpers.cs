using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Globalization;

namespace ImageFunctions.Helpers
{
	public static class OptionsHelpers
	{
		//in case you might need to include a custom parser
		public delegate bool Parser<T>(string inp, out T val);

		public static void SamplerHelpLine(this StringBuilder sb)
		{
			sb.WL(1,"--sampler (name)","Use given sampler (defaults to nearest pixel)");
		}

		public static Params.Result DefaultSampler(this Params p, out ISampler s, ISampler def = null)
		{
			s = def ?? Registry.DefaultIFResampler;
			var r = p.Default("--sampler",out Sampler sam);
			if (r.IsBad()) {
				return r;
			}
			s = Registry.Map(sam);
			return r;
		}

		public static void MetricHelpLine(this System.Text.StringBuilder sb)
		{
			sb.WL(1,"--metric (name) [args]","Use alternative distance function");
		}

		public static Params.Result DefaultMetric(this Params p, out IMeasurer m, IMeasurer def = null)
		{
			m = def ?? Registry.DefaultMetric;
			Func<Metric,bool> hasTwo = (Metric mm) => mm == Metric.Minkowski;

			var r = p.Default("--metric",out Metric metric,out double pfactor,Metric.None,0.0,hasTwo);
			if (r.IsGood()) {
				if (hasTwo(metric)) {
					m = Registry.Map(metric,pfactor);
				}
				else {
					m = Registry.Map(metric);
				}
			}
			return r;
		}

		public static Params.Result ExpectFile(this Params p, out string fileName, string name)
		{
			var r = p.Expect(out fileName, name);
			if (r.IsBad()) { return r; }

			if (!File.Exists(fileName)) {
				Tell.CannotFindFile(fileName);
				return Params.Result.Invalid;
			}
			return Params.Result.Good;
		}

		public static Params.Result DefaultFile(this Params p,out string fileName, string template)
		{
			//defaulting to png since it's a good general-purpose format
			string noext = Path.GetFileNameWithoutExtension(template);
			template = $"{noext}.{Options.ImageFormat ?? "png"}";

			if (p.Default(out fileName).IsBad()) {
				fileName = CreateOutputFileName(template);
			}
			return Params.Result.Good;
		}

		public static Params.Result BeGreaterThanZero<T>(this Params.Result r, string name, T val, bool includeZero = false)
		{
			if (r.IsBad()) { return r; }

			var t = typeof(T);
			var nullType = Nullable.GetUnderlyingType(t);
			if (nullType != null) { t = nullType; }
			bool isInvalid = false;

			if (t.Equals(typeof(double))) {
				double v = (double)((object)val);
				if ((!includeZero && v >= double.Epsilon)
					|| (includeZero && v >= 0.0)) {
					return Params.Result.Good;
				}
				isInvalid = true;
			}
			else if (t.Equals(typeof(int))) {
				int v = (int)((object)val);
				if ((!includeZero && v > 0)
					|| (includeZero && v >= 0)) {
					return Params.Result.Good;
				}
				isInvalid = true;
			}

			if (isInvalid) {
				Tell.MustBeGreaterThanZero(name,includeZero);
				return Params.Result.Invalid;
			}
			else {
				throw new NotSupportedException($"Type {t?.Name} is not supported by {nameof(BeGreaterThanZero)}");
			}
		}

		public static Params.Result BeSizeInBytes(this Params.Result r, string name,
			byte[] val, int sizeInBytes, bool isMin = false)
		{
			if (r.IsGood() && val != null && val.Length < sizeInBytes) {
				Tell.MustBeSizeInBytes(name,sizeInBytes,isMin);
				return Params.Result.Invalid;
			}
			return r;
		}

		public static string FunctionName(Activity a)
		{
			return $"{((int)a)}. {a}";
		}

		public static string CreateOutputFileName(string input)
		{
			string ext = (Path.GetExtension(input) ?? "").ToLowerInvariant();
			string name = Path.GetFileNameWithoutExtension(input);
			string outFile = $"{name}-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}{ext}";
			return outFile;
		}

		public static bool ParseNumberPercent(string num, out double? val)
		{
			val = null;
			bool worked = ParseNumberPercent(num,out double v);
			if (worked) { val = v; }
			return worked;
		}

		public static bool ParseNumberPercent(string num, out double val)
		{
			val = 0.0;
			bool isPercent = false;
			if (num.EndsWith('%')) {
				isPercent = true;
				num = num.Remove(num.Length - 1);
			}
			if (!double.TryParse(num, out double d)) {
				Log.Error("could not parse \""+num+"\" as a number");
				return false;
			}
			if (!double.IsFinite(d)) {
				Log.Error("invalid number \""+d+"\"");
				return false;
			}
			val = isPercent ? d/100.0 : d;
			return true;
		}

		static IFormatProvider ifp = CultureInfo.InvariantCulture.NumberFormat;
		public static bool TryParse<V>(string sub, out V val)
		{
			val = default(V);
			Type t = typeof(V);

			var nullType = Nullable.GetUnderlyingType(t);
			if (nullType != null) { t = nullType; }

			if (t.IsEnum) {
				if (Enum.TryParse(t,sub,true,out object o)) {
					val = (V)o;
					return Enum.IsDefined(t,o);
				}
			}
			else if (t.Equals(typeof(double))) {
				if (double.TryParse(sub,NumberStyles.Any,ifp,out double b)) {
					if (!double.IsInfinity(b) && !double.IsNaN(b)) {
						val = (V)((object)b);
						return true;
					}
				}
			}
			else if (t.Equals(typeof(int))) {
				if (int.TryParse(sub,NumberStyles.Any,ifp,out int b)) {
					val = (V)((object)b); return true;
				}
			}
			else if (t.Equals(typeof(ulong))) {
				if (ulong.TryParse(sub,NumberStyles.Any,ifp,out ulong b)) {
					val = (V)((object)b); return true;
				}
			}
			else if (t.Equals(typeof(string))) {
				if (!String.IsNullOrWhiteSpace(sub)) {
					val = (V)((object)sub);
					return true;
				}
			}
			else if (t.Equals(typeof(Rectangle))) {
				if (TryParseRectangle(sub,out var rect)) {
					val = (V)((object)rect); return true;
				}
			}
			else if (t.Equals(typeof(Point))) {
				if (TryParsePoint(sub,out var point)) {
					val = (V)((object)point); return true;
				}
			}
			else if (t.Equals(typeof(Color))) {
				if (TryParseColor(sub,out var clr)) {
					val = (V)((object)clr); return true;
				}
			}
			else if (t.Equals(typeof(IColor))) {
				if (TryParseColor(sub,out var clr)) {
					var ntv = ImageHelpers.RgbaToNative(clr);
					val = (V)((object)ntv); return true;
				}
			}

			return false;
		}

		public static bool TryParseColor(string sub, out Color color)
		{
			var try1 = ColorHelpers.FromName(sub);
			if (try1.HasValue) {
				color = try1.Value;
				return true;
			}

			try {
				color = ColorHelpers.FromHex(sub);
				return true;
			}
			//don't crash here - follow the convention of returning false
			catch(ArgumentException) {}
			catch(FormatException) {}

			color = Color.Transparent;
			return false;
		}

		//static HashSet<string> ColorMap = null;
		//static void PopColorMap()
		//{
		//	if (ColorMap != null) { return; }
		//	ColorMap = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
		//	var flags = BindingFlags.Public | BindingFlags.Static;
		//	Type colorType = typeof(Color);
		//	var fields = colorType.GetFields(flags);
		//
		//	foreach(var info in fields) {
		//		if (colorType.Equals(info.FieldType)) {
		//			ColorMap.Add(info.Name);
		//		}
		//	}
		//}

		// //TODO this is broken now
		// static bool TryGetColorByName(string name, out IFColor color)
		// {
		// 	color = ColorHelpers.Transparent;
		// 	//var flags = BindingFlags.Public | BindingFlags.Static;
		// 	//Type colorType = typeof(Color);
		// 	//var field = colorType.GetField(name,flags);
		// 	//if (field == null) { return false; }
		// 	//color = (Color)field.GetValue(null);
		// 	return true;
		// }

		// public static IEnumerable<(string,IFColor)> AllColors()
		// {
		// 	PopColorMap();
		// 	foreach(string name in ColorMap) {
		// 		if (!TryGetColorByName(name,out IFColor color)) { continue; }
		// 		yield return (name,color);
		// 	}
		// }

		public static IEnumerable<T> EnumAll<T>(bool includeZero = false)
			where T : struct
		{
			foreach(T a in Enum.GetValues(typeof(T))) {
				int v = (int)((object)a);
				if (!includeZero && v == 0) { continue; }
				yield return a;
			};
		}

		public static void PrintEnum<T>(this StringBuilder sb, int level = 0, Func<T,string> descriptionMap = null,
			Func<T,string> nameMap = null) where T : struct
		{
			var allEnums = EnumAll<T>().ToList();
			int numLen = 1 + (int)Math.Floor(Math.Log10(allEnums.Count));
			foreach(T e in allEnums) {
				int inum = (int)((object)e);
				string pnum = inum.ToString();
				int lpad = pnum.Length < numLen ? numLen - pnum.Length : 0;
				string npad = new string(' ',lpad);
				string pname = nameMap == null ? e.ToString() : nameMap(e);
				string pdsc = descriptionMap == null ? "" : descriptionMap(e);
				sb.WL(level,$"{npad}{pnum}. {pname}",pdsc);
			}
		}

		static char[] RectPointDelims = new char[] { ' ',',','x' };

		//args = [x,y,]w,h
		static bool TryParseRectangle(string arg, out Rectangle rect)
		{
			//Log.Debug($"Parsing Rectangle {arg}");
			rect = Rectangle.Empty;

			var parser = new Parser<int>(int.TryParse);
			if (!TryParseSequence(arg,RectPointDelims,out var list,parser)) {
				return false;
			}
			if (list.Count != 2 && list.Count != 4) { //must be two or four elements w,h / x,y,w,h
				return false;
			}

			if (list.Count == 2) {
				rect = new Rectangle(0,0,list[0],list[1]);
			}
			else {
				rect = new Rectangle(list[0],list[1],list[2],list[3]);
			}

			//sanity check
			if (rect.Height <= 0 || rect.Width <= 0 || rect.X < 0 || rect.Y < 0) { return false; }

			return true;
		}

		public static bool TryParsePoint(string arg, out Point point)
		{
			point = Point.Empty;
			var parser = new Parser<int>(int.TryParse);
			if (!TryParseSequence(arg,RectPointDelims,out var list,parser)) {
				return false;
			}
			if (list.Count != 2) { //must be two elements x,y
				return false;
			}
			point = new Point(list[0],list[1]);
			return true;
		}

		public static bool TryParseSequence<T>(string arg, char[] delimiters,
			out IReadOnlyList<T> seq, Parser<T> parser = null)
		{
			seq = null;
			if (String.IsNullOrWhiteSpace(arg)) { return false; }

			if (parser == null) { parser = TryParse; }
			var parts = arg.Split(delimiters,StringSplitOptions.RemoveEmptyEntries);
			var list = new List<T>();
			for(int p=0; p<parts.Length; p++) {
				if (!parser(parts[p], out T n)) {
					return false; //not able to parse as the underlying type
				}
				list.Add(n);
			}
			seq = list;
			return true;
		}

		public static bool TryParseEnumFirstLetter<T>(string arg, out T val) where T : struct
		{
			bool worked = Enum.TryParse<T>(arg,true,out val);
			//try to match the first letter if normal enum parse fails
			if (!worked) {
				string f = arg.Substring(0,1);
				foreach(T e in Enum.GetValues(typeof(T))) {
					string name = e.ToString();
					if (name.Equals("none",StringComparison.OrdinalIgnoreCase)) {
						continue;
					}
					string n = name.Substring(0,1);
					if (f.Equals(n,StringComparison.OrdinalIgnoreCase)) {
						val = e;
						return true;
					}
				}
			}
			return worked;
		}

		const int ColumnOffset = 30;
		public static StringBuilder WL(this StringBuilder sb, int level, string def, string desc)
		{
			int pad = level;
			return sb
				.Append(' ',pad)
				.Append(def)
				.Append(' ',ColumnOffset - def.Length - pad)
				.AppendWrap(ColumnOffset,desc);
		}

		public static StringBuilder WL(this StringBuilder sb, int level, string def)
		{
			int pad = level;
			return sb
				.Append(' ',pad)
				.AppendWrap(pad,def);
		}

		public static StringBuilder WL(this StringBuilder sb, string s = null)
		{
			return s == null ? sb.AppendLine() : sb.AppendWrap(0,s);
		}

		public static StringBuilder AppendWrap(this StringBuilder self, int offset, string m)
		{
			int w = Console.IsOutputRedirected
				? int.MaxValue
				: Console.BufferWidth - 1 - offset
			;
			int c = 0;
			int l = m.Length;

			while(c < l) {
				//need spacing after first line
				string o = c > 0 ? new string(' ',offset) : "";
				//this is the last iteration
				if (c + w >= l) {
					string s = m.Substring(c);
					c += w;
					self.Append(o).AppendLine(s);
				}
				//we're in the middle
				else {
					string s = m.Substring(c,w);
					c += w;
					self.Append(o).AppendLine(s);
				}
			}

			//we always want a newline even if m is empty
			if (l < 1) {
				self.AppendLine();
			}

			//StringBuilder likes to chain
			return self;
		}

	}
}
