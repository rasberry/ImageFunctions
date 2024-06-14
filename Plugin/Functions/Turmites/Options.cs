using ImageFunctions.Core;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.Turmites;

public sealed class Options : IOptions, IUsageProvider
{
	public PickEdgeRule EdgeRule = PickEdgeRule.None;
	public IReadOnlyList<Rule> Sequence = null;
	public Point? Start = null;
	public ulong Iterations = 0;
	public const int DefaultWidth = 1024;
	public const int DefaultHeight = 1024;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1,"Turing machine mites/ants. see https://en.wikipedia.org/wiki/Turmite"),
			Parameters = [
				new UsageOne<string>(1, "-p (string)", "LR pattern string. See below for full language (default 'LR')"),
				new UsageOne<PickEdgeRule>(1, "-e (edge rule)", "Change edge handling rule (default Wrap)") { Default = PickEdgeRule.Wrap },
				new UsageOne<Point?>(1, "-s (x,y)", "Starting location of turmite (defaults to center coordinate)"),
				new UsageOne<ulong>(1, "-i (number)", "Number of iterations (default 1000)") { Min = 1, Default = 1000 },
			],
			EnumParameters = [
				new UsageEnum<PickEdgeRule>(1,"Available Edge Rules:") { DescriptionMap = EdgeRuleDesc }
			],
			SuffixParameters = [
				new UsageText(1, "Pattern language:") { AddNewLineBefore = true },
				new UsageText(2, "The pattern language consist of a string of characters used to decide which action to take."),
				new UsageText(2, "Adding a number after the letter will repeat that rule. For example R3 is the same as RRR."),
				new UsageText(2, "L", "Make a left turn (counterclock-wise)") { AddNewLineBefore = true },
				new UsageText(2, "R", "Make a right turn (clock-wise)"),
				new UsageText(2, "U", "Turn around (180 degree turn)"),
				new UsageText(2, "F", "Continue forward (no turn)"),
				new UsageText(2, "N", "Point north"),
				new UsageText(2, "S", "Point south"),
				new UsageText(2, "E", "Point east"),
				new UsageText(2, "W", "Point west")
			]
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);

		if(p.Scan("-p", DefaultSeq(), ParsePattern)
			.WhenGoodOrMissing(r => { Sequence = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-e", PickEdgeRule.Wrap)
			.WhenGoodOrMissing(r => { EdgeRule = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<Point?>("-s")
			.WhenGood(r => { Start = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan("-i", 1000ul)
			.WhenGoodOrMissing(r => { Iterations = r.Value; return r; })
			.BeGreaterThan(1ul, true)
			.IsInvalid()
		) {
			return false;
		}

		return true;
	}

	static string EdgeRuleDesc(object rule)
	{
		switch(rule) {
		case PickEdgeRule.Wrap: return "Wrap around to the other side";
		case PickEdgeRule.Reflect: return "Turn around at the edge";
		}
		return "";
	}

	static IReadOnlyList<Rule> ParsePattern(string pattern)
	{
		var list = new List<Rule>();
		int i = 0, len = pattern.Length;
		var parser = new ParseParams.Parser<PickOp>((string n) => {
			return ExtraParsers.ParseEnumFirstLetter<PickOp>(n, ignoreZero: true);
		});

		while(i < len) {
			//check for a letter operation
			var letter = pattern[i].ToString();
			var op = parser(letter);
			i++; //consume letter

			//check for a number
			string snum = "";
			int? num = null;

			while(i < len && char.IsNumber(pattern[i])) {
				snum += pattern[i];
				i++; // consume number
			}
			if(!String.IsNullOrWhiteSpace(snum)) {
				if(!int.TryParse(snum, out int inum)) {
					throw PlugSqueal.CannotParsePatterNumber(snum);
				}
				num = inum;
			}
			if(num.HasValue && num < 1) {
				throw PlugSqueal.PatternNumberGtrZero();
			}
			else {
				num = num.GetValueOrDefault(1);
			}

			list.Add(new Rule {
				Operation = op,
				Count = num.Value
			});
		}

		return list;
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
	L = 1, R = 2, U = 3, F = 4, N = 5, S = 6, E = 7, W = 8
}

public class Rule
{
	public PickOp Operation = PickOp.None;
	public int Count = 0;
}
