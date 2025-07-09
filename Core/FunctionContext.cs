using Rasberry.Cli;

namespace ImageFunctions.Core;

public sealed class FunctionContext : IFunctionContext
{
	public IRegister Register { get; set; }
	public ILayers Layers { get; set; }
	public ICoreOptions Options { get; set; }
	public ICoreLog Log { get; set; }
	public IProgressWithLabel<double> Progress { get; set; }
	public CancellationToken Token { get; set; }
}
