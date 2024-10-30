using Rasberry.Cli;

namespace ImageFunctions.Core.Samplers;

public static class SamplerHelpers
{
	public static UsageOne SamplerUsageParameter(int indention = 1)
	{
		return new UsageRegistered(indention,
			"--sampler", "Use given (registered) sampler (defaults to nearest neighbor)") {
			NameSpace = SamplerRegister.NS,
			TypeText = "name"
		};
	}

	public static ParseResult<Lazy<ISampler>> ScanSampler(this ParseParams p, ICoreLog log, IRegister register)
	{
		if(p == null) { throw Squeal.ArgumentNull(nameof(p)); }
		if(log == null) { throw Squeal.ArgumentNull(nameof(log)); }

		var reg = new SamplerRegister(register);
		ParseParams.Result result;
		Lazy<ISampler> sampler = null;

		var r = p.Scan<string>("--sampler");

		if(r.IsMissing()) {
			var entry = reg.Get("NearestNeighbor");
			sampler = entry.Item;
			result = ParseParams.Result.Good;
		}
		else if(!reg.Try(r.Value, out var entry)) {
			sampler = default;
			log.Error(Note.NotRegistered(r.Name, r.Value));
			result = ParseParams.Result.UnParsable;
		}
		else {
			sampler = entry.Item;
			result = ParseParams.Result.Good;
		}

		return new ParseResult<Lazy<ISampler>>(result, "--sampler", sampler);
	}
}
