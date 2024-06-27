using Rasberry.Cli;

namespace ImageFunctions.Core.Metrics;

public static class MetricHelpers
{
	// public static void MetricHelpLine(this StringBuilder sb)
	// {
	// 	sb.ND(1, "--metric (name)", "Use a (registered) distance metric (defaults to euclidean)");
	// }

	public static UsageOne MetricUsageParameter(int indention = 1)
	{
		return new UsageRegistered(indention,
			"--metric", "Use a (registered) distance metric (defaults to euclidean)") {
			NameSpace = MetricRegister.NS,
			TypeText = "name"
		};
	}

	public static ParseResult<Lazy<IMetric>> ScanMetric(this ParseParams p, IRegister register)
	{
		if(p == null) {
			throw Squeal.ArgumentNull(nameof(p));
		}

		var reg = new MetricRegister(register);
		Lazy<IMetric> metric = null;
		ParseParams.Result result;

		var r = p.Scan<string>("--metric");

		if(r.IsMissing()) {
			var entry = reg.Get("Euclidean");
			metric = entry.Item;
			result = ParseParams.Result.Good;
		}
		else if(!reg.Try(r.Value, out var entry)) {
			metric = default;
			Log.Error(Note.NotRegistered(reg.Namespace, r.Value));
			result = ParseParams.Result.UnParsable;
		}
		else {
			metric = entry.Item;
			result = ParseParams.Result.Good;
		}

		return new ParseResult<Lazy<IMetric>>(result, "--metric", metric);
	}
}
