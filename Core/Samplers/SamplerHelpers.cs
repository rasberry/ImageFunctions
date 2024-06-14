using Rasberry.Cli;

namespace ImageFunctions.Core.Samplers;

public static class SamplerHelpers
{
	public static void SamplerHelpLine(this StringBuilder sb)
	{
		sb.ND(1, "--sampler (name)", "Use given (registered) sampler (defaults to nearest pixel)");
	}

	public static UsageOne SamplerUsageParameter(int indention = 1)
	{
		return new UsageOne<string>(indention,
			"--sampler (name)", "Use given (registered) sampler (defaults to nearest pixel)") {
			Auxiliary = AuxiliaryKind.Sampler
		};
	}

	public static ParseResult<Lazy<ISampler>> ScanSampler(this ParseParams p, IRegister register)
	{
		if(p == null) {
			throw Squeal.ArgumentNull(nameof(p));
		}

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
			Log.Error(Note.NotRegistered(r.Name, r.Value));
			result = ParseParams.Result.UnParsable;
		}
		else {
			sampler = entry.Item;
			result = ParseParams.Result.Good;
		}

		return new ParseResult<Lazy<ISampler>>(result, "--sampler", sampler);
	}
}
