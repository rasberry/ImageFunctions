using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Encrypt;

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

		if (!Tools.Engine.TryNewCanvasFromLayers(layers, out var newCanvas)) {
			return false;
		}
		var frame = layers.First();
		using var progress = new ProgressBar();
		using var canvas = newCanvas; //temporary canvas
		Encryptor processor = new Encryptor() { IVBytes = Options.IVBytes };

		//make a copy of the original
		canvas.CopyFrom(frame);

		//Encryption really wants to use streams
		var inStream = new PixelStream(canvas);
		var outStream = new PixelStream(canvas);

		using(inStream) using(outStream) {
			processor.TransformStream(Options.DoDecryption,inStream,outStream,Options.Password,progress);
		}

		//put processed image back
		frame.CopyFrom(canvas);

		return true;
	}
}