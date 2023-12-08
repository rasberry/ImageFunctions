using ImageFunctions.Core;

namespace ImageFunctions.Plugin.Functions.SliceComponent;

[InternalRegisterFunction(nameof(SliceComponent))]
public class Function : IFunction
{
	public static IFunction Create(IRegister register, ILayers layers, ICoreOptions core)
	{
		var f = new Function {
			Register = register,
			Core = core,
			Layers = layers
		};
		return f;
	}
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb, Register);
	}

	public bool Run(string[] args)
	{
		if (Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if (!Options.ParseArgs(args, Register)) {
			return false;
		}

		//TODO do the work here

		return true;
	}

	readonly Options Options = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions Core;
}