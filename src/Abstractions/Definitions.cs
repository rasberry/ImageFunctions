using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImageFunctions
{
	public enum Activity
	{
		None = 0,
		PixelateDetails = 1,
		Derivatives = 2,
		AreaSmoother = 3,
		AreaSmoother2 = 4,
		ZoomBlur = 5,
		Swirl = 6,
		Deform = 7,
		Encrypt = 8,
		PixelRules = 9,
		ImgDiff = 10,
		AllColors = 11,
		SpearGraphic = 12,
		ColatzVis = 13,
		UlamSpiral = 14,
		Maze = 15,
		Test = 99
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

	public enum PickEdgeRule
	{
		Edge = 0,
		Reflect = 1,
		Tile = 2,
		Transparent = 3
	}

	public enum PickEngine
	{
		None = 0,
		ImageMagick = 1,
		SixLabors = 2
	}

	public interface IHasSampler
	{
		ISampler Sampler { get; }
	}

	public interface ISampler
	{
		IColor GetSample(IImage img, int x, int y);
		double Radius { get; }
		double GetKernelAt(double x);
		Sampler Kind { get; }
		double Scale { get; }
		PickEdgeRule EdgeRule { get; }
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

	public interface IMeasurer
	{
		double Measure(double x1, double y1, double x2, double y2);
		double Measure(double[] u, double[] v);
	}

	public interface IHasDistance
	{
		IMeasurer Measurer { get; }
	}

	public enum Direction
	{
		None = 0,
		N,NE,E,SE,S,SW,W,NW
	}

	public readonly struct PointD
	{
		public PointD(double x,double y) {
			X = x; Y = y;
		}
		public readonly double X;
		public readonly double Y;
	}

	public interface IImage : IDisposable
	{
		int Width { get; }
		int Height { get; }

		IColor this[int x, int y] { get; set; }
	}

	public readonly struct IColor
	{
		public IColor(double r, double g, double b, double a) {
			R = r; G = g; B = b; A = a;
		}

		public readonly double R,G,B,A;

		public override string ToString() {
			return $"{nameof(IColor)} [{R},{G},{B},{A}]";
		}
	}

	public interface IFunction
	{
		void Usage(StringBuilder sb);
		bool ParseArgs(string[] args);
		Rectangle Bounds { get; set; }
		int? MaxDegreeOfParallelism { get; set; }
		void Main();
	}

	public interface IImageEngine
	{
		IImage LoadImage(string path);
		void SaveImage(IImage img, string path, string format = null);
		IImage NewImage(int width, int height);
	}

	public interface IGenerator
	{
		Size StartingSize { get; }
	}

	public interface IDrawEngine
	{
		void DrawLine(IImage image, IColor color, PointD p0, PointD p1, double width = 1.0);
	}

	public interface IFormatGuide
	{
		IEnumerable<string> ListFormatNames();
		string GetFormatDescription(string formatName);
	}
}