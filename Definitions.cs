using System;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace ImageFunctions
{
	public enum Action
	{
		None = 0,
		PixelateDetails = 1,
		Derivatives = 2,
		AreaSmoother = 3,
		AreaSmoother2 = 4,
		ZoomBlur = 5,
		Swirl = 6
	}

	public interface IFunction
	{
		void Usage(StringBuilder sb);
		bool ParseArgs(string[] args);
		Rectangle Rect { get; set; }
		int? MaxDegreeOfParallelism { get; set; }
		void Main();
	}

	public enum Sampler
	{
		None = 0,
		NearestNeighbor = 1,
		Bicubic = 2,
		Box = 3,
		CatmullRom = 4,
		Hermite = 5,
		Lanczos2 = 6,
		Lanczos3 = 7,
		Lanczos5 = 8,
		Lanczos8 = 9,
		MitchellNetravali = 10,
		Robidoux = 11,
		RobidouxSharp = 12,
		Spline = 13,
		Triangle = 14,
		Welch = 15
	}

	public interface IHasResampler
	{
		IResampler Sampler { get; set; }
	}


	public enum Metric
	{
		None = 0,
		Manhattan = 1,
		Euclidean = 2,
		Chebyshev = 3,
		ChebyshevInv = 4,
		Minkowski = 5,
		Canberra = 6
	}

	public delegate double MetricFunction(double x1, double y1, double x2, double y2);

	public interface IHasDistance
	{
		MetricFunction Measurer { get; set; }
	}
}