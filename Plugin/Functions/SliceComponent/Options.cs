using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.ColorSpace;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.SliceComponent;

public sealed class Options : IOptions, IUsageProvider
{
	public string SomeOption;
	readonly ICoreLog Log;

	public Options(IFunctionContext context)
	{
		if(context == null) { throw Squeal.ArgumentNull(nameof(context)); }
		Log = context.Log;
	}

	public void Usage(StringBuilder sb, IRegister register)
	{
		sb.RenderUsage(this);
	}

	public Usage GetUsageInfo()
	{
		var u = new Usage {
			Description = new UsageDescription(1, "Slices an image component into multiple layers."),
			Parameters = [
				ColorSpaceHelpers.Color3SpaceUsageParameter(1),
				new UsageOne<string>(1, "-c", "Component to slice (default R)") { TypeText = "component" },
				new UsageOne<int>(1, "-n", "Number of slices to use (default 16)") { Min = 1, Default = 16, Max = 99 },
				new UsageOne<double>(1, "-r", "Reset the component to given value (0.0-1.0 / 0%-100%)") { IsNumberPct = true },
				new UsageOne<int>(1, "-o", "Keep only a specific slice between 1 and -n") { Min = 1, Max = 99 },
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

		if(ColorSpaceHelpers.ScanColor3Space(p, Log, register)
			.WhenGoodOrMissing(r => { Space = r.Value; return r; })
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<string>("-s", "Rgb")
			.WhenGoodOrMissing(r => { SpaceName = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<string>("-c", "R")
			.WhenGoodOrMissing(r => { ComponentName = r.Value; return r; })
			.WhenInvalidTellDefault(Log)
			.IsInvalid()
		) {
			return false;
		}

		if(p.Scan<int>("-n", 16)
			.WhenInvalidTellDefault(Log)
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
			.WhenInvalidTellDefault(Log)
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
			.WhenInvalidTellDefault(Log)
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
