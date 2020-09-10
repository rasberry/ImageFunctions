using System;
using System.IO;
using System.Text;
using ImageFunctions;
using ImageFunctions.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace test
{
	[TestClass]
	public class TestGradient
	{
		[DataTestMethod]
		[DataRow(0.0, 0.0)]
		[DataRow(0.1, 0.1)]
		[DataRow(0.2, 0.2)]
		[DataRow(0.3, 0.3)]
		[DataRow(0.4, 0.4)]
		[DataRow(0.5, 0.5)]
		[DataRow(0.6, 0.6)]
		[DataRow(0.7, 0.7)]
		[DataRow(0.8, 0.8)]
		[DataRow(0.9, 0.9)]
		[DataRow(1.0, 1.0)]
		public void TestGrayGradient(double index, double egray)
		{
			var grad = new GrayGradient();
			var c1 = new IColor(egray,egray,egray,1.0);
			var c2 = grad.GetColor(index);
			Helpers.AssertAreSimilar(c1,c2,0.2);
		}

		[DataTestMethod]
		[DataRow(0.0, 0.0, 0.0, 0.0)]
		[DataRow(0.1, 0.2, 0.0, 0.0)]
		[DataRow(0.2, 0.4, 0.1, 0.0)]
		[DataRow(0.3, 0.6, 0.3, 0.0)]
		[DataRow(0.4, 0.8, 0.5, 0.0)]
		[DataRow(0.5, 1.0, 0.8, 0.0)]
		[DataRow(0.6, 1.0, 0.9, 0.2)]
		[DataRow(0.7, 1.0, 0.9, 0.4)]
		[DataRow(0.8, 1.0, 0.9, 0.6)]
		[DataRow(0.9, 1.0, 0.9, 0.8)]
		[DataRow(1.0, 1.0, 1.0, 1.0)]
		public void TestFullRangeRGBGradient(double index, double r,double g,double b)
		{
			var grad = new FullRangeRGBGradient();
			var c1 = new IColor(r,g,b,1.0);
			var c2 = grad.GetColor(index);
			Helpers.AssertAreSimilar(c1,c2,0.2);
		}

		static GimpGGRGradient _ggr = null;
		static GimpGGRGradient GetGimpGGRGradient()
		{
			if(_ggr == null) {
				string ggrFile = String.Join(Environment.NewLine
					,"GIMP Gradient"
					,"Name: CD"
					,"18"
					,"0.000000 0.010566 0.023372 0.819999 0.820000 0.820000 1.000000 0.879999 0.880000 0.880000 1.000000 0 0"
					,"0.023372 0.045682 0.063439 0.879999 0.880000 0.880000 1.000000 0.999999 1.000000 1.000000 1.000000 0 0"
					,"0.063439 0.082638 0.176962 0.999999 1.000000 1.000000 1.000000 0.909999 0.910000 0.910000 1.000000 0 0"
					,"0.176962 0.205342 0.236227 0.909999 0.910000 0.910000 1.000000 0.819999 0.820000 0.820000 1.000000 0 0"
					,"0.236227 0.267623 0.281302 0.819999 0.820000 0.820000 1.000000 0.903167 1.000000 0.000000 1.000000 0 0"
					,"0.281302 0.296327 0.310518 0.903167 1.000000 0.000000 1.000000 0.000000 0.877893 1.000000 1.000000 0 0"
					,"0.310518 0.321369 0.340568 0.000000 0.877893 1.000000 1.000000 0.384390 1.000000 0.900682 1.000000 0 0"
					,"0.340568 0.357129 0.373957 0.384390 1.000000 0.900682 1.000000 0.819999 0.820000 0.820000 1.000000 0 0"
					,"0.373957 0.434190 0.500000 0.819999 0.820000 0.820000 1.000000 0.879999 0.880000 0.880000 1.000000 0 0"
					,"0.500000 0.510566 0.523372 0.819999 0.820000 0.820000 1.000000 0.879999 0.880000 0.880000 1.000000 0 0"
					,"0.523372 0.545682 0.563439 0.879999 0.880000 0.880000 1.000000 0.999999 1.000000 1.000000 1.000000 0 0"
					,"0.563439 0.582638 0.676962 0.999999 1.000000 1.000000 1.000000 0.909999 0.910000 0.910000 1.000000 0 0"
					,"0.676962 0.705342 0.736227 0.909999 0.910000 0.910000 1.000000 0.819999 0.820000 0.820000 1.000000 0 0"
					,"0.736227 0.767623 0.781302 0.819999 0.820000 0.820000 1.000000 0.903167 1.000000 0.000000 1.000000 0 0"
					,"0.781302 0.796327 0.810518 0.903167 1.000000 0.000000 1.000000 0.000000 0.877893 1.000000 1.000000 0 0"
					,"0.810518 0.821369 0.840568 0.000000 0.877893 1.000000 1.000000 0.384390 1.000000 0.900682 1.000000 0 0"
					,"0.840568 0.857129 0.873957 0.384390 1.000000 0.900682 1.000000 0.819999 0.820000 0.820000 1.000000 0 0"
					,"0.873957 0.934190 1.000000 0.819999 0.820000 0.820000 1.000000 0.879999 0.880000 0.880000 1.000000 0 0"
				);

				var ggrBytes = Encoding.UTF8.GetBytes(ggrFile);
				using (var stream = new MemoryStream(ggrBytes)) {
					_ggr = new GimpGGRGradient(stream);
				}
			}
			return _ggr;
		}

		[DataTestMethod]
		[DataRow(0.0, 0.8, 0.8, 0.8)]
		[DataRow(0.1, 0.9, 0.9, 0.9)]
		[DataRow(0.2, 0.9, 0.9, 0.9)]
		[DataRow(0.3, 0.3, 0.9, 0.6)]
		[DataRow(0.4, 0.8, 0.8, 0.8)]
		[DataRow(0.5, 0.9, 0.9, 0.9)]
		[DataRow(0.6, 0.9, 0.9, 0.9)]
		[DataRow(0.7, 0.9, 0.9, 0.9)]
		[DataRow(0.8, 0.3, 0.9, 0.6)]
		[DataRow(0.9, 0.8, 0.8, 0.8)]
		[DataRow(1.0, 0.9, 0.9, 0.9)]
		public void TestGimpGGRGradient(double index, double r,double g,double b)
		{
			var grad = GetGimpGGRGradient();
			var c1 = new IColor(r,g,b,1.0);
			var c2 = grad.GetColor(index);
			Helpers.AssertAreSimilar(c1,c2,0.2);
		}
	}
}