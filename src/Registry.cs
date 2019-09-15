using System;
using System.Text;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace ImageFunctions
{
	public static class Registry
	{
		public static IFunction Map(Action action)
		{
			switch(action)
			{
			default:
			case Action.None: return null;
			case Action.PixelateDetails: return new PixelateDetails.Function();
			case Action.Derivatives: return new Derivatives.Function();
			case Action.AreaSmoother: return new AreaSmoother.Function();
			case Action.AreaSmoother2: return new AreaSmoother2.Function();
			case Action.ZoomBlur: return new ZoomBlur.Function();
			case Action.Swirl: return new Swirl.Function();
			case Action.Deform: return new Deform.Function();
			case Action.Encrypt: return new Encrypt.Function();
			case Action.PixelRules: return new PixelRules.Function();
			}
		}

		public static IResampler Map(Sampler sampler)
		{
			switch(sampler)
			{
			default:
			case Sampler.None: return null;
			case Sampler.Bicubic: return new BicubicResampler();
			case Sampler.Box: return new BoxResampler();
			case Sampler.CatmullRom: return new CatmullRomResampler();
			case Sampler.Hermite: return new HermiteResampler();
			case Sampler.Lanczos2: return new Lanczos2Resampler();
			case Sampler.Lanczos3: return new Lanczos3Resampler();
			case Sampler.Lanczos5: return new Lanczos5Resampler();
			case Sampler.Lanczos8: return new Lanczos8Resampler();
			case Sampler.MitchellNetravali: return new MitchellNetravaliResampler();
			case Sampler.NearestNeighbor: return new NearestNeighborResampler();
			case Sampler.Robidoux: return new RobidouxResampler();
			case Sampler.RobidouxSharp: return new RobidouxSharpResampler();
			case Sampler.Spline: return new SplineResampler();
			case Sampler.Triangle: return new TriangleResampler();
			case Sampler.Welch: return new WelchResampler();
			}
		}

		public static IResampler DefaultResampler { get {
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
