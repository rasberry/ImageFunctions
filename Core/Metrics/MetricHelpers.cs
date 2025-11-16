using Rasberry.Cli;

namespace ImageFunctions.Core.Metrics;

public static class MetricHelpers
{
	const string ParamName = "--metric";
	public static UsageOne MetricUsageParameter(int indention = 1)
	{
		return new UsageRegistered(indention,
			ParamName, "Use a (registered) distance metric (defaults to euclidean)") {
			NameSpace = MetricRegister.NS,
			TypeText = "name"
		};
	}

	public static ParseResult<Lazy<IMetric>> ScanMetric(this ParseParams p, ICoreLog log, IRegister register)
	{
		if(p == null) { throw Squeal.ArgumentNull(nameof(p)); }
		if(log == null) { throw Squeal.ArgumentNull(nameof(log)); }

		var reg = new MetricRegister(register);
		Lazy<IMetric> metric = null;
		ParseParams.Result result;

		var r = p.Scan<string>(ParamName);

		if(r.IsMissing()) {
			var entry = reg.Get("Euclidean");
			metric = entry.Item;
			result = ParseParams.Result.Good;
		}
		else if(!reg.Try(r.Value, out var entry)) {
			metric = default;
			log.Error(Note.NotRegistered(reg.Namespace, r.Value));
			result = ParseParams.Result.UnParsable;
		}
		else {
			metric = entry.Item;
			result = ParseParams.Result.Good;
		}

		return new ParseResult<Lazy<IMetric>>(result, ParamName, metric);
	}
}
