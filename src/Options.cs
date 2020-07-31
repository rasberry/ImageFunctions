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
			string text = GetUsageText(action,WhichShow);
			Log.Message(text);
		}

		public static string GetUsageText(Activity action, PickShow show)
		{
			StringBuilder sb = new StringBuilder();
			sb.WL(0,"Usage "+nameof(ImageFunctions)+" (action) [options]");
			sb.WL(1,"-h / --help"            ,"Show full help");
			sb.WL(1,"(action) -h"            ,"Action specific help");
			sb.WL(1,"-# / --rect ([x,y,]w,h)","Apply function to given rectagular area (defaults to entire image)");
			sb.WL(1,"--format (name)"        ,"Save any output files as specified format");
			sb.WL(1,"--max-threads (number)" ,"Restrict parallel processing to a given number of threads (defaults to # of cores)");
			sb.WL(1,"--engine (name)"        ,"Select image engine (default SixLabors)");
			sb.WL(1,"--actions"              ,"List possible actions");
			sb.WL(1,"--colors"               ,"List available colors");
			sb.WL(1,"--formats"              ,"List output formats");
			sb.WL();
			sb.WL(0,"Available Engines:");
			sb.PrintEnum<PickEngine>(1);

			if (show.HasFlag(PickShow.FullHelp))
			{
				foreach(Activity a in OptionsHelpers.EnumAll<Activity>()) {
					IFunction func = Registry.Map(a);
					func.Usage(sb);
				}
				SamplerHelp(sb);
				MetricHelp(sb);
				ColorsHelp(sb);
				FormatsHelp(sb);
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
				if (show.HasFlag(PickShow.Actions)) {
					sb.WL();
					sb.WL(0,"Available Actions:");
					OptionsHelpers.PrintEnum<Activity>(sb);
				}
				if (show.HasFlag(PickShow.ColorList)) {
					ColorsHelp(sb);
				}
				if(show.HasFlag(PickShow.Formats)) {
					FormatsHelp(sb);
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

		static void FormatsHelp(StringBuilder sb)
		{
			sb.WL();
			sb.WL(0,"Available Formats:");
			sb.WL(0,"Note: Formats are engine specific");
			var guide = Registry.GetFormatGuide();
			foreach(string f in guide.ListFormatNames()) {
				var desc = guide.GetFormatDescription(f);
				sb.WL(0,f,desc ?? "");
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
					WhichShow |= PickShow.FullHelp;
				}
				else {
					WhichShow |= PickShow.Actions;
				}
			}

			//engine selection needs to happen before other engine specific options
			var oeng = p.Default("--engine",out PickEngine eng, PickEngine.SixLabors);
			if (oeng.IsInvalid()) {
				return false;
			}
			else if (oeng.IsGood()) {
				Engine = eng;
			}

			if (p.Has("--actions").IsGood()) {
				WhichShow |= PickShow.Actions;
			}
			if (p.Has("--colors").IsGood()) {
				WhichShow |= PickShow.ColorList;
			}
			if (p.Has("--formats").IsGood()) {
				WhichShow |= PickShow.Formats;
			}

			var orect = p.Default(new string[] { "-#","--rect"},out Rectangle rect);
			Log.Debug($"rect = {rect}");
			if (orect.IsInvalid()) {
				return false;
			}
			else if (orect.IsGood()) {
				Log.Debug($"Set Bounds {rect}");
				Bounds = rect;
			}
			Log.Debug($"Bounds = {Bounds} {rect}");

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

			var ofmt = p.Default("--format",out string fmt);
			if (ofmt.IsInvalid()) {
				return false;
			}
			else if (ofmt.IsGood()) {
				ImageFormat = fmt;
			}

			if (WhichShow != PickShow.None) {
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
		public static PickEngine Engine { get; private set; } = PickEngine.SixLabors;
		public static string ImageFormat { get; private set; } = null;

		static PickShow WhichShow = PickShow.None;

		[Flags]
		public enum PickShow
		{
			None      = 0,
			FullHelp  = 1,
			Actions   = 2,
			ColorList = 4,
			Formats   = 8
		}

	}
}