using System.Runtime.CompilerServices;
using System.Text;
using Rasberry.Cli;

namespace ImageFunctions.Core.Metrics;

public static class Helpers
{
	public static void MetricHelpLine(this StringBuilder sb)
	{
		sb.ND(1,"--metric (name)","Use a (registered) distance metric (defaults to euclidean)");
	}

	public static ParseParams.Result DefaultMetric(this ParseParams p, IRegister register, out Lazy<IMetric> metric)
	{
		var reg = new MetricRegister(register);
		var r = p.Default("--metric", out string name);
		if (r.IsMissing()) {
			metric = reg.Get("Euclidean");
		}
		else if (!reg.Try(name,out metric)) {
			Tell.NotRegistered(reg.Namespace,name);
			return ParseParams.Result.UnParsable;
		}
		return ParseParams.Result.Good;

		//Func<IMetric,bool> hasTwo = (Metric mm) => mm == Metric.Minkowski;
		//var r = p.Default("--metric",out Metric metric,out double pfactor,Metric.None,0.0,hasTwo);
		//if (r.IsGood()) {
		//	if (hasTwo(metric)) {
		//		m = Registry.Map(metric,pfactor);
		//	}
		//	else {
		//		m = Registry.Map(metric);
		//	}
		//}
		//return r;
	}
}