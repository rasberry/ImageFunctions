namespace ImageFunctions.Core.Samplers;

public enum PickEdgeRule
{
	Edge = 0, //Clamp
	Reflect = 1,
	Tile = 2,
	Transparent = 3
}

public interface ISampler
{
	ColorRGBA GetSample(ICanvas img, int x, int y);
	double Radius { get; }
	double Scale { get; set; }
	PickEdgeRule EdgeRule { get; set; }
}

//public interface IMeasurer
//{
//	double Measure(double x1, double y1, double x2, double y2);
//	double Measure(double[] u, double[] v);
//}
