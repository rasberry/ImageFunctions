using ImageFunctions.Core;
using System.Drawing;

namespace ImageFunctions.Plugin.Functions.DistanceColoring;

public interface IPointSource : IEnumerable<Point>
{
	ICanvas Image { get; set; }
	Random Rnd { get; set; }
}

public enum PlacementKind
{
	None = 0,
	Random = 1,
	Sequential = 2,
	Spiral = 3
}