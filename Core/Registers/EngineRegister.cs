using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;

public class EngineRegister : AbstractRegistrant<Lazy<IImageEngine>>
{
	public EngineRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	internal const string NS = "Engine";
	public override string Namespace { get { return NS; } }


	[InternalRegister]
	internal static void Register(IRegister register)
	{
		var er = new EngineRegister(register);
		er.Add(SixLaborsString, new Lazy<IImageEngine>(() => new Engines.SixLaborsEngine()));
		er.Default(SixLaborsString);
	}

	internal const string SixLaborsString = "SixLabors";
}
