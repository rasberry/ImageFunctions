using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using ImageFunctions.Helpers;

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
				if ((func as IHasSampler) != null) {
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
			sb.PrintEnum<Metric>(0, null, (m) => {
				return m == Metric.Minkowski ? m + " (p-factor)" : m.ToString();
			});
		}

		static void ColorsHelp(StringBuilder sb)
		{
			sb.WL();
			sb.WL(0,"Available Colors:");
			sb.WL(0,"Note: Colors may be specified as a name or as a hex value");
			foreach(Color c in ColorHelpers.AllColors()) {
				sb.WL(0,ColorHelpers.GetColorName(c),c.ToHex());
			}
		}

		public static bool Parse(string[] args, out string[] prunedArgs)
		{
			var p = new Params(args);
			prunedArgs = null;
			Activity w = Activity.None;

			//do an initial check for activity
			p.Default(out w,Activity.None);

			if (p.Has("-h","--help").IsGood()) {
				if (w == Activity.None) {
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

			var orect = p.Default(new string[] { "-#","--rect"},out Rectangle rect);
			if (orect.IsInvalid()) {
				return false;
			}
			else if (orect.IsGood()) {
				Bounds = rect;
			}

			var omtr = p.Default("--max-threads",out int mdop, 0);
			if (omtr.IsInvalid()) {
				return false;
			}
			else if(omtr.IsGood()) {
				if (mdop < 1) {
					Tell.MustBeGreaterThanZero("--max-threads");
					return false;
				}
				MaxDegreeOfParallelism = mdop;
			}

			if (ShowFullHelp || ShowHelpActions || ShowColorList) {
				Usage(w);
				return false;
			}

			if (w == Activity.None) {
				if (p.Expect(out w,"activity").IsBad()) {
					return false;
				}
			}
			Which = w;

			prunedArgs = p.Remaining();
			return true;
		}

		public static Activity Which { get; private set; } = Activity.None;
		public static Rectangle Bounds { get; private set; } = Rectangle.Empty;
		public static int? MaxDegreeOfParallelism { get; private set; } = null;
		public static object OptionHelpers { get; private set; }

		static bool ShowFullHelp = false;
		static bool ShowHelpActions = false;
		static bool ShowColorList = false;
	}
}