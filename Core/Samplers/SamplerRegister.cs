using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core.Samplers;

public class SamplerRegister : AbstractRegistrant<Lazy<ISampler>>
{
	public SamplerRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	internal const string NS = "Sampler";
	public override string Namespace { get { return NS; } }

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		var reg = new SamplerRegister(register);
		reg.Add("NearestNeighbor", new Lazy<ISampler>(() => new NearestNeighbor()));
		reg.Add("Bicubic", new Lazy<ISampler>(() => new Bicubic()));
		reg.Add("Box", new Lazy<ISampler>(() => new Box()));
		reg.Add("CatmullRom", new Lazy<ISampler>(() => new CatmullRom()));
		reg.Add("Hermite", new Lazy<ISampler>(() => new Hermite()));
		reg.Add("Lanczos2", new Lazy<ISampler>(() => new Lanczos2()));
		reg.Add("Lanczos3", new Lazy<ISampler>(() => new Lanczos3()));
		reg.Add("Lanczos5", new Lazy<ISampler>(() => new Lanczos5()));
		reg.Add("Lanczos8", new Lazy<ISampler>(() => new Lanczos8()));
		reg.Add("MitchellNetravali", new Lazy<ISampler>(() => new MitchellNetravali()));
		reg.Add("Robidoux", new Lazy<ISampler>(() => new Robidoux()));
		reg.Add("RobidouxSharp", new Lazy<ISampler>(() => new RobidouxSharp()));
		reg.Add("Spline", new Lazy<ISampler>(() => new Spline()));
		reg.Add("Triangle", new Lazy<ISampler>(() => new Triangle()));
		reg.Add("Welch", new Lazy<ISampler>(() => new Welch()));
	}
}
