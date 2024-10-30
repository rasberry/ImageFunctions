# if false
namespace ImageFunctions.Core.Functions.SimpleAlter;

public class Function : IFunction
{
	public static IFunction Create(IFunctionContext context)
	{
		if (context == null) {
			throw Squeal.ArgumentNull(nameof(context));
		}

		var f = new Function {
			Register = context.Register,
			CoreOptions = context.Options,
			Layers = context.Layers
		};
		return f;
	}
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb, Register);
	}

	public bool Run(string[] args)
	{
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!Options.ParseArgs(args, Register)) {
			return false;
		}

		//TODO do the work here

		return true;
	}

	public IOptions Options { get { return O; } }
	readonly Options O = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions CoreOptions;
}
#endif