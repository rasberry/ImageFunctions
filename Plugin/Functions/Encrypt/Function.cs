using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Encrypt;

[InternalRegisterFunction(nameof(Encrypt))]
public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb);
	}

	public bool Run(IRegister register, ILayers layers, string[] args)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if (!Options.ParseArgs(args, register)) {
			return false;
		}

		if (layers.Count < 1) {
			Tell.LayerMustHaveAtLeast();
			return false;
		}

		var frame = layers.First();
		using var progress = new ProgressBar();
		using var canvas = layers.NewCanvasFromLayers();
		Encryptor processor = new Encryptor() { IVBytes = Options.IVBytes };

		//make a copy of the original
		canvas.CopyFrom(frame);

		//Encryption really wants to use streams
		using var inStream = new PixelStream(canvas);
		using var outStream = new PixelStream(canvas);

		processor.TransformStream(Options.DoDecryption,inStream,outStream,Options.Password,progress);

		//put processed image back
		frame.CopyFrom(canvas);

		return true;
	}
}