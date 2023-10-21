using System.Collections;
using ImageFunctions.Core;
using Rasberry.Cli;

namespace ImageFunctions.Plugin.Functions.FindBest;

[InternalRegisterFunction(nameof(FindBest))]
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

	public bool Run(string[] args)
	{
		var fr = new FunctionRegister(Register);
		//var regFunc = fr.Get("AllColors");
		//var difFunc = fr.Get("ImgDiff");

		Core.Engine.Item.Value.LoadImage(Layers,@"img-11-Default-1.png");
		var keep = Layers.First();

		object padlock = new object();
		double best = double.MaxValue;
		int besto = 0;
		int max = int.MaxValue / 256;
		using var pro = new ProgressBar();

		var po = new ParallelOptions();
		if (Core.MaxDegreeOfParallelism.HasValue) {
			po.MaxDegreeOfParallelism = Core.MaxDegreeOfParallelism.GetValueOrDefault(1);
		}

		int count = 0;
		//int count = 3243800;
		//int count = 6412400;
		var m1k = max / 100;
		Parallel.For(0,max,po,(i) => {
			int c = Interlocked.Increment(ref count);
			int o = (2 * c + 1) * 128;

			var tempLayers = new Layers();
			tempLayers.Push(keep);

			var ac = AllColors.Function.Create(Register, tempLayers, Core);
			string[] fargs = new[] { "-l","-on", o.ToString() };
			ac.Run(fargs);

			var dist = ImageComparer.CanvasDistance(keep, tempLayers.First());
			if (dist.Total < best) {
				lock(padlock) {
					if (dist.Total < best) {
						best = dist.Total; besto = o;
					}
				}
				Log.Error($"new best={best} besto={besto} c={c}");
			}

			//string [] dargs = new[] { "-nl" };
			//difFunc.Item.Value.Run(register, tempLayers, options, dargs); //adds a layer

			//tempLayers.DisposeAt(0); //remove diff layer
			tempLayers.DisposeAt(0); //remove allcolors layer
			tempLayers.PopAt(0,out _); //pop loaded image so it doesn't get disposed

			string m = $"best={best:0.0} besto={besto} {count/100}/{m1k} c={c}";
			m += new string(' ',Math.Max(1,70 - m.Length));

			pro.Prefix = m;
			pro.Report((double)count / max);
			//Thread.Sleep(100);
		});

		Log.Message($"best={best} besto={besto}");

		return true;
	}

	public void Usage(StringBuilder sb)
	{
		sb.Append("NO USAGE");
	}

	IRegister Register;
	ICoreOptions Core;
	ILayers Layers;
}
