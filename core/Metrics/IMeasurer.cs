namespace ImageFunctions.Core.Metrics;

public interface IMetric
{
	double Measure(double x1, double y1, double x2, double y2);
	double Measure(double[] u, double[] v);
}