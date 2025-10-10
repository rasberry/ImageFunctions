using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;

namespace ImageFunctions.Plugin.Functions.DistanceColoring;

public class Function : IFunction
{
	public static IFunction Create(IFunctionContext context)
	{
		if(context == null) {
			throw Squeal.ArgumentNull(nameof(context));
		}

		var f = new Function {
			Context = context,
			Local = new(context),
		};
		return f;
	}

	public void Usage(StringBuilder sb)
	{
		Context.Options.Usage(sb, Context.Register);
	}

	public bool Run(string[] args)
	{
		if(Context.Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!Context.Options.ParseArgs(args, Context.Register)) {
			return false;
		}
		if(Context.Layers.Count < 1) {
			Context.Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}

		//since we're rendering pixels make a new layer each time
		var engine = Context.Options.Engine.Item.Value;
		var (dfw, dfh) = Context.Options.GetDefaultWidthHeight();
		var image = engine.NewCanvasFromLayersOrDefault(Context.Layers, dfw, dfh);
		Context.Layers.Push(image);



		//TODO do the work here

		return true;
	}

	Options Local;
	IFunctionContext Context;
	public IOptions Options { get { return Context.Options; } }
}
