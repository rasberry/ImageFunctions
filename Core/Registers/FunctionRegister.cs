using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;

public delegate IFunction FunctionSpawner(IRegister register, ILayers layers, ICoreOptions options);

public class FunctionRegister : AbstractRegistrant<FunctionSpawner>
{
	public FunctionRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	public override string Namespace { get { return "Function"; }}

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		//TODO register Functions here
	}
}