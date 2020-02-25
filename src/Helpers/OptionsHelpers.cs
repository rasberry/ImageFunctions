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

		//TODO I think i decided against using this for now
		public static bool ParseDelimitedValues<V>(string arg, out V[] items,char sep = ',') where V : IConvertible
		{
			items = null;
			if (arg == null) { return false; }

			string[] tokens = arg.Split(sep,4); //NOTE change 4 to more if needed
			items = new V[tokens.Length];

			for(int i=0; i<tokens.Length; i++) {
				if (TryParse(tokens[i],out V val)) {
					items[i] = val;
				} else {
					items[i] = default(V);
				}
			}
			return true;
		}

		public static string FunctionName(Activity a)
		{
			return ((int)a).ToString() + ". " + a.ToString();
		}

		public static string CreateOutputFileName(string input)
		{
			//string ex = Path.GetExtension(input);
			string name = Path.GetFileNameWithoutExtension(input);
			string outFile = name+"-"+DateTime.Now.ToString("yyyyMMdd-HHmmss")+".png";
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


		public static bool TryParse<V>(string sub, out V val) where V : IConvertible
		{
			val = default(V);
			TypeCode tc = val.GetTypeCode();
			Type t = typeof(V);

			if (t.IsEnum) {
				if (Enum.TryParse(t,sub,true,out object o)) {
					val = (V)o;
					return Enum.IsDefined(t,o);
				}
				return false;
			}

			switch(tc)
			{
			case TypeCode.Double: {
				if (double.TryParse(sub,out double b)) {
					val = (V)((object)b); return true;
				} break;
			}
			case TypeCode.Int32: {
				if (int.TryParse(sub,out int b)) {
					val = (V)((object)b); return true;
				} break;
			}
			//add others as needed
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

		public static void PrintEnum<T>(StringBuilder sb, bool nested = false, Func<T,string> descriptionMap = null,
			Func<T,string> nameMap = null) where T : struct
		{
			var allEnums = EnumAll<T>().ToList();
			int numLen = 1 + (int)Math.Floor(Math.Log10(allEnums.Count));
			foreach(T e in allEnums) {
				int inum = (int)((object)e);
				string pnum = inum.ToString();
				string npad = pnum.Length < numLen ? new string(' ',numLen - pnum.Length) : "";
				if (nested) { npad = " "+npad; }
				string pname = nameMap == null ? e.ToString() : nameMap(e);
				string ppad = new string(' ',(nested ? 24 : 26) - pname.Length);
				string pdsc = descriptionMap == null ? "" : descriptionMap(e);
				sb.AppendLine($"{npad}{pnum}. {pname}{ppad}{pdsc}");
			}
		}

		//args = [x,y,]w,h
		//Note: negative width and height are allowed to allow w,h to be used as a point
		public static bool TryParseRectangle(string arg, out Rectangle rect)
		{
			rect = Rectangle.Empty;
			if (String.IsNullOrWhiteSpace(arg)) { return false; }

			var parts = arg.Split(new char[] { ',','x' },
				StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2 && parts.Length != 4) {
				//Log.Error("rectangle must contain two or four numbers");
				return false;
			}
			bool isTwo = parts.Length == 2;
			for(int p=0; p<parts.Length; p++) {
				if (!int.TryParse(parts[p],out int n)) {
					//Log.Error("could not parse \""+parts[p]+"\" as a number");
					return false;
				}
				switch(p + (isTwo ? 2 : 0)) {
				case 0: rect.X = n; break;
				case 1: rect.Y = n; break;
				case 2: rect.Width = n; break;
				case 3: rect.Height = n; break;
				}
			}
			return true;
		}
	}
}
