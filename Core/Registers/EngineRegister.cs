using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;

public class EngineRegister : AbstractRegistrant<Lazy<IImageEngine>>
{
	public EngineRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	internal override string Namespace { get { return "Engine"; }}

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		//TODO register engines here
	}
}