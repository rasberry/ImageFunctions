using System;
using System.Text;

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

			if (ShowHelpActions) {
				sb.AppendLine().AppendLine(" Actions:");
				foreach(Action a in Enum.GetValues(typeof(Action))) {
					if (a == Action.None) { continue; }
					sb.AppendLine("  "+a);
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
			}
			else if (action != Action.None)
			{
				IFunction func = Registry.Map(action);
				func.Usage(sb);
			}

			Log.Message(sb.ToString());
		}

		public static bool Parse(string[] args)
		{
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
				else if (curr == "--actions") {
					ShowHelpActions = true;
				}
				else if (Which == Action.None) {
					Action which;
					if (!Enum.TryParse<Action>(curr,true,out which)) {
						Log.Error("unkown action \""+curr+"\"");
						return false;
					}
					Which = which;
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
			return true;
		}

		public static Action Which { get; private set; } = Action.None;
		static bool ShowFullHelp = false;
		static bool ShowHelpActions = false;
		static bool ShowActionHelp = false;
	}
}