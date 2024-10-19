using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core;

public delegate IFunction FunctionSpawner(IRegister register, ILayers layers, ICoreOptions options);

public class FunctionRegister : AbstractRegistrant<FunctionSpawner>
{
	public FunctionRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	internal const string NS = "Function";
	public override string Namespace { get { return NS; } }

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		register.Add(NS, nameof(Functions.Line), new FunctionSpawner(Functions.Line.Function.Create));
	}
}
