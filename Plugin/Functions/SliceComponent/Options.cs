using ImageFunctions.Core;
using ImageFunctions.Core.ColorSpace;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.SliceComponent;

public sealed class Options : IOptions, IUsageProvider
{
	public string SomeOption;

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1,"Slices an image component into multiple layers."),
			Parameters = [
				ColorSpaceHelpers.Color3SpaceUsageParameter(1),
				new UsageOne<string>(1, "-c (component)", "Component to slice (default R)"),
				new UsageOne<int>(1, "-n (number)", "Number of slices to use (default 16)"),
				new UsageOne<double>(1, "-r (number[%])", "Reset the component to given value (0.0-1.0 / 0%-100%)"),
				new UsageOne<int>(1, "-o (number)", "Keep only a specific slice between 1 and -n"),
			],
		};

		return u;
	}

	public bool ParseArgs(string[] args, IRegister register)
	{
		var p = new ParseParams(args);
		var parser = new ParseParams.Parser<double>((string n) => {
			return ExtraParsers.ParseNumberPercent(n);
		});

		if (ColorSpaceHelpers.ScanColor3Space(p,register)
			.WhenGoodOrMissing(r => { Space = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

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
