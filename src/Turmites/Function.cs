using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions.Turmites
{
	public class Function : AbstractFunction, IGenerator
	{
		public Size StartingSize { get {
			return new Size(1024,1024);
		}}

		public override bool ParseArgs(string[] args)
		{
			var p = new Params(args);
			if (p.Default("-p",out O.Sequence, DefaultSeq(), ParsePattern).IsInvalid()) {
				return false;
			}
			if (p.Default("-img", out InImage).IsInvalid()) {
				return false;
			}
			if (p.Default("-e", out O.EdgeRule, PickEdgeRule.Wrap).IsInvalid()) {
				return false;
			}
			if (p.Default("-s", out O.Start, null).IsInvalid()) {
				return false;
			}
			if (p.Default("-i", out O.Iterations, 1000ul).IsInvalid()) {
				return false;
			}
			if (p.DefaultFile(out OutImage,nameof(Turmites)).IsBad()) {
				return false;
			}

			return true;
		}

		public override void Usage(StringBuilder sb)
		{
			string name = OptionsHelpers.FunctionName(Activity.Turmites);
			sb.WL();
			sb.WL(0,name + "[options] [output image]");
			sb.WL(1,"Turing machine mites/ants. see https://en.wikipedia.org/wiki/Turmite");
			sb.WL(1,"-p (string)"   ,"LR pattern string. See below for full language (default 'LR')");
			// sb.WL(1,"-img (image)"  ,"Use an image file as the starting state");
			sb.WL(1,"-e (edge rule)","Change edge handling rule (default Wrap)");
			sb.WL(1,"-s (x,y)"      ,"Starting location of turmite (defaults to center coordinate)");
			sb.WL(1,"-i (number)"   ,"Number of iterations (default 1000)");
			sb.WL();
			sb.WL(1,"Available Edge Rules:");
			sb.PrintEnum<PickEdgeRule>(1,EdgeRuleDesc);
			sb.WL();
			sb.WL(1,"Pattern language:");
			sb.WL(2,"The pattern language consist of a string of characters used to decide which action to take.");
			sb.WL(2,"Adding a number after the letter will repeat that rule. For example R3 is the same as RRR.");
			sb.WL();
			sb.WL(2,"L","Make a left turn (counterclock-wise)");
			sb.WL(2,"R","Make a right turn (clock-wise)");
			sb.WL(2,"U","Turn around (180 degree turn)");
			sb.WL(2,"F","Continue forward (no turn)");
			sb.WL(2,"N","Point north");
			sb.WL(2,"S","Point south");
			sb.WL(2,"E","Point east");
			sb.WL(2,"W","Point west");
		}

		static string EdgeRuleDesc(PickEdgeRule rule)
		{
			switch(rule) {
			case PickEdgeRule.Wrap: return "Wrap around to the other side";
			case PickEdgeRule.Reflect: return "Turn around at the edge";
			}
			return "";
		}

		static bool ParsePattern(string pattern, out IReadOnlyList<Rule> seq)
		{
			var list = new List<Rule>();
			int i=0, len = pattern.Length;
			while(i < len) {
				//check for a letter operation
				var letter = pattern[i].ToString();
				if (!OptionsHelpers.TryParseEnumFirstLetter<PickOp>(letter,out PickOp op)) {
					throw new ArgumentException($"Unknown operation '{letter}'");
				}
				i++; //consume letter

				//check for a number
				string snum = "";
				int? num = null;

				while (i < len && char.IsNumber(pattern[i])) {
					snum += pattern[i];
					i++; // consume number
				}
				if (!String.IsNullOrWhiteSpace(snum)) {

					if (!int.TryParse(snum, out int inum)) {
						throw new ArgumentException($"Unable to parse pattern number '{snum}'");
					}
					num = inum;
				}
				if (num.HasValue && num < 1) {
					throw new ArgumentOutOfRangeException("Pattern number must be greater than zero");
				}
				else {
					num = num.GetValueOrDefault(1);
				}

				list.Add(new Rule {
					Operation = op,
					Count = num.Value
				});
			}

			seq = list;
			return true;
		}

		static IReadOnlyList<Rule> DefaultSeq()
		{
			return new List<Rule> {
				 new Rule { Operation = PickOp.L, Count = 1 }
				,new Rule { Operation = PickOp.R, Count = 1 }
			};
		}

		protected override AbstractProcessor CreateProcessor()
		{
			return new Processor { O = O };
		}

		Options O = new Options();
	}
}
