using System;
using System.IO;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace ImageFunctions
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
			if (!Enum.TryParse<Sampler>(args[a],true,out which)) {
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

		public static bool TryParseMetric(string[] args, ref int a, out MetricFunction mf)
		{
			mf = null;
			Metric which;
			if (!Enum.TryParse<Metric>(args[a],true,out which)) {
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

		public static string FunctionName(Action a)
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
			switch(tc)
			{
			case TypeCode.Double: {
				double.TryParse(sub,out double b);
				val = (V)((object)b); return true;
			}
			case TypeCode.Int32: {
				int.TryParse(sub,out int b);
				val = (V)((object)b); return true;
			}
			//add others as needed
			}
			return false;
		}
	}
}
