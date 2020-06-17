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

	public struct IFColor
	{
		public IFColor(double r, double g, double b, double a)
		{
			R = r; G = g; B = b; A = a;
		}

		public double R;
		public double G;
		public double B;
		public double A;
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