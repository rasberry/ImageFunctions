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

	#if false
	public struct IFRectangle
	{
		public IFRectangle(int x,int y,int w,int h)
		{
			X = x; Y = y;
			Width = w; Height = h;
		}

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

		public IFRectangle Intersect(IFRectangle rect)
		{
			return Intersect(this,rect);
		}

		public bool Contains(int x,int y)
		{
			return
				X <= x &&
				Y <= y &&
				x < X + Width &&
				y < Y + Height
			;
		}

		public static IFRectangle Intersect(IFRectangle a, IFRectangle b)
		{
			int x1 = Math.Max(a.X, b.X);
			int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
			int y1 = Math.Max(a.Y, b.Y);
			int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

			if (x2 >= x1 && y2 >= y1) {
				return new IFRectangle(x1, y1, x2 - x1, y2 - y1);
			}
			return Empty;
		}

		public static IFRectangle Empty { get {
			return new IFRectangle(0,0,0,0);
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
	#endif

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