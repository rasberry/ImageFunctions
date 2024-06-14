using ImageFunctions.Core;
using Rasberry.Cli;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.ImgDiff;

[InternalRegisterFunction(nameof(ImgDiff))]
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

	public IOptions Options { get { return O; }}

	public bool Run(string[] args)
	{
		if(Layers == null) {
			throw Squeal.ArgumentNull(nameof(Layers));
		}
		if(!O.ParseArgs(args, Register)) {
			return false;
		}

		if(Layers.Count < 2) {
			Log.Error(Note.LayerMustHaveAtLeast(2));
			return false;
		}

		const int topIx = 0;
		const int nextIx = 1;

		var srcImg = Layers[topIx].Canvas;
		var compareImg = Layers[nextIx].Canvas;
		ICanvas frame = O.MakeThirdLayer
			? Core.Engine.Item.Value.NewCanvasFromLayers(Layers)
			: srcImg;

		InitMetric();

		var totalDist = ProcessDiff(frame, srcImg, compareImg);
		Log.Message($"{nameof(ImgDiff)} - total distance = {totalDist}");

		if(O.MakeThirdLayer) {
			Layers.Push(frame);
		}
		else {
			Layers.DisposeAt(nextIx);
		}

		return true;
	}

	double ProcessDiff(ICanvas frame, ICanvas srcImg, ICanvas compareImg)
	{
		var minimum = Rectangle.Intersect(frame.Bounds(), compareImg.Bounds());
		var colorWhite = PlugColors.White;
		var colorHilight = O.HilightColor;
		var colorTransp = PlugColors.Transparent;
		double totalDist = 0.0;
		var progress = new ProgressBar();

		Tools.ThreadPixels(minimum, (x, y) => {
			var one = srcImg[x, y];
			var two = compareImg[x, y];
			bool areSame = one.Equals(two);
			//toggle matching of different pixels vs same pixels
			bool sameSame = O.MatchSamePixels ^ areSame; //XOR

			//option to output original pixels if they 'match'
			if(O.OutputOriginal) {
				if(sameSame) {
					frame[x, y] = colorTransp;
				}
			}
			//otherwise highlight 'unmatched' pixels
			else if(!sameSame) {
				double dist; ColorRGBA sc, ec;
				if(O.HilightOpacity == null) {
					dist = ColorDistanceRatio(one, two);
					sc = colorHilight;
					ec = colorWhite;
				}
				else {
					dist = O.HilightOpacity.Value;
					sc = one;
					ec = colorHilight;
				}
				totalDist += dist;
				var overlay = PlugTools.BetweenColor(sc, ec, dist);
				frame[x, y] = overlay;
			}
			//otherwise leave empty
		}, Core.MaxDegreeOfParallelism, progress);

		return totalDist;
	}

	double ColorDistanceRatio(ColorRGBA one, ColorRGBA two)
	{
		var dist = ImageComparer.ColorDistance(one, two, O.MetricInstance.Value);
		return dist.Total / DistanceMax;
	}

	//max distance for rgba color
	double DistanceMax;
	void InitMetric()
	{
		DistanceMax = ImageComparer.Max(O.MetricInstance.Value);
	}

	readonly Options O = new();
	IRegister Register;
	ILayers Layers;
	ICoreOptions Core;
}
