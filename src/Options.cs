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
			sb.WL(0,"Usage "+nameof(ImageFunctions)+" (action) [options]");
			sb.WL(1,"-h / --help"            ,"Show full help");
			sb.WL(1,"(action) -h"            ,"Action specific help");
			sb.WL(1,"--actions"              ,"List possible actions");
			sb.WL(1,"-# / --rect ([x,y,]w,h)","Apply function to given rectagular area (defaults to entire image)");
			sb.WL(1,"--max-threads (number)" ,"Restrict parallel processing to a given number of threads (defaults to # of cores)");
			sb.WL(1,"--colors"               ,"List available colors");

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
					sb.WL();
					sb.WL(0,"Actions:");
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
			sb.WL();
			sb.WL(0,"Available Samplers:");
			OptionsHelpers.PrintEnum<Sampler>(sb);
		}

		static void MetricHelp(StringBuilder sb)
		{
			sb.WL();
			sb.WL(0,"Available Metrics:");
			OptionsHelpers.PrintEnum<Metric>(sb, false, null, (m) => {
				return m == Metric.Minkowski ? m + " (p-factor)" : m.ToString();
			});
		}

		static void ColorsHelp(StringBuilder sb)
		{
			sb.WL();
			sb.WL(0,"Note: Colors may be specified as a name or as a hex value");
			sb.WL(0,"Available Colors:");
			foreach(var kvp in OptionsHelpers.AllColors()) {
				string name = kvp.Item1;
				Color color = kvp.Item2;
				sb.WL(0,color.ToHex(),name);
			}
		}

		public static bool Parse(string[] args, out string[] prunedArgs)
		{
			var p = new Params(args);
			prunedArgs = null;
			
			if (p.Has("-h","--help").IsGood()) {
				if (Which == Activity.None) {
					ShowFullHelp = true;
				}
				else {
					ShowHelpActions = true;
				}
			}
			if (p.Has("--actions").IsGood()) {
				ShowHelpActions = true;
			}
			if (p.Has("--colors").IsGood()) {
				ShowColorList = true;
			}
			if (p.Default(new string[] { "-#","--rect"},out Rectangle _Rect).IsInvalid()) {
				return false;
			}
			{
				var mtr = p.Default("--max-threads",out int mdop, 0);
				if (mtr.IsInvalid()) {
					return false;
				}
				else if(mtr.IsGood()) {
					if (mdop < 1) {
						Tell.MaxThreadsGreaterThanZero();
						return false;
					}
					MaxDegreeOfParallelism = mdop;
				}
			}

			prunedArgs = p.Remaining();
			return true;
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
						ShowHelpActions = true;
					}
				}
				else if ((curr == "-#" || curr == "--rect") && ++a < len) {
					if (!OptionsHelpers.TryParseRectangle(args[a],out var rect)) {
						Log.Error($"invalid rectangle '{args[a]}'");
						return false;
					}
					if (rect.Height < 1 || rect.Width < 1 || rect.X < 0 || rect.Y < 0) {
						Log.Error($"invalid rectangle '{args[a]}'");
						return false;
					}
					_Rect = rect;
					Log.Debug(rect.ToString());
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

			if (ShowFullHelp || ShowHelpActions || ShowColorList) {
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
		static bool ShowColorList = false;
		static Rectangle _Rect = Rectangle.Empty;
	}
}