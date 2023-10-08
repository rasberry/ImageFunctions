using System.Drawing;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Turmites;

public sealed class Options : IOptions
{
	public PickEdgeRule EdgeRule = PickEdgeRule.None;
	public IReadOnlyList<Rule> Sequence = null;
	public Point? Start = null;
	public ulong Iterations = 0;
	public const int DefaultWidth = 1024;
	public const int DefaultHeight = 1024;

	public void Usage(StringBuilder sb)
	{
		sb.ND(1,"Turing machine mites/ants. see https://en.wikipedia.org/wiki/Turmite");
		sb.ND(1,"-p (string)"   ,"LR pattern string. See below for full language (default 'LR')");
		// sb.ND(1,"-img (image)"  ,"Use an image file as the starting state");
		sb.ND(1,"-e (edge rule)","Change edge handling rule (default Wrap)");
		sb.ND(1,"-s (x,y)"      ,"Starting location of turmite (defaults to center coordinate)");
		sb.ND(1,"-i (number)"   ,"Number of iterations (default 1000)");
		sb.WT();
		sb.ND(1,"Available Edge Rules:");
		sb.PrintEnum<PickEdgeRule>(1,EdgeRuleDesc);
		sb.WT();
		sb.ND(1,"Pattern language:");
		sb.ND(2,"The pattern language consist of a string of characters used to decide which action to take.");
		sb.ND(2,"Adding a number after the letter will repeat that rule. For example R3 is the same as RRR.");
		sb.WT();
		sb.ND(2,"L","Make a left turn (counterclock-wise)");
		sb.ND(2,"R","Make a right turn (clock-wise)");
		sb.ND(2,"U","Turn around (180 degree turn)");
		sb.ND(2,"F","Continue forward (no turn)");
		sb.ND(2,"N","Point north");
		sb.ND(2,"S","Point south");
		sb.ND(2,"E","Point east");
		sb.ND(2,"W","Point west");
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if (p.Default("-p",out Sequence, DefaultSeq(), ParsePattern).IsInvalid()) {
			return false;
		}
		//if (p.Default("-img", out InImage).IsInvalid()) {
		//	return false;
		//}
		if (p.Default("-e", out EdgeRule, PickEdgeRule.Wrap).IsInvalid()) {
			return false;
		}
		if (p.Default("-s", out Start, null).IsInvalid()) {
			return false;
		}
		if (p.Default("-i", out Iterations, 1000ul).IsInvalid()) {
			return false;
		}

		return true;
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
		var parser = new ParseParams.Parser<PickOp>((string n, out PickOp p) => {
			return ExtraParsers.TryParseEnumFirstLetter(n, out p, ignoreZero: true);
		});

		while(i < len) {
			//check for a letter operation
			var letter = pattern[i].ToString();
			if (!parser(letter,out PickOp op)) {
				Tell.CouldNotParse(letter,"-p");
				seq = default;
				return false;
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
}

public enum PickEdgeRule
{
	None = 0,
	Wrap = 1,
	Reflect = 2
}

public enum PickOp
{
	None = 0,
	L,R,U,F,N,S,E,W
}

public class Rule
{
	public PickOp Operation = PickOp.None;
	public int Count = 0;
}