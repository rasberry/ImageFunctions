using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Linq;
using SixLabors.Primitives;

namespace ImageFunctions.Helpers
{
	public static class OptionsHelpers
	{
		public static void SamplerHelpLine(this System.Text.StringBuilder sb)
		{
			sb.AppendLine(" --sampler (name)            Use given sampler (defaults to nearest pixel)");
		}

		public static bool HasSamplerArg(string[] args, ref int a)
		{
			return args[a] == "--sampler" && ++a < args.Length;
		}

		public static bool TryParseSampler(string[] args, ref int a, out IResampler sampler)
		{
			sampler = null;
			Sampler which;
			if (!OptionsHelpers.TryParse<Sampler>(args[a],out which)) {
				Log.Error("unknown sampler \""+args[a]+"\"");
				return false;
			}
			sampler = Registry.Map(which);
			return true;
		}

		public static void MetricHelpLine(this System.Text.StringBuilder sb)
		{
			sb.AppendLine(" --metric (name) [args]      Use alterntive distance function");
		}

		public static bool HasMetricArg(string[] args, ref int a)
		{
			return args[a] == "--metric" && ++a < args.Length;
		}

		public static bool TryParseMetric(string[] args, ref int a, out IMeasurer mf)
		{
			mf = null;
			Metric which;
			if (!OptionsHelpers.TryParse<Metric>(args[a],out which)) {
				Log.Error("unknown metric \""+args[a]+"\"");
				return false;
			}
			if (which == Metric.Minkowski && ++a < args.Length) {
				if (!double.TryParse(args[a],out double p)) {
					Log.Error("could not parse p-factor");
					return false;
				}
				mf = Registry.Map(which,p);
			}
			else {
				mf = Registry.Map(which);
			}
			return true;
		}

		public static string FunctionName(Activity a)
		{
			return $"{((int)a)}. {a}";
		}

		public static string CreateOutputFileName(string input)
		{
			//string ex = Path.GetExtension(input);
			string name = Path.GetFileNameWithoutExtension(input);
			string outFile = $"{name}-{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.png";
			return outFile;
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

		public static bool TryParse<V>(string sub, out V val)
		{
			val = default(V);
			Type t = typeof(V);

			if (t.IsEnum) {
				if (Enum.TryParse(t,sub,true,out object o)) {
					val = (V)o;
					return Enum.IsDefined(t,o);
				}
			}
			else if (t.Equals(typeof(double))) {
				if (double.TryParse(sub,out double b)) {
					val = (V)((object)b); return true;
				}
			}
			else if (t.Equals(typeof(int))) {
				if (int.TryParse(sub,out int b)) {
					val = (V)((object)b); return true;
				}
			}
			else if (t.Equals(typeof(string))) {
				val = (V)((object)sub); return true;
			}
			else if (t.Equals(typeof(Rectangle))) {
				bool y = TryParseRectangle(sub,out var rect);
				if (y) { val = (V)((object)rect); }
				return y;
			}
			return false;
		}

		public static bool TryParseColor(string sub, out Color color)
		{
			color = default(Color);
			try {
				color = Color.FromHex(sub);
				return true;
			}
			catch(ArgumentException) {
				//Continue
			}

			PopColorMap();

			if (ColorMap.TryGetValue(sub,out string ColorName)) {
				if (TryGetColorByName(ColorName, out color)) {
					return true;
				}
			}

			return false;
		}

		static HashSet<string> ColorMap = null;
		static void PopColorMap()
		{
			if (ColorMap != null) { return; }
			ColorMap = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
			var flags = BindingFlags.Public | BindingFlags.Static;
			Type colorType = typeof(Color);
			var fields = colorType.GetFields(flags);

			foreach(var info in fields) {
				if (colorType.Equals(info.FieldType)) {
					ColorMap.Add(info.Name);
				}
			}
		}

		static bool TryGetColorByName(string name, out Color color)
		{
			color = default(Color);
			var flags = BindingFlags.Public | BindingFlags.Static;
			Type colorType = typeof(Color);
			var field = colorType.GetField(name,flags);
			if (field == null) { return false; }
			color = (Color)field.GetValue(null);
			return true;
		}

		public static IEnumerable<(string,Color)> AllColors()
		{
			PopColorMap();
			foreach(string name in ColorMap) {
				if (!TryGetColorByName(name,out Color color)) { continue; }
				yield return (name,color);
			}
		}

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

		//args = [x,y,]w,h
		//Note: negative width and height are allowed to allow w,h to be used as a point
		static bool TryParseRectangle(string arg, out Rectangle rect)
		{
			rect = Rectangle.Empty;
			if (String.IsNullOrWhiteSpace(arg)) { return false; }

			var parts = arg.Split(new char[] { ' ',',','x' },
				StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2 && parts.Length != 4) {
				return false; //must be 2 or 4 numbers
			}
			bool isTwo = parts.Length == 2;
			for(int p=0; p<parts.Length; p++) {
				if (!int.TryParse(parts[p],out int n)) {
					return false; //we only like numbers
				}
				switch(p + (isTwo ? 2 : 0)) {
				case 0: rect.X = n; break;
				case 1: rect.Y = n; break;
				case 2: rect.Width = n; break;
				case 3: rect.Height = n; break;
				}
			}

			//sanity check
			if (rect.Height < 1 || rect.Width < 1 || rect.X < 0 || rect.Y < 0) { return false; }
			return true;
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

			//we always want a newline even if m is emptys
			if (l < 1) {
				self.AppendLine();
			}

			//StringBuilder likes to chain
			return self;
		}

	}
}
