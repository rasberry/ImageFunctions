using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.Encrypt;

[InternalRegisterFunction(nameof(Encrypt))]
public class Function : IFunction
{
	public static IFunction Create(IFunctionContext context)
	{
		if(context == null) {
			throw Squeal.ArgumentNull(nameof(context));
		}

		var f = new Function {
			Context = context,
			O = new(context)
		};
		return f;
	}
	public void Usage(StringBuilder sb)
	{
		Options.Usage(sb, Context.Register);
	}

	public IOptions Options { get { return O; } }
	IFunctionContext Context;
	Options O;

	public bool Run(string[] args)
	{
		if(Context.Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!O.ParseArgs(args, Context.Register)) {
			return false;
		}

		if(Context.Layers.Count < 1) {
			Context.Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}

		var engine = Context.Options.Engine.Item.Value;
		var frame = Context.Layers.First().Canvas;
		using var progress = new ProgressBar();
		using var canvas = engine.NewCanvasFromLayers(Context.Layers);
		Encryptor processor = new Encryptor() { IVBytes = O.IVBytes };

		//make a copy of the original
		canvas.CopyFrom(frame);

		//Encryption really wants to use streams
		using var inStream = new PixelStream(canvas);
		using var outStream = new PixelStream(canvas);

		processor.TransformStream(O.DoDecryption, inStream, outStream, O.Password, progress);

		//put processed image back
		frame.CopyFrom(canvas);

		return true;
	}
}
