using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;

namespace test
{
	[TestClass]
	public class TestMathHelpers
	{
		[DataTestMethod]
		[DataRow( 0, 0, 0)]
		[DataRow( 1, 1, 0)]
		[DataRow( 2, 1,-1)]
		[DataRow( 3, 0,-1)]
		[DataRow( 4,-1,-1)]
		[DataRow( 5,-1, 0)]
		[DataRow( 6,-1, 1)]
		[DataRow( 7, 0, 1)]
		[DataRow( 8, 1, 1)]
		[DataRow( 9, 2, 1)]
		[DataRow(10, 2, 0)]
		[DataRow(11, 2,-1)]
		[DataRow(12, 2,-2)]
		[DataRow(13, 1,-2)]
		[DataRow(14, 0,-2)]
		[DataRow(15,-1,-2)]
		[DataRow(16,-2,-2)]
		[DataRow(17,-2,-1)]
		[DataRow(18,-2, 0)]
		[DataRow(19,-2, 1)]
		[DataRow(20,-2, 2)]
		[DataRow(21,-1, 2)]
		public void TestSpiralSquareToXY(long p,int exx,int exy)
		{
			var (x,y) = MathHelpers.SpiralSquareToXY(p);
			Assert.AreEqual(exx,x);
			Assert.AreEqual(exy,y);
		}

		[DataTestMethod]
		[DataRow( 0, 0, 0)]
		[DataRow( 1, 1, 0)]
		[DataRow( 2, 1,-1)]
		[DataRow( 3, 0,-1)]
		[DataRow( 4,-1,-1)]
		[DataRow( 5,-1, 0)]
		[DataRow( 6,-1, 1)]
		[DataRow( 7, 0, 1)]
		[DataRow( 8, 1, 1)]
		[DataRow( 9, 2, 1)]
		[DataRow(10, 2, 0)]
		[DataRow(11, 2,-1)]
		[DataRow(12, 2,-2)]
		[DataRow(13, 1,-2)]
		[DataRow(14, 0,-2)]
		[DataRow(15,-1,-2)]
		[DataRow(16,-2,-2)]
		[DataRow(17,-2,-1)]
		[DataRow(18,-2, 0)]
		[DataRow(19,-2, 1)]
		[DataRow(20,-2, 2)]
		[DataRow(21,-1, 2)]
		public void TestXYToSpiralSquare(long exp,int x,int y)
		{
			var p = MathHelpers.XYToSpiralSquare(x,y);
			Assert.AreEqual(exp,p);
		}
	}
}