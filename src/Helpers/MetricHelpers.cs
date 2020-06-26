using System;
using System.Collections.Generic;
using System.Text;

namespace ImageFunctions.Helpers
{
	public static class MetricHelpers
	{
		public static double DistanceManhattan(double x1, double y1, double x2, double y2)
		{
			return Math.Abs(y2 - y1) + Math.Abs(x2 - x1);
		}

		public static double DistanceManhattan(double[] p1, double[] p2)
		{
			if (p1.Length != p2.Length) {
				throw new ArgumentException();
			}
			double total = 0;
			for (int p = 0; p < p1.Length; p++) {
				total += Math.Abs(p2[p] - p1[p]);
			}
			return total;
		}

		public static double DistanceEuclidean(double x1, double y1, double x2, double y2)
		{
			double dx = x2 - x1;
			double dy = y2 - y1;
			return Math.Sqrt(dx * dx + dy * dy);
		}

		public static double DistanceEuclidean(double[] p1, double[] p2)
		{
			if (p1.Length != p2.Length) {
				throw new ArgumentException();
			}
			double total = 0;
			for (int p = 0; p < p1.Length; p++) {
				double d = p2[p] - p1[p];
				total += d * d;
			}
			return Math.Sqrt(total);
		}

		public static double DistanceChebyshev(double x1, double y1, double x2, double y2, bool invert = false)
		{
			double dx = Math.Abs(x2 - x1);
			double dy = Math.Abs(y2 - y1);

			return invert ? Math.Min(dx, dy) : Math.Max(dx, dy);
		}

		public static double DistanceChebyshev(double[] p1, double[] p2, bool invert = false)
		{
			if (p1.Length != p2.Length) {
				throw new ArgumentException();
			}
			double total = invert ? double.MaxValue : double.MinValue;
			for (int p = 0; p < p1.Length; p++) {
				double d = Math.Abs(p2[p] - p1[p]);
				total = invert ? Math.Min(total, d) : Math.Max(total, d);
			}
			return total;
		}

		public static double DistanceCanberra(double x1, double y1, double x2, double y2)
		{
			double xden = x2 + x1;
			double yden = y2 + y1;
			//deal with singularities
			double dx = Math.Abs(xden) < double.Epsilon ? 0.0 : (x2 - x1) / xden;
			double dy = Math.Abs(yden) < double.Epsilon ? 0.0 : (y2 - y1) / yden;
			return dx + dy;
		}

		public static double DistanceCanberra(double[] p1, double[] p2)
		{
			if (p1.Length != p2.Length) {
				throw new ArgumentException();
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

		public static double DistanceMinkowski(double x1, double y1, double x2, double y2, double p)
		{
			if (double.IsPositiveInfinity(p)) {
				return DistanceChebyshev(x1, y1, x2, y2);
			}
			else if (double.IsNegativeInfinity(p)) {
				return DistanceChebyshev(x1, y1, x2, y2, true);
			}
			else if (Math.Abs(p - 1.0) < double.Epsilon) {
				return DistanceManhattan(x1, y1, x2, y2);
			}
			else if (Math.Abs(p - 2.0) < double.Epsilon) {
				return DistanceEuclidean(x1, y1, x2, y2);
			}
			else {
				double dx = Math.Abs(x2 - x1);
				double dy = Math.Abs(y2 - y1);
				return Math.Pow(Math.Pow(dx, p) + Math.Pow(dy, p), 1 / p);
			}
		}

		public static double DistanceMinkowski(double[] p1, double[] p2, double p)
		{
			if (p1.Length != p2.Length) {
				throw new ArgumentException();
			}

			if (double.IsPositiveInfinity(p)) {
				return DistanceChebyshev(p1,p2);
			}
			else if (double.IsNegativeInfinity(p)) {
				return DistanceChebyshev(p1,p2, true);
			}
			else if (Math.Abs(p - 1.0) < double.Epsilon) {
				return DistanceManhattan(p1,p2);
			}
			else if (Math.Abs(p - 2.0) < double.Epsilon) {
				return DistanceEuclidean(p1,p2);
			}
			else {
				double total = 0;
				for (int i = 0; i < p1.Length; i++) {
					double d = Math.Abs(p2[i] - p1[i]);
					total += Math.Pow(d, p);
				}
				return Math.Pow(total, 1 / p);
			}
		}

		/*
		public static double ColorDistance<TPixel>(TPixel one, TPixel two, IMeasurer measurer = null)
			where TPixel : struct, IPixel<TPixel>
		{
			if (measurer == null) {
				measurer = DefaultColorDistanceMeasurer;
			}
			var cOne = one.ToColor();
			var cTwo = two.ToColor();
			double[] vOne = new double[] { cOne.R, cOne.G, cOne.B, cOne.A };
			double[] vTwo = new double[] { cTwo.R, cTwo.G, cTwo.B, cTwo.A };
			double dist = measurer.Measure(vOne,vTwo);
			return dist;

		}
		*/
		
		public static double ColorDistance(IFColor one, IFColor two, IMeasurer measurer = null)
		{
			if (measurer == null) {
				measurer = DefaultColorDistanceMeasurer;
			}
			double[] vOne = new double[] { one.R, one.G, one.B, one.A };
			double[] vTwo = new double[] { two.R, two.G, two.B, two.A };
			double dist = measurer.Measure(vOne,vTwo);
			return dist;

		}
		static IMeasurer DefaultColorDistanceMeasurer = Registry.Map(Metric.Euclidean);

		//Don't use this. use Registry Map instead - so that things stay consistent
		// This is here because I don't want to expose MeasureRaft, but I want to keep all
		// of the metrics related stuff together.
		internal static IMeasurer Map(Metric m, double pFactor = 2.0)
		{
			switch(m)
			{
			default:
			case Metric.None:
			case Metric.Euclidean:
				return new MeasurerRaft(DistanceEuclidean,DistanceEuclidean);
			case Metric.Canberra:
				return new MeasurerRaft(DistanceCanberra,DistanceCanberra);
			case Metric.Manhattan:
				return new MeasurerRaft(DistanceManhattan,DistanceManhattan);
			case Metric.Chebyshev:
				return new MeasurerRaft(Chebyshev,Chebyshev);
			case Metric.ChebyshevInv:
				return new MeasurerRaft(ChebyshevInv,ChebyshevInv);
			case Metric.Minkowski:
				return new MeasureMinkowski(pFactor);
			}
		}

		class MeasurerRaft : IMeasurer
		{
			public delegate double MeasureTwo(double x1, double y1, double x2, double y2);
			public delegate double MeasureMore(double[] u, double[] v);

			public MeasurerRaft(MeasureTwo two,MeasureMore more) {
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

		class MeasureMinkowski : IMeasurer
		{
			public MeasureMinkowski(double pFactor) {
				PFactor = pFactor;
			}
			double PFactor;

			public double Measure(double x1, double y1, double x2, double y2) {
				return MetricHelpers.DistanceMinkowski(x1,y1,x2,y2,PFactor);
			}
			public double Measure(double[] u, double[] v) {
				return MetricHelpers.DistanceMinkowski(u,v,PFactor);
			}
		}
	}
}
