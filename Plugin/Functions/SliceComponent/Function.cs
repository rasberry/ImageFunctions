using ImageFunctions.Core;
using ImageFunctions.Core.ColorSpace;
using ImageMagick;
using Rasberry.Cli;

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

		if (Layers.Count < 1) {
			Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}

		using var progress = new ProgressBar();
		var engine = Core.Engine.Item.Value;
		int numSlices = Options.WhichSlice.HasValue ? 1 : Options.Slices;

		//pull out the original which we'll replace with slices
		var original = Layers.PopAt(0, out _);
		var slices = new ICanvas[numSlices];
		for(int s=0; s < numSlices; s++) {
			slices[s] = engine.NewCanvas(original.Width, original.Height);
			Layers.Push(slices[s]);
		}

		Tools.ThreadPixels(original, (x,y) => {
			var c = Options.Space.ToSpace(original[x,y]);
			var ord = c.GetOrdinal(Options.ComponentName);
			double v = GetValue(ord,c);
			int index = Math.Clamp((int)(v * Options.Slices), 0, Options.Slices - 1);

			if (Options.WhichSlice.HasValue) {
				if (index + 1 != Options.WhichSlice.Value) { return; }
				index = 0;
			}

			IColor3 mc = Options.ResetValue.HasValue
				? WithValue(ord, c, Options.ResetValue.Value)
				: c
			;
			slices[index][x,y] = Options.Space.ToNative(mc);
		}, Core.MaxDegreeOfParallelism, progress);

		return true;
	}

	static double GetValue(ComponentOrdinal ord, IColor3 color)
	{
		return ord switch {
			ComponentOrdinal.C1 => color.C1,
			ComponentOrdinal.C2 => color.C2,
			ComponentOrdinal.C3 => color.C3,
			ComponentOrdinal.A  => color.A,
			_ => 0.0,
		};
	}

	//using ColorRGBA as a generic IColor3 - no reason to re-invent the wheel
	static ColorRGBA WithValue(ComponentOrdinal ord, IColor3 c, double v)
	{
		return ord switch {
			ComponentOrdinal.C1 => new ColorRGBA(v, c.C2, c.C3, c.A),
			ComponentOrdinal.C2 => new ColorRGBA(c.C1, v, c.C3, c.A),
			ComponentOrdinal.C3 => new ColorRGBA(c.C1, c.C2, v, c.A),
			ComponentOrdinal.A  => new ColorRGBA(c.C1, c.C2, c.C3, v),
			_ => new ColorRGBA(c.C1, c.C2, c.C3, c.A),
		};
	}

	readonly Options Options = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions Core;
}