using ImageFunctions.Core;
using ImageFunctions.Core.ColorSpace;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.SliceComponent;

public sealed class Options : IOptions
{
	public string SomeOption;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1,"Slices an image component into multiple layers.");
		sb.ND(1,"-s (space)"     ,"Color space to use (default RGB)");
		sb.ND(1,"-c (component)" ,"Component to slice (default R)");
		sb.ND(1,"-n (number)"    ,"Number of slices to use (default 16)");
		sb.ND(1,"-r (number[%])" ,"Reset the component to given value (0.0-1.0 / 0%-100%)");
		sb.ND(1,"-o (number)"    ,"Keep only a specific slice between 1 and -n");
		sb.WT();
		sb.ND(1,"Available Spaces","Components");
		PrintSpaces(sb,register);
	}

	void PrintSpaces(StringBuilder sb, IRegister register)
	{
		var reg = new Color3SpaceRegister(register);
		foreach(var name in reg.All()) {
			var space = reg.Get(name);
			var color = space.Item.ToSpace(PlugColors.Black);
			var comps = color.ComponentNames.ToArray();
			sb.ND(1,$"{space.Name}",$"[{String.Join(',',comps)}]");
		}
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});

		if (p.Scan<string>("-s", "Rgb")
			.WhenGoodOrMissing(r => { SpaceName = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan<string>("-c", "R")
			.WhenGoodOrMissing(r => { ComponentName = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan<int>("-n", 16)
			.WhenGoodOrMissing(r => { Slices = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan<double>("-r", par: parser)
			.WhenGood(r => { ResetValue = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (p.Scan<int>("-o")
			.WhenGood(r => { WhichSlice = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if (Slices < 1) {
			//Tell.must(
		}

		return true;
	}

	public string SpaceName;
	public string ComponentName;
	public int Slices;
	public double? ResetValue;
	public int? WhichSlice;
}