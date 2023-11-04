using System.Text;
using Rasberry.Cli;

namespace ImageFunctions.Core.Samplers;

public static class SamplerHelpers
{
	public static void SamplerHelpLine(this StringBuilder sb)
	{
		sb.ND(1,"--sampler (name)","Use given (registered) sampler (defaults to nearest pixel)");
	}

	public static ParseResult<Lazy<ISampler>> DefaultSampler(this ParseParams p, IRegister register)
	{
		var reg = new SamplerRegister(register);
		ParseParams.Result result;
		Lazy<ISampler> sampler = null;

		var r = p.Scan<string>("--sampler");

		if (r.IsMissing()) {
			var entry = reg.Get("NearestNeighbor");
			sampler = entry.Item;
			result = ParseParams.Result.Good;
		}
		else if (!reg.Try(r.Value,out var entry)) {
			sampler = default;
			Tell.NotRegistered("Sampler",r.Value);
			result = ParseParams.Result.UnParsable;
		}
		else {
			sampler = entry.Item;
			result = ParseParams.Result.Good;
		}

		return new ParseResult<Lazy<ISampler>>(result, "--sampler", sampler);
	}
}
