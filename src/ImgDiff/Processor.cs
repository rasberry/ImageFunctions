using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using ImageFunctions.Helpers;

namespace ImageFunctions.ImgDiff
{
	public class Processor : AbstractProcessor
	{
		public Options O = null;

		// OO = O.OutputOriginal
		// MC = O.MatchSamePixels
		// AS = are same
		// OO MC AS
		// 0  0  0	hilight
		// 0  0  1	do nothing
		// 0  1  0	do nothing
		// 0  1  1	hilight
		// 1  0  0	do nothing
		// 1  0  1	transparent
		// 1  1  0	transparent
		// 1  1  1	do nothing

		public override void Apply()
		{
			var Iis = Engines.Engine.GetConfig();
			var frame = Source;
			using (var progress = new ProgressBar())
			using (var compareImg = Iis.LoadImage(O.CompareImage))
			{
				double totalDist = 0.0;
				var ab = Rectangle.Intersect(
					new Rectangle(0,0,frame.Width,frame.Height),
					new Rectangle(0,0,compareImg.Width,compareImg.Height)
				);
				var minimum = Rectangle.Intersect(ab,this.Bounds);
				var colorWhite = Helpers.ColorHelpers.White;
				var colorHilight = O.HilightColor;
				var colorTransp = Helpers.ColorHelpers.Transparent;

				MoreHelpers.ThreadPixels(minimum, MaxDegreeOfParallelism, (x,y) => {
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
						double dist; IColor sc,ec;
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
						var overlay = ImageHelpers.BetweenColor(sc,ec,dist);
						frame[x,y] = overlay;
					}
					//otherwise leave empty
				},progress);
				Log.Message("total distance = "+totalDist);
			}
		}

		double ColorDistanceRatio(IColor one, IColor two)
		{
			var vo = new double[] { one.R, one.B, one.G, one.A };
			var vt = new double[] { two.R, two.B, two.G, two.A };
			//TODO consider other metrics ?
			double dist = MetricHelpers.DistanceEuclidean(vo,vt);
			return dist / DistanceMax;
		}

		//max distance for rgba color
		static double DistanceMax = MetricHelpers.DistanceEuclidean(
			new double[] { 0.0,0.0,0.0,0.0 },
			new double[] { 255.0,255.0,255.0,255.0 }
		);

		public override void Dispose() {}
	}
}
