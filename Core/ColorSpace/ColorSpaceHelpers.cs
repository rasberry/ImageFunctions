using Rasberry.Cli;

namespace ImageFunctions.Core.ColorSpace;

public static class ColorSpaceHelpers
{
	internal static string GetColorSpaceHelp(IRegister register, INameSpaceName item)
	{
		if (item == null) {
			return null;
		}
		if (register == null) {
			Squeal.ArgumentNull(nameof(register));
		}

		ColorSpaceInfo info;

		if (item.NameSpace == Color3SpaceRegister.NS) {
			var reg = new Color3SpaceRegister(register);
			var space = reg.Get(item.Name);
			info = space.Item.Info;
		}
		else if (item.NameSpace == Color4SpaceRegister.NS) {
			var reg = new Color4SpaceRegister(register);
			var space = reg.Get(item.Name);
			info = space.Item.Info;
		}
		else {
			return null;
		}

		var desc = info.Description;
		if(!String.IsNullOrWhiteSpace(desc)) {
			desc = $" - {desc}";
		}

		return $"[{String.Join(',', info.ComponentNames)}]{desc}";
	}

	const string Default3 = "Rgb";
	const string Default4 = "Cmyk";

	public static UsageOne Color3SpaceUsageParameter(int indention = 1)
	{
		return new UsageRegistered(indention,
			"--space", "Use a (registered) color(3) space (defaults to RGB)") {
			NameSpace = Color3SpaceRegister.NS,
			TypeText = "name",
			Default = Default3
		};
	}

	public static UsageOne Color4SpaceUsageParameter(int indention = 1)
	{
		return new UsageRegistered(indention,
			"--space4", "Use a (registered) color(4) space (defaults to CMYK)") {
			NameSpace = Color4SpaceRegister.NS,
			TypeText = "name",
			Default = Default4
		};
	}

	public static ParseResult<IColor3Space> ScanColor3Space(this ParseParams p, IRegister register)
	{
		if(p == null) {
			throw Squeal.ArgumentNull(nameof(p));
		}

		var reg = new Color3SpaceRegister(register);
		IColor3Space space = null;
		ParseParams.Result result;

		var r = p.Scan<string>("--space");

		if(r.IsMissing()) {
			var entry = reg.Get(Default3);
			space = entry.Item;
			result = ParseParams.Result.Good;
		}
		else if(!reg.Try(r.Value, out var entry)) {
			space = default;
			Log.Error(Note.NotRegistered(reg.Namespace, r.Value));
			result = ParseParams.Result.UnParsable;
		}
		else {
			space = entry.Item;
			result = ParseParams.Result.Good;
		}

		return new ParseResult<IColor3Space>(result, "--space", space);
	}

	public static ParseResult<IColor4Space> ScanColor4Space(this ParseParams p, IRegister register)
	{
		if(p == null) {
			throw Squeal.ArgumentNull(nameof(p));
		}

		var reg = new Color4SpaceRegister(register);
		IColor4Space space = null;
		ParseParams.Result result;

		var r = p.Scan<string>("--space4");

		if(r.IsMissing()) {
			var entry = reg.Get(Default4);
			space = entry.Item;
			result = ParseParams.Result.Good;
		}
		else if(!reg.Try(r.Value, out var entry)) {
			space = default;
			Log.Error(Note.NotRegistered(reg.Namespace, r.Value));
			result = ParseParams.Result.UnParsable;
		}
		else {
			space = entry.Item;
			result = ParseParams.Result.Good;
		}

		return new ParseResult<IColor4Space>(result, "--space4", space);
	}
}