using ImageFunctions.Core;
using System.Collections;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.DistanceColoring;

internal class RandomSource() : IPointSource
{
	public ICanvas Image { get; set; }
	public Random Rnd { get; set; }

	public IEnumerator<Point> GetEnumerator()
	{
		// keep track of points so we don't have to search for empty spots later
		List<Point> allPoints = new(Image.Width * Image.Height);
		for(int y = 0; y < Image.Height; y++) {
			for(int x = 0; x < Image.Width; x++) {
				allPoints.Add(new Point(x, y));
			}
		}

		while(allPoints.Count > 0) {
			int ix = Rnd.Next(allPoints.Count);
			yield return allPoints[ix];

			//speedup - swap and remove it from the end of the array
			(allPoints[^1], allPoints[ix]) = (allPoints[ix], allPoints[^1]);
			allPoints.RemoveAt(allPoints.Count - 1);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
