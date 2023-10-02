using ImageFunctions.Core.Attributes;

namespace ImageFunctions.Core.Metrics;

public class MetricRegister : AbstractRegistrant<Lazy<IMetric>>
{
	public MetricRegister(IRegister register) : base(register)
	{
		//Nothing to do
	}

	public override string Namespace { get { return "Metric"; }}

	[InternalRegister]
	internal static void Register(IRegister register)
	{
		var reg = new MetricRegister(register);
		reg.Add("Euclidean"   ,new Lazy<IMetric>(() => new MetricWrap(DistanceEuclidean,DistanceEuclidean)));
		reg.Add("Canberra"    ,new Lazy<IMetric>(() => new MetricWrap(DistanceCanberra,DistanceCanberra)));
		reg.Add("Manhattan"   ,new Lazy<IMetric>(() => new MetricWrap(DistanceManhattan,DistanceManhattan)));
		reg.Add("Chebyshev"   ,new Lazy<IMetric>(() => new MetricWrap(Chebyshev,Chebyshev)));
		reg.Add("ChebyshevInv",new Lazy<IMetric>(() => new MetricWrap(ChebyshevInv,ChebyshevInv)));
	}

	class MetricWrap : IMetric
	{
		public delegate double MeasureTwo(double x1, double y1, double x2, double y2);
		public delegate double MeasureMore(double[] u, double[] v);

		public MetricWrap(MeasureTwo two,MeasureMore more) {
			Two = two; More = more;
		}

		MeasureTwo Two;
		MeasureMore More;

		public double Measure(double x1, double y1, double x2, double y2) {
			return Two(x1,y1,x2,y2);
		}
		public double Measure(double[] u, double[] v) {
			return More(u,v);
		}
	}


	static double DistanceManhattan(double x1, double y1, double x2, double y2)
	{
		return Math.Abs(y2 - y1) + Math.Abs(x2 - x1);
	}

	static double DistanceManhattan(double[] p1, double[] p2)
	{
		if (p1.Length != p2.Length) {
			throw Squeal.ArgumentsMustBeEqual<int>("Length",p1.Length,p2.Length);
		}
		double total = 0;
		for (int p = 0; p < p1.Length; p++) {
			total += Math.Abs(p2[p] - p1[p]);
		}
		return total;
	}

	static double DistanceEuclidean(double x1, double y1, double x2, double y2)
	{
		double dx = x2 - x1;
		double dy = y2 - y1;
		return Math.Sqrt(dx * dx + dy * dy);
	}

	static double DistanceEuclidean(double[] p1, double[] p2)
	{
		if (p1.Length != p2.Length) {
			throw Squeal.ArgumentsMustBeEqual<int>("Length",p1.Length,p2.Length);
		}
		double total = 0;
		for (int p = 0; p < p1.Length; p++) {
			double d = p2[p] - p1[p];
			total += d * d;
		}
		return Math.Sqrt(total);
	}

	static double DistanceChebyshev(double x1, double y1, double x2, double y2, bool invert = false)
	{
		double dx = Math.Abs(x2 - x1);
		double dy = Math.Abs(y2 - y1);

		return invert ? Math.Min(dx, dy) : Math.Max(dx, dy);
	}

	static double DistanceChebyshev(double[] p1, double[] p2, bool invert = false)
	{
		if (p1.Length != p2.Length) {
			throw Squeal.ArgumentsMustBeEqual<int>("Length",p1.Length,p2.Length);
		}
		double total = invert ? double.MaxValue : double.MinValue;
		for (int p = 0; p < p1.Length; p++) {
			double d = Math.Abs(p2[p] - p1[p]);
			total = invert ? Math.Min(total, d) : Math.Max(total, d);
		}
		return total;
	}

	static double DistanceCanberra(double x1, double y1, double x2, double y2)
	{
		double xden = x2 + x1;
		double yden = y2 + y1;
		//deal with singularities
		double dx = Math.Abs(xden) < double.Epsilon ? 0.0 : (x2 - x1) / xden;
		double dy = Math.Abs(yden) < double.Epsilon ? 0.0 : (y2 - y1) / yden;
		return dx + dy;
	}

	static double DistanceCanberra(double[] p1, double[] p2)
	{
		if (p1.Length != p2.Length) {
			throw Squeal.ArgumentsMustBeEqual<int>("Length",p1.Length,p2.Length);
		}
		double total = 0;
		for (int p = 0; p < p1.Length; p++) {
			double den = p2[p] + p1[p];
			//deal with singularities
			double d = Math.Abs(den) < double.Epsilon ? 0.0 : (p2[p] - p1[p]) / den;
			total += d;
		}
		return total;
	}

	static double Chebyshev(double x1,double y1,double x2,double y2) {
		return DistanceChebyshev(x1,y1,x2,y2,false);
	}
	static double Chebyshev(double[] u, double[] v) {
		return DistanceChebyshev(u,v,false);
	}
	static double ChebyshevInv(double x1,double y1,double x2,double y2) {
		return DistanceChebyshev(x1,y1,x2,y2,true);
	}
	static double ChebyshevInv(double[] u, double[] v) {
		return DistanceChebyshev(u,v,true);
	}
}