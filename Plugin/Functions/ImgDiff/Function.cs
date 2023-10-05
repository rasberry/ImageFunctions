using System.Drawing;
using ImageFunctions.Core;
using O = ImageFunctions.Plugin.Functions.ImgDiff.Options;


namespace ImageFunctions.Plugin.Functions.ImgDiff;

[InternalRegisterFunction(nameof(ImgDiff))]
public class Function : IFunction
{
	public void Usage(StringBuilder sb)
	{
		O.Usage(sb);
	}

	public bool Run(IRegister register, ILayers layers, string[] args)
	{
		if (layers == null) {
			throw Squeal.ArgumentNull(nameof(layers));
		}
		if (!O.ParseArgs(args, register)) {
			return false;
		}

		if (layers.Count < 2) {
			Tell.LayerMustHaveAtLeast(2);
			return false;
		}

		const int topIx = 0;
		const int nextIx = 1;
		var frame = layers[topIx];
		var compareImg = layers[nextIx];
		using var progress = new Rasberry.Cli.ProgressBar();

		double totalDist = 0.0;
		var minimum = Rectangle.Intersect(frame.Bounds(), compareImg.Bounds());
		var colorWhite = PlugColors.White;
		var colorHilight = O.HilightColor;
		var colorTransp = PlugColors.Transparent;
		InitMetric();

		Tools.ThreadPixels(minimum, (x,y) => {
			var one = frame[x,y];
			var two = compareImg[x,y];
			bool areSame = one.Equals(two);
			//toggle matching of different pixels vs same pixels
			bool sameSame = O.MatchSamePixels ^ areSame; //XOR

			//option to output original pixels if they 'match'
			if (O.OutputOriginal) {
				if (sameSame) {
					frame[x,y] = colorTransp;
				}
			}
			//otherwise highlight 'unmatched' pixels
			else if (!sameSame) {
				double dist; ColorRGBA sc,ec;
				if (O.HilightOpacity == null) {
					dist = ColorDistanceRatio(one,two);
					sc = colorHilight;
					ec = colorWhite;
				}
				else {
					dist = O.HilightOpacity.Value;
					sc = one;
					ec = colorHilight;
				}
				totalDist += dist;
				var overlay = PlugTools.BetweenColor(sc,ec,dist);
				frame[x,y] = overlay;
			}
			//otherwise leave empty
		},progress);

		layers.PopAt(nextIx);
		Log.Message($"{nameof(ImgDiff)} - total distance = {totalDist}");
		return true;
	}

	double ColorDistanceRatio(ColorRGBA one, ColorRGBA two)
	{
		var vo = new double[] { one.R, one.B, one.G, one.A };
		var vt = new double[] { two.R, two.B, two.G, two.A };
		//TODO consider other metrics ?
		double dist = O.MetricInstance.Value.Measure(vo,vt);
		return dist / DistanceMax;
	}

	//max distance for rgba color
	double DistanceMax;
	void InitMetric()
	{
		DistanceMax = O.MetricInstance.Value.Measure(
			new double[] { 0.0,0.0,0.0,0.0 },
			new double[] { 1.0,1.0,1.0,1.0 }
		);
	}
}
