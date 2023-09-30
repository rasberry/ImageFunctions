using ImageFunctions.Core;
using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;

public class EngineRegister : AbstractRegistrant<Lazy<IImageEngine>>
{
	public EngineRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	public override string Namespace { get { return "Engine"; }}


	[InternalRegister]
	internal static void Register(IRegister register)
	{
		var er = new EngineRegister(register);
		er.Add(SixLaborsString,new Lazy<IImageEngine>(() => new Engines.SixLaborsEngine()));
	}

	internal const string SixLaborsString = "SixLabors";
}