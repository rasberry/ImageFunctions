using System.Runtime.CompilerServices;
using System.Text;
using Rasberry.Cli;

namespace ImageFunctions.Core.Metrics;

public static class MetricHelpers
{
	public static void MetricHelpLine(this StringBuilder sb)
	{
		sb.ND(1,"--metric (name)","Use a (registered) distance metric (defaults to euclidean)");
	}

	public static ParseResult<Lazy<IMetric>> DefaultMetric(this ParseParams p, IRegister register)
	{
		var reg = new MetricRegister(register);
		Lazy<IMetric> metric = null;
		ParseParams.Result result;

		var r = p.Scan<string>("--metric");

		if (r.IsMissing()) {
			var entry = reg.Get("Euclidean");
			metric = entry.Item;
			result = ParseParams.Result.Good;
		}
		else if (!reg.Try(r.Value,out var entry)) {
			metric = default;
			Log.Error(Note.NotRegistered(reg.Namespace,r.Value));
			result = ParseParams.Result.UnParsable;
		}
		else {
			metric = entry.Item;
			result = ParseParams.Result.Good;
		}

		return new ParseResult<Lazy<IMetric>>(result, "--metric", metric);
	}
}