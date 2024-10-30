namespace ImageFunctions.Core;

public sealed class FunctionContext : IFunctionContext
{
	public IRegister Register { get; set; }
	public ILayers Layers { get; set; }
	public ICoreOptions Options { get; set; }
	public ICoreLog Log { get; set; }
}
