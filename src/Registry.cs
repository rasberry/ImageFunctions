using System;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions
{
	public static class Registry
	{
		public static IFunction Map(Activity action)
		{
			switch(action)
			{
			case Activity.PixelateDetails: return new PixelateDetails.Function();
			case Activity.Derivatives:     return new Derivatives.Function();
			case Activity.AreaSmoother:    return new AreaSmoother.Function();
			case Activity.AreaSmoother2:   return new AreaSmoother2.Function();
			case Activity.ZoomBlur:        return new ZoomBlur.Function();
			case Activity.Swirl:           return new Swirl.Function();
			case Activity.Deform:          return new Deform.Function();
			case Activity.Encrypt:         return new Encrypt.Function();
			case Activity.PixelRules:      return new PixelRules.Function();
			case Activity.ImgDiff:         return new ImgDiff.Function();
			case Activity.AllColors:       return new AllColors.Function();
			case Activity.SpearGraphic:    return new SpearGraphic.Function();
			case Activity.ColatzVis:       return new ColatzVis.Function();
			case Activity.UlamSpiral:      return new UlamSpiral.Function();
			case Activity.GraphNet:        return new GraphNet.Function();
			case Activity.Maze:            return new Maze.Function();
			case Activity.ProbableImg:     return new ProbableImg.Function();
			#if DEBUG
			case Activity.Playground:      return new Playground.Function();
			#endif
			}
			throw new ArgumentException("E: Unmapped action "+action);
		}

		public static ISampler Map(Sampler s,double scale = 1.5, PickEdgeRule edgeRule = PickEdgeRule.Edge)
		{
			return SampleHelpers.Map(s,scale,edgeRule);
		}

		public static ISampler DefaultResampler { get {
			return Map(Sampler.NearestNeighbor);
		}}

		public static ISampler DefaultIFResampler { get {
			return Map(Sampler.NearestNeighbor);
		}}

		public static IMeasurer Map(Metric m, double pFactor = 2.0) {
			return MetricHelpers.Map(m,pFactor);
		}

		public static IMeasurer DefaultMetric { get {
			return Map(Metric.Euclidean);
		}}

		static IImageEngine CachedImageEngine = null;
		public static IImageEngine GetImageEngine(PickEngine specific = PickEngine.None)
		{
			if (specific == PickEngine.None && CachedImageEngine != null) {
				return CachedImageEngine;
			}

			if (specific == PickEngine.None) {
				specific = Options.Engine;
			}

			switch(specific)
			{
			case PickEngine.ImageMagick:
				return CachedImageEngine = new Engines.ImageMagick.IMImageEngine();
			case PickEngine.SixLabors:
				return CachedImageEngine = new Engines.SixLabors.SLImageEngine();
			}
			throw new NotSupportedException($"Engine {Options.Engine} is not supported as an image engine");
		}

		public static IDrawEngine GetDrawEngine()
		{
			var iie = GetImageEngine();
			var ide = iie as IDrawEngine;
			if (ide == null) {
				throw new NotSupportedException($"Engine {Options.Engine} is not supported as a draw engine");
			}
			return ide;
		}

		public static IFormatGuide GetFormatGuide()
		{
			var iie = GetImageEngine();
			var ifg = iie as IFormatGuide;
			if (ifg == null) {
				throw new NotSupportedException($"Engine {Options.Engine} is not supported as a format guide");
			}
			return ifg;
		}

	}
}
