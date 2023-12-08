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
		sb.ND(2,"number[%] values are normalized to the range 0.0-1.0 / 0% to 100%");
		sb.ND(1,"-s (space)"     ,"Color space to use (default RGB)");
		sb.ND(1,"-c (component)" ,"Component to slice");
		sb.ND(1,"-n (number)"    ,"Number of slices to use (default 16)");
		sb.ND(1,"-r (number[%])" ,"Reset the component to given value instead of keeping original");
		sb.ND(1,"-o (number[%])" ,"Keep only a specific slice");
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

		if (p.Scan<string>("-myopt", "default")
			.WhenGoodOrMissing(r => { SomeOption = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		//TODO parse any other options and maybe do checks

		return true;
	}
}