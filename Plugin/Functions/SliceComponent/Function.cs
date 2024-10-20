using ImageFunctions.Core;
using ImageFunctions.Core.Aides;
using ImageFunctions.Core.ColorSpace;
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

	public IOptions Options { get { return O; } }

	public bool Run(string[] args)
	{
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!O.ParseArgs(args, Register)) {
			return false;
		}

		if(Layers.Count < 1) {
			Log.Error(Note.LayerMustHaveAtLeast());
			return false;
		}

		using var progress = new ProgressBar();
		var engine = Core.Engine.Item.Value;
		int numSlices = O.WhichSlice.HasValue ? 1 : O.Slices;

		//pull out the original which we'll replace with slices
		var original = Layers.PopAt(0).Canvas;
		var slices = new ICanvas[numSlices];
		for(int s = 0; s < numSlices; s++) {
			slices[s] = engine.NewCanvas(original.Width, original.Height);
			Layers.Push(slices[s]);
		}

		ImageAide.ThreadPixels(original, (x, y) => {
			var c = O.Space.ToSpace(original[x, y]);
			var ord = c.GetOrdinal(O.ComponentName);
			double v = GetValue(ord, c);
			int index = Math.Clamp((int)(v * O.Slices), 0, O.Slices - 1);

			if(O.WhichSlice.HasValue) {
				if(index + 1 != O.WhichSlice.Value) { return; }
				index = 0;
			}

			IColor3 mc = O.ResetValue.HasValue
				? WithValue(ord, c, O.ResetValue.Value)
				: c
			;
			slices[index][x, y] = O.Space.ToNative(mc);
		}, Core.MaxDegreeOfParallelism, progress);

		return true;
	}

	static double GetValue(ComponentOrdinal ord, IColor3 color)
	{
		return ord switch {
			ComponentOrdinal.C1 => color.C1,
			ComponentOrdinal.C2 => color.C2,
			ComponentOrdinal.C3 => color.C3,
			ComponentOrdinal.A => color.A,
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
			ComponentOrdinal.A => new ColorRGBA(c.C1, c.C2, c.C3, v),
			_ => new ColorRGBA(c.C1, c.C2, c.C3, c.A),
		};
	}

	readonly Options O = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions Core;
}
