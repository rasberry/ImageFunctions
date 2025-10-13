using ImageFunctions.Core;
using ImageFunctions.Plugin.Aides;
using System.Collections;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.DistanceColoring;

internal class SpiralSource : IPointSource
{
	public ICanvas Image { get; set;  }
	public Random Rnd { get; set; }

	public IEnumerator<Point> GetEnumerator()
	{
		int max = Image.Height * Image.Width;
		for(int i = 0; i < max; i++) {
			var (x, y) = MathAidePlus.SpiralToXY(i, Image.Width, Image.Height);
			yield return new Point(x, y);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
