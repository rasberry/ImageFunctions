using System;
using System.Collections.Generic;
using System.Text;
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
			}
			else if (action != Action.None)
			{
				IFunction func = Registry.Map(action);
				func.Usage(sb);
			}

			Log.Message(sb.ToString());
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
						case 0: Rect.X = n; break;
						case 1: Rect.Y = n; break;
						case 2: Rect.Width = n; break;
						case 3: Rect.Height = n; break;
						}
					}
					// Log.Debug("rect = ["+Rect.X+","+Rect.Y+","+Rect.Width+","+Rect.Height+"]");
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
		public static Rectangle Rect = Rectangle.Empty;
		
		static bool ShowFullHelp = false;
		static bool ShowHelpActions = false;
		static bool ShowActionHelp = false;
	}
}