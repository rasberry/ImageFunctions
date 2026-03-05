using ImageFunctions.Core;
using System.Collections;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.DistanceColoring;

internal class SequentialSource : IPointSource
{
	public ICanvas Image { get; set; }
	public Random Rnd { get; set; }

	public IEnumerator<Point> GetEnumerator()
	{
		for(int y = 0; y < Image.Height; y++) {
			for(int x = 0; x < Image.Width; x++) {
				yield return new Point(x, y);
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
