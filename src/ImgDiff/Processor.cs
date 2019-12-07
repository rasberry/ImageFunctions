using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageFunctions.ImgDiff
{
	public class Processor<TPixel> : AbstractProcessor<TPixel>
		where TPixel : struct, IPixel<TPixel>
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

		protected override void Apply(ImageFrame<TPixel> frame, Rectangle rectangle, Configuration config)
		{
			using (var progress = new ProgressBar())
			using (var compareImg = Image.Load<TPixel>(O.CompareImage))
			{
				double totalDist = 0.0;
				var ab = Rectangle.Intersect(frame.Bounds(),compareImg.Bounds());
				var minimum = Rectangle.Intersect(ab,rectangle);
				var colorWhite = Color.White.ToPixel<TPixel>();
				var colorHilight = O.HilightColor.ToPixel<TPixel>();
				var colorTransp = Color.Transparent.ToPixel<TPixel>();

				MoreHelpers.ThreadPixels(minimum, config.MaxDegreeOfParallelism, (x,y) => {
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
						double dist; TPixel sc,ec;
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

		double ColorDistanceRatio(TPixel one, TPixel two)
		{
			var vone = one.ToColor();
			var vtwo = two.ToColor();
			var vo = new double[] { vone.R, vone.B, vone.G, vone.A };
			var vt = new double[] { vtwo.R, vtwo.B, vtwo.G, vtwo.A };
			double dist = MetricHelpers.DistanceEuclidean(vo,vt);
			return dist / DistanceMax;
		}

		//max distance for rgba color
		static double DistanceMax = MetricHelpers.DistanceEuclidean(
			new double[] { 0.0,0.0,0.0,0.0 },
			new double[] { 255.0,255.0,255.0,255.0 }
		);
	}
}
