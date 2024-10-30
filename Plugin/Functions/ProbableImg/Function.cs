using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Plugin.Aides;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.ProbableImg;

[InternalRegisterFunction(nameof(ProbableImg))]
public class Function : IFunction
{
	public static IFunction Create(IFunctionContext context)
	{
		if (context == null) {
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
	public ILayers Layers { get { return Context.Layers; }}

	public bool Run(string[] args)
	{
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!O.ParseArgs(args, Context.Register)) {
			return false;
		}

		if(Layers.Count < 1) {
			Context.Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}

		var source = Layers.First().Canvas;
		var bounds = source.Bounds();

		using var progress = new ProgressBar();
		MethodBase m = O.UseNonLookup
			? new MethodTwo { O = O, Log = Context.Log }
			: new MethodOne { O = O, Log = Context.Log }
		;
		m.CreateProfile(progress, source, bounds);

		//foreach(var kvp in Profile) {
		//	Log.Debug($"Key = {kvp.Key}");
		//	Log.Debug(kvp.Value.ToString());
		//}

		var engine = Context.Options.Engine.Item.Value;
		using var canvas = engine.NewCanvasFromLayers(Layers);
		m.CreateImage(progress, canvas);
		source.CopyFrom(canvas);

		return true;
	}
}

class ColorProfile<T>
{
	//dictionaries of index,count
	public Dictionary<T, long> NColor;
	public Dictionary<T, long> WColor;
	public Dictionary<T, long> SColor;
	public Dictionary<T, long> EColor;

	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.AppendLine("\tNorth =====");
		DColorToString(sb, NColor);
		sb.AppendLine("\tWest  =====");
		DColorToString(sb, WColor);
		sb.AppendLine("\tSouth =====");
		DColorToString(sb, SColor);
		sb.AppendLine("\tEast  =====");
		DColorToString(sb, EColor);
		return sb.ToString();
	}

	void DColorToString(StringBuilder sb, Dictionary<T, long> d)
	{
		foreach(var kvp in d) {
			sb.AppendLine($"\t{kvp.Key} #={kvp.Value}");
		}
	}
}
