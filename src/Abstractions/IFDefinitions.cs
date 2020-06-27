using System;
using System.Drawing;
using System.Text;

namespace ImageFunctions
{
	public interface IFImage : IDisposable
	{
		int Width { get; }
		int Height { get; }

		IFColor this[int x, int y] { get; set; }
	}

	public readonly struct IFColor
	{
		public IFColor(double r, double g, double b, double a) {
			R = r; G = g; B = b; A = a;
		}

		public readonly double R,G,B,A;

		//public static double MinValue = 0.0;
		//public static double MaxValue = 1.0;
	}

	public interface IFFunction
	{
		void Usage(StringBuilder sb);
		bool ParseArgs(string[] args);
		Rectangle Bounds { get; set; }
		int? MaxDegreeOfParallelism { get; set; }
		void Main();
	}

	public interface IFImageConfig
	{
		IFImage LoadImage(string path);
		void SaveImage(IFImage img, string path);
		IFImage NewImage(int width, int height);
	}

	public interface IFGenerator
	{
		Size StartingSize { get; }
	}
}