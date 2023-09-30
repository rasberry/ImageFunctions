using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;

public class FunctionRegister : AbstractRegistrant<Lazy<IFunction>>
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