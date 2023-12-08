using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Encrypt;

[InternalRegisterFunction(nameof(Encrypt))]
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
		O.Usage(sb, Register);
	}

	public bool Run(string[] args)
	{
		if (Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if (!O.ParseArgs(args, Register)) {
			return false;
		}

		if (Layers.Count < 1) {
			Tell.LayerMustHaveAtLeast();
			return false;
		}

		var engine = Core.Engine.Item.Value;
		var frame = Layers.First();
		using var progress = new ProgressBar();
		using var canvas = engine.NewCanvasFromLayers(Layers);
		Encryptor processor = new Encryptor() { IVBytes = O.IVBytes };

		//make a copy of the original
		canvas.CopyFrom(frame);

		//Encryption really wants to use streams
		using var inStream = new PixelStream(canvas);
		using var outStream = new PixelStream(canvas);

		processor.TransformStream(O.DoDecryption,inStream,outStream,O.Password,progress);

		//put processed image back
		frame.CopyFrom(canvas);

		return true;
	}

	readonly Options O = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions Core;
}