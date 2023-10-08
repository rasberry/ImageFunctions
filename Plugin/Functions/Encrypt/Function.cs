using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Encrypt;

[InternalRegisterFunction(nameof(Encrypt))]
public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		O.Usage(sb);
	}

	public bool Run(IRegister register, ILayers layers, ICoreOptions core, string[] args)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if (!O.ParseArgs(args, register)) {
			return false;
		}

		if (layers.Count < 1) {
			Tell.LayerMustHaveAtLeast();
			return false;
		}

		var engine = core.Engine.Item.Value;
		var frame = layers.First();
		using var progress = new ProgressBar();
		using var canvas = engine.NewCanvasFromLayers(layers);
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

	Options O = new Options();
}