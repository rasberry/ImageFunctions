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
				var colorWhite = Color.White.ToPixel<RgbaD>();
				var colorHilight = O.HilightColor.ToPixel<RgbaD>();

				MoreHelpers.ThreadPixels(minimum, config.MaxDegreeOfParallelism, (x,y) => {
					var one = frame[x,y].ToColor();
					var two = compareImg[x,y].ToColor();
					bool areSame = one == two;
					bool sameSame = O.MatchSamePixels ^ areSame; //XOR

					if (O.OutputOriginal) {
						if (sameSame) {
							frame[x,y] = Color.Transparent.ToPixel<TPixel>();
						}
					}
					else {
						if (!sameSame) {
							double dist; RgbaD sc,ec;
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
							frame[x,y] = overlay.FromColor<TPixel>();
						}
					}
				},progress);
				Log.Message("total distance = "+totalDist);
			}
		}

		double ColorDistanceRatio(RgbaD one, RgbaD two)
		{
			var vo = new double[] { one.R, one.B, one.G, one.A };
			var vt = new double[] { two.R, two.B, two.G, two.A };
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
