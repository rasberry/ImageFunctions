using ImageFunctions.Core;
using ImageFunctions.Core.ColorSpace;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.SliceComponent;

public sealed class Options : IOptions
{
	public string SomeOption;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.ND(1, "Slices an image component into multiple layers.");
		sb.ND(1, "-s (space)", "Color space to use (default RGB)");
		sb.ND(1, "-c (component)", "Component to slice (default R)");
		sb.ND(1, "-n (number)", "Number of slices to use (default 16)");
		sb.ND(1, "-r (number[%])", "Reset the component to given value (0.0-1.0 / 0%-100%)");
		sb.ND(1, "-o (number)", "Keep only a specific slice between 1 and -n");
		sb.WT();
		sb.ND(1, "Available Spaces", "Components");
		PrintSpaces(sb, register);
	}

	void PrintSpaces(StringBuilder sb, IRegister register)
	{
		var reg = new Color3SpaceRegister(register);
		foreach(var name in reg.All()) {
			var space = reg.Get(name);
			var info = space.Item.Info;
			var desc = info.Description;
			if(!String.IsNullOrWhiteSpace(desc)) {
				desc = $" - {desc}";
			}
			sb.ND(1, $"{space.Name}", $"[{String.Join(',', info.ComponentNames)}]{desc}");
		}
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});

		if(p.Scan<string>("-s", "Rgb")
			.WhenGoodOrMissing(r => { SpaceName = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<string>("-c", "R")
			.WhenGoodOrMissing(r => { ComponentName = r.Value; return r; })
			.WhenInvalidTellDefault()
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<int>("-n", 16)
			.WhenInvalidTellDefault()
			.WhenGoodOrMissing(r => {
				if(r.Value < 1) {
					Log.Error(Note.MustBeGreaterThan(r.Name, 1, true));
					return r with { Result = ParseParams.Result.UnParsable };
				}
				Slices = r.Value; return r;
			})
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<double>("-r", par: parser)
			.WhenInvalidTellDefault()
			.WhenGood(r => {
				if(r.Value < 0.0 || r.Value > 1.0) {
					Log.Error(Note.MustBeBetween(r.Name, "0.0, 0%", "1.0, 100%"));
					return r with { Result = ParseParams.Result.UnParsable };
				}
				ResetValue = r.Value; return r;
			})
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<int>("-o")
			.WhenInvalidTellDefault()
			.WhenGood(r => {
				if(r.Value < 1 || r.Value > Slices) {
					Log.Error(Note.MustBeBetween(r.Name, "1", Slices.ToString()));
					return r with { Result = ParseParams.Result.UnParsable };
				}
				WhichSlice = r.Value; return r;
			})
			.IsInvalid()
		) {
			return false;
		}

		var reg = new Color3SpaceRegister(register);
		if(!reg.Try(SpaceName, out var spaceItem)) {
			Log.Error(Note.NotRegistered(reg.Namespace, SpaceName));
			return false;
		}
		Space = spaceItem.Item;

		var nameUc = ComponentName.ToUpperInvariant();
		bool found = false;
		foreach(var cn in Space.Info.ComponentNames) {
			if(nameUc == cn) {
				found = true; break;
			}
		}
		if(!found) {
			Log.Error(Note.ComponentNotFound(ComponentName));
			return false;
		}

		return true;
	}

	string SpaceName;
	public string ComponentName;
	public int Slices;
	public double? ResetValue;
	public int? WhichSlice;
	public IColor3Space Space;
}
