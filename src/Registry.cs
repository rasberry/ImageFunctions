using System;
using System.Text;
using ImageFunctions.Helpers;

namespace ImageFunctions
{
	public static class Registry
	{
		public static IFFunction Map(Activity action)
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
			}
			throw new ArgumentException("E: Unmapped action "+action);
		}

		public static IFResampler Map(Sampler s,double scale = 1.5, PickEdgeRule edgeRule = PickEdgeRule.Edge)
		{
			return SampleHelpers.Map(s,scale,edgeRule);
		}

		public static IFResampler DefaultResampler { get {
			return Map(Sampler.NearestNeighbor);
		}}

		public static IFResampler DefaultIFResampler { get {
			return Map(Sampler.NearestNeighbor);
		}}

		public static IMeasurer Map(Metric m, double pFactor = 2.0) {
			return MetricHelpers.Map(m,pFactor);
		}

		public static IMeasurer DefaultMetric { get {
			return Map(Metric.Euclidean);
		}}
	}
}
