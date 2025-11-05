using Rasberry.Cli;

namespace ImageFunctions.Core.Gradients;

public static class GradientHelpers
{
	internal const string ParamName = "--gradient";
	public static UsageOne GradientUsageParameter(int indention = 1)
	{
		return new UsageRegistered(indention,
			ParamName, "Use a (registered) gradient (defaults to 'FullRGB')") {
			NameSpace = GradientRegister.NS,
			TypeText = "name"
		};
	}

	public static ParseResult<Lazy<IColorGradient>> ScanGradient(this ParseParams p, ICoreLog log, IRegister register)
	{
		if(p == null) { throw Squeal.ArgumentNull(nameof(p)); }
		if(log == null) { throw Squeal.ArgumentNull(nameof(log)); }

		var reg = new GradientRegister(register);
		Lazy<IColorGradient> gradient = null;
		ParseParams.Result result;

		var r = p.Scan<string>(ParamName);

		if(r.IsMissing()) {
			var entry = reg.Get("FullRGB");
			gradient = entry.Item;
			result = ParseParams.Result.Good;
		}
		else if(!reg.Try(r.Value, out var entry)) {
			gradient = default;
			log.Error(Note.NotRegistered(reg.Namespace, r.Value));
			result = ParseParams.Result.UnParsable;
		}
		else {
			gradient = entry.Item;
			result = ParseParams.Result.Good;
		}

		return new ParseResult<Lazy<IColorGradient>>(result, ParamName, gradient);
	}
}
