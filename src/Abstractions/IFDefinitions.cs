using System;
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
		public double R;
		public double G;
		public double B;
		public double A;
	}

	public interface IFFunction
	{
		void Usage(StringBuilder sb);
		bool ParseArgs(string[] args);
		IFRectangle Bounds { get; set; }
		int? MaxDegreeOfParallelism { get; set; }
		void Main();
	}

	public struct IFRectangle
	{
		public int X;
		public int Y;
		public int Width;
		public int Height;

		public int Top { get { return Y; }}
		public int Bottom { get { return Y + Height; }}
		public int Left { get { return X; }}
		public int Right { get { return X + Width; }}

		public bool IsEmpty { get {
			return X == 0 && Y == 0 && Width == 0 && Height == 0;
		}}
	}

	public struct IFPoint
	{
		public int X;
		public int Y;
	}

	public struct IFSize
	{
		public int Width;
		public int Height;
	}

	public interface IFImageConfig
	{
		IFImage LoadImage(string path);
		void SaveImage(IFImage img, string path);
		IFImage NewImage(int width, int Height);
	}

	public interface IFGenerator
	{
		IFSize StartingSize { get; }
	}
}