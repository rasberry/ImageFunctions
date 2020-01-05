using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImageFunctions
{
	public static class Options
	{

		public static void Usage(Activity action = Activity.None)
		{
			string text = GetUsageText(action,ShowFullHelp,ShowHelpActions);
			Log.Message(text);
		}

		public static string GetUsageText(Activity action, bool showFull, bool showActions)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Usage "+nameof(ImageFunctions)+" (action) [options]");
			sb.AppendLine(" -h / --help                 Show full help");
			sb.AppendLine(" (action) -h                 Action specific help");
			sb.AppendLine(" --actions                   List possible actions");
			sb.AppendLine(" -# / --rect ([x,y,]w,h)     Apply function to given rectagular area (defaults to entire image)");
			sb.AppendLine(" --max-threads (number)      Restrict parallel processing to a given number of threads (defaults to # of cores)");
			sb.AppendLine(" --colors                    List available colors");

			if (showFull)
			{
				foreach(Activity a in OptionsHelpers.EnumAll<Activity>()) {
					IFunction func = Registry.Map(a);
					func.Usage(sb);
				}
				SamplerHelp(sb);
				MetricHelp(sb);
				ColorsHelp(sb);
			}
			else if (action != Activity.None)
			{
				IFunction func = Registry.Map(action);
				func.Usage(sb);
				if ((func as IHasResampler) != null) {
					SamplerHelp(sb);
				}
				if ((func as IHasDistance) != null) {
					MetricHelp(sb);
				}
			}
			else
			{
				if (showActions) {
					sb.AppendLine().AppendLine("Actions:");
					OptionsHelpers.PrintEnum<Activity>(sb);
				}

				if (ShowColorList) {
					ColorsHelp(sb);
				}
			}

			return sb.ToString();
		}

		static void SamplerHelp(StringBuilder sb)
		{
			sb.AppendLine();
			sb.AppendLine("Available Samplers:");
			OptionsHelpers.PrintEnum<Sampler>(sb);
		}

		static void MetricHelp(StringBuilder sb)
		{
			sb.AppendLine();
			sb.AppendLine("Available Metrics:");
			OptionsHelpers.PrintEnum<Metric>(sb, false, null, (m) => {
				return m == Metric.Minkowski ? m + " (p-factor)" : m.ToString();
			});
		}

		static void ColorsHelp(StringBuilder sb)
		{
			sb.AppendLine();
			sb.AppendLine("Note: Colors may be specified as a name or as a hex value");
			sb.AppendLine("Available Colors:");
			foreach(var kvp in OptionsHelpers.AllColors()) {
				string name = kvp.Item1;
				Color color = kvp.Item2;
				sb.AppendLine($"{color.ToHex()}  {name}");
			}
		}

		public static bool Parse(string[] args, out string[] prunedArgs)
		{
			prunedArgs = null;
			var pArgs = new List<string>();

			int len = args.Length;
			for(int a=0; a<len; a++) {
				string curr = args[a];
				if (curr == "-h" || curr == "--help") {
					if (Which == Activity.None) {
						ShowFullHelp = true;
					}
					else {
						ShowActionHelp = true;
					}
				}
				else if ((curr == "-#" || curr == "--rect") && ++a < len) {
					var parts = args[a].Split(new char[] { ',','x' },
						StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length != 2 && parts.Length != 4) {
						Log.Error("rectangle must contain two or four numbers");
						return false;
					}
					bool isTwo = parts.Length == 2;
					for(int p=0; p<parts.Length; p++) {
						if (!int.TryParse(parts[p],out int n)) {
							Log.Error("could not parse \""+parts[p]+"\" as a number");
							return false;
						}
						switch(p + (isTwo ? 2 : 0)) {
						case 0: _Rect.X = n; break;
						case 1: _Rect.Y = n; break;
						case 2: _Rect.Width = n; break;
						case 3: _Rect.Height = n; break;
						}
					}
					Log.Debug(_Rect.ToString());
				}
				else if (curr == "--actions") {
					ShowHelpActions = true;
				}
				else if (curr == "--colors") {
					ShowColorList = true;
				}
				else if (curr == "--max-threads" && ++a < len) {
					if (!int.TryParse(args[a],out int num)) {
						Log.Error("Could not parse "+args[a]);
						return false;
					}
					if (num < 1) {
						Log.Error("max-threads must be greater than zero");
						return false;
					}
					MaxDegreeOfParallelism = num;
				}
				else if (Which == Activity.None) {
					Activity which;
					if (!OptionsHelpers.TryParse<Activity>(curr,out which)) {
						Log.Error("unkown action \""+curr+"\"");
						return false;
					}
					Which = which;
				}
				else {
					pArgs.Add(curr);
				}
			}

			if (ShowFullHelp || ShowHelpActions || ShowActionHelp || ShowColorList) {
				Usage(Which);
				return false;
			}

			if (Which == Activity.None) {
				Log.Error("action was not specified");
				return false;
			}

			prunedArgs = pArgs.ToArray();
			return true;
		}

		public static Activity Which { get; private set; } = Activity.None;
		public static Rectangle Bounds { get { return _Rect; }}
		public static int? MaxDegreeOfParallelism { get; private set; } = null;
		public static object OptionHelpers { get; private set; }

		static bool ShowFullHelp = false;
		static bool ShowHelpActions = false;
		static bool ShowActionHelp = false;
		static bool ShowColorList = false;
		static Rectangle _Rect = Rectangle.Empty;
	}
}