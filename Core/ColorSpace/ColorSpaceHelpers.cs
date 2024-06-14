using Rasberry.Cli;

namespace ImageFunctions.Core.ColorSpace;

public static class ColorSpaceHelpers
{
	internal static bool TryPrintColorSpace(IRegister register, StringBuilder sb, INameSpaceName item)
	{
		if (item == null) {
			return false;
		}
		if (register == null) {
			Squeal.ArgumentNull(nameof(register));
		}
		if (sb == null) {
			Squeal.ArgumentNull(nameof(sb));
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
			return false;
		}

		var desc = info.Description;
		if(!String.IsNullOrWhiteSpace(desc)) {
			desc = $" - {desc}";
		}
		sb.ND(1, $"{item.Name}", $"[{String.Join(',', info.ComponentNames)}]{desc}");
		return true;
	}

	public static UsageOne Color3SpaceUsageParameter(int indention = 1)
	{
		return new UsageOne<string>(indention,
			"--space (name)", "Use a (registered) color(3) space (defaults to RGB)") {
			Auxiliary = AuxiliaryKind.Color3Space
		};
	}

	public static UsageOne Color4SpaceUsageParameter(int indention = 1)
	{
		return new UsageOne<string>(indention,
			"--space4 (name)", "Use a (registered) color(4) space (defaults to CMYK)") {
			Auxiliary = AuxiliaryKind.Color4Space
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
			var entry = reg.Get("Rgb");
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
			var entry = reg.Get("Cmyk");
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