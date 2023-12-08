using System.Collections;
using System.Drawing;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.ProbableImg;

[InternalRegisterFunction(nameof(ProbableImg))]
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

	public bool Run( string[] args)
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

		var source = Layers.First();
		var bounds = source.Bounds();

		using var progress = new ProgressBar();
		MethodBase m = O.UseNonLookup
			? new MethodTwo { O = O }
			: new MethodOne { O = O }
		;
		m.CreateProfile(progress,source,bounds);

		//foreach(var kvp in Profile) {
		//	Log.Debug($"Key = {kvp.Key}");
		//	Log.Debug(kvp.Value.ToString());
		//}

		var engine = Core.Engine.Item.Value;
		using var canvas = engine.NewCanvasFromLayers(Layers);
		m.CreateImage(progress,canvas);
		source.CopyFrom(canvas);

		return true;
	}

	readonly Options O = new();
	IRegister Register;
	ICoreOptions Core;
	ILayers Layers;
}


class ColorProfile<T>
{
	//dictionaries of index,count
	public Dictionary<T,long> NColor;
	public Dictionary<T,long> WColor;
	public Dictionary<T,long> SColor;
	public Dictionary<T,long> EColor;

	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.AppendLine("\tNorth =====");
		DColorToString(sb,NColor);
		sb.AppendLine("\tWest  =====");
		DColorToString(sb,WColor);
		sb.AppendLine("\tSouth =====");
		DColorToString(sb,SColor);
		sb.AppendLine("\tEast  =====");
		DColorToString(sb,EColor);
		return sb.ToString();
	}

	void DColorToString(StringBuilder sb, Dictionary<T,long> d)
	{
		foreach(var kvp in d) {
			sb.AppendLine($"\t{kvp.Key} #={kvp.Value}");
		}
	}
}