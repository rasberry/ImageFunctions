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
				for (int i = 0; i < p1.Length; p++) {
					double d = Math.Abs(p2[i] - p1[i]);
					total += Math.Pow(d, p);
				}
				return Math.Pow(total, 1 / p);
			}
		}
	}
}
