using Rasberry.Cli;

namespace ImageFunctions.Core.Gradients;

public static class GradientHelpers
{
	internal const string ParamName = "--gradient";
	public static UsageOne GradientUsageParameter(int indention = 1, bool skipDefault = false)
	{
		string text = "Use a (registered) gradient" + (skipDefault ? "" : " (defaults to 'FullRGB')");
		return new UsageRegistered(indention,
			ParamName, text) {
			NameSpace = GradientRegister.NS,
			TypeText = "name"
		};
	}

	public static ParseResult<Lazy<IColorGradient>> ScanGradient(this ParseParams p, ICoreLog log, IRegister register, bool skipDefault = false)
	{
		if(p == null) { throw Squeal.ArgumentNull(nameof(p)); }
		if(log == null) { throw Squeal.ArgumentNull(nameof(log)); }

		var reg = new GradientRegister(register);
		Lazy<IColorGradient> gradient = null;
		ParseParams.Result result;

		var r = p.Scan<string>(ParamName);
		string name;

		if(r.IsMissing()) {
			name = skipDefault ? null : "FullRGB";
		}
		else {
			name = r.Value;
		}

		if(string.IsNullOrWhiteSpace(name)) {
			result = ParseParams.Result.Missing;
		}
		else if(!reg.Try(name, out var entry)) {
			gradient = default;
			log.Error(Note.NotRegistered(reg.Namespace, name));
			result = ParseParams.Result.UnParsable;
		}
		else {
			gradient = entry.Item;
			result = ParseParams.Result.Good;
		}

		return new ParseResult<Lazy<IColorGradient>>(result, ParamName, gradient);
	}
}
