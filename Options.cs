using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions
{
	public static class Options
	{
		public static void Usage(Action action = Action.None)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Usage "+nameof(ImageFunctions)+" (action) [options]");
			sb.AppendLine(" -h / --help                 Show full help");
			sb.AppendLine(" (action) -h                 Action specific help");
			sb.AppendLine(" --actions                   List possible actions");
			sb.AppendLine(" -# / --rect (x,y,w,h)       Apply function to given rectagular area (defaults to entire image)");
			sb.AppendLine(" --max-threads (number)      Restrict parallel processing to a given number of threads (defaults to # of cores)");

			if (ShowHelpActions) {
				sb.AppendLine().AppendLine("Actions:");
				foreach(Action a in Enum.GetValues(typeof(Action))) {
					if (a == Action.None) { continue; }
					sb.AppendLine(((int)a)+". "+a);
				}
			}

			if (ShowFullHelp)
			{
				foreach(Action a in Enum.GetValues(typeof(Action)))
				{
					if (a == Action.None) { continue; }
					IFunction func = Registry.Map(a);
					func.Usage(sb);
				}
				SamplerHelp(sb);
				MetricHelp(sb);
			}
			else if (action != Action.None)
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

			Log.Message(sb.ToString());
		}

		static void SamplerHelp(StringBuilder sb)
		{
			sb.AppendLine();
			sb.AppendLine("Available Samplers:");
			foreach(Sampler s in Enum.GetValues(typeof(Sampler))) {
				if (s == Sampler.None) { continue; }
				sb.AppendLine(((int)s)+". "+s);
			}
		}

		static void MetricHelp(StringBuilder sb)
		{
			sb.AppendLine();
			sb.AppendLine("Available Metrics:");
			foreach(Metric s in Enum.GetValues(typeof(Metric))) {
				if (s == Metric.None) { continue; }
				string txt = ((int)s)+". "+s+
					(s == Metric.Minkowski ? " (p-factor)" : "")
				;
				sb.AppendLine(txt);
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
					if (Which == Action.None) {
						ShowFullHelp = true;
					}
					else {
						ShowActionHelp = true;
					}
				}
				else if ((curr == "-#" || curr == "--rect") && ++a < len) {
					var parts = args[a].Split(new char[] { ',','x' },
						StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length < 4) {
						Log.Error("rectangle must contain four numbers");
						return false;
					}
					for(int p=0; p<4; p++) {
						if (!int.TryParse(parts[p],out int n)) {
							Log.Error("could not parse \""+parts[p]+"\" as a number");
							return false;
						}
						switch(p) {
						case 0: _Rect.X = n; break;
						case 1: _Rect.Y = n; break;
						case 2: _Rect.Width = n; break;
						case 3: _Rect.Height = n; break;
						}
					}
					// Log.Debug("rect = ["+Rect.X+","+Rect.Y+","+Rect.Width+","+Rect.Height+"]");
				}
				else if (curr == "--actions") {
					ShowHelpActions = true;
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
				else if (Which == Action.None) {
					Action which;
					if (!OptionsHelpers.TryParse<Action>(curr,out which)) {
						Log.Error("unkown action \""+curr+"\"");
						return false;
					}
					Which = which;
				}
				else {
					pArgs.Add(curr);
				}
			}

			if (ShowFullHelp || ShowHelpActions || ShowActionHelp) {
				Usage(Which);
				return false;
			}

			if (Which == Action.None) {
				Log.Error("action was not specified");
				return false;
			}

			prunedArgs = pArgs.ToArray();
			return true;
		}

		public static Action Which { get; private set; } = Action.None;
		public static Rectangle Rect { get { return _Rect; }}
		public static int? MaxDegreeOfParallelism { get; private set; } = null;
		
		static bool ShowFullHelp = false;
		static bool ShowHelpActions = false;
		static bool ShowActionHelp = false;
		static Rectangle _Rect = Rectangle.Empty;
	}
}