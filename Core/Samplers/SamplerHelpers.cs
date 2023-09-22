using System.Text;
using Rasberry.Cli;

namespace ImageFunctions.Core.Samplers;

public static class SamplerHelpers
{
	public static void SamplerHelpLine(this StringBuilder sb)
	{
		sb.ND(1,"--sampler (name)","Use given (registered) sampler (defaults to nearest pixel)");
	}

	public static ParseParams.Result DefaultSampler(this ParseParams p, IRegister register, out Lazy<ISampler> s)
	{
		var reg = new SamplerRegister(register);
		var r = p.Default("--sampler",out string name);
		if (r.IsMissing()) {
			s = reg.Get("NearestNeighbor");
		}
		else if (!reg.Try(name,out s)) {
			Tell.NotRegistered("Sampler",name);
			return ParseParams.Result.UnParsable;
		}
		return ParseParams.Result.Good;
	}
}
