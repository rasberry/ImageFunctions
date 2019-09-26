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
			using (var compareImg = Image.Load(O.CompareImage))
			{
				var ab = Rectangle.Intersect(frame.Bounds(),compareImg.Bounds());
				var minimum = Rectangle.Intersect(ab,rectangle);

				MoreHelpers.ThreadPixels(minimum, config.MaxDegreeOfParallelism, (x,y) => {
					Rgba32 one = frame[x,y].ToColor();
					Rgba32 two = compareImg[x,y];
					bool areSame = one == two;
					bool sameSame = O.MatchSamePixels ^ areSame; //XOR

					if (O.OutputOriginal) {
						if (sameSame) {
							frame[x,y] = NamedColors<TPixel>.Transparent;
						}
					}
					else {
						if (!sameSame) {
							double dist; Rgba32 sc,ec;
							if (O.HilightOpacity == null) {
								dist = ColorDistanceRatio(one,two);
								sc = O.HilightColor;
								ec = NamedColors<Rgba32>.White;
							}
							else {
								dist = O.HilightOpacity.Value;
								sc = one;
								ec = O.HilightColor;
							}
							var overlay = ImageHelpers.BetweenColor(sc,ec,dist);
							frame[x,y] = overlay.FromColor<TPixel>();
						}
					}
				},progress);
			}
		}

		double ColorDistanceRatio(Rgba32 one, Rgba32 two)
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
