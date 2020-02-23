using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageFunctions.Helpers;
using SixLabors.ImageSharp;
using System.Collections.Generic;

namespace test
{
	[TestClass]
	public class TestMathHelpers
	{
		[DataTestMethod]
		[DynamicData(nameof(LinearData),DynamicDataSourceType.Method)]
		public void TestLinearToXY(long p,int exx,int exy)
		{
			var (x,y) = MathHelpers.LinearToXY(p,10);
			Assert.AreEqual(exx,x);
			Assert.AreEqual(exy,y);
		}

		[DataTestMethod]
		[DynamicData(nameof(LinearData),DynamicDataSourceType.Method)]
		public void TestXYToLinear(long exp,int x,int y)
		{
			var p = MathHelpers.XYToLinear(x,y,10);
			Assert.AreEqual(exp,p);
		}

		static IEnumerable<object[]> LinearData()
		{
			yield return new object[] {  0, 0, 0 };
			yield return new object[] {  1, 1, 0 };
			yield return new object[] {  2, 2, 0 };
			yield return new object[] {  3, 3, 0 };
			yield return new object[] {  4, 4, 0 };
			yield return new object[] {  5, 5, 0 };
			yield return new object[] {  6, 6, 0 };
			yield return new object[] {  7, 7, 0 };
			yield return new object[] {  8, 8, 0 };
			yield return new object[] {  9, 9, 0 };
			yield return new object[] { 10, 0, 1 };
			yield return new object[] { 11, 1, 1 };
			yield return new object[] { 12, 2, 1 };
			yield return new object[] { 13, 3, 1 };
			yield return new object[] { 14, 4, 1 };
			yield return new object[] { 15, 5, 1 };
			yield return new object[] { 16, 6, 1 };
			yield return new object[] { 17, 7, 1 };
			yield return new object[] { 18, 8, 1 };
			yield return new object[] { 19, 9, 1 };
			yield return new object[] { 20, 0, 2 };
			yield return new object[] { 21, 1, 2 };
		}

		[DataTestMethod]
		[DynamicData(nameof(DiagonalData),DynamicDataSourceType.Method)]
		public void TestDiagonalToXY(long p,int exx,int exy)
		{
			var (x,y) = MathHelpers.DiagonalToXY(p);
			Assert.AreEqual(exx,x);
			Assert.AreEqual(exy,y);
		}

		[DataTestMethod]
		[DynamicData(nameof(DiagonalData),DynamicDataSourceType.Method)]
		public void TestXYToDiagonal(long exp,int x,int y)
		{
			var p = MathHelpers.XYToDiagonal(x,y);
			Assert.AreEqual(exp,p);
		}

		static IEnumerable<object[]> DiagonalData()
		{
			yield return new object[] {  0, 0, 0 };
			yield return new object[] {  1, 0, 1 };
			yield return new object[] {  2, 1, 0 };
			yield return new object[] {  3, 0, 2 };
			yield return new object[] {  4, 1, 1 };
			yield return new object[] {  5, 2, 0 };
			yield return new object[] {  6, 0, 3 };
			yield return new object[] {  7, 1, 2 };
			yield return new object[] {  8, 2, 1 };
			yield return new object[] {  9, 3, 0 };
			yield return new object[] { 10, 0, 4 };
			yield return new object[] { 11, 1, 3 };
			yield return new object[] { 12, 2, 2 };
			yield return new object[] { 13, 3, 1 };
			yield return new object[] { 14, 4, 0 };
			yield return new object[] { 15, 0, 5 };
			yield return new object[] { 16, 1, 4 };
			yield return new object[] { 17, 2, 3 };
			yield return new object[] { 18, 3, 2 };
			yield return new object[] { 19, 4, 1 };
			yield return new object[] { 20, 5, 0 };
			yield return new object[] { 21, 0, 6 };
		}

		[DataTestMethod]
		[DynamicData(nameof(SpiralSquareData),DynamicDataSourceType.Method)]
		public void TestSpiralSquareToXY(long p,int exx,int exy)
		{
			var (x,y) = MathHelpers.SpiralSquareToXY(p);
			Assert.AreEqual(exx,x);
			Assert.AreEqual(exy,y);
		}

		[DataTestMethod]
		[DynamicData(nameof(SpiralSquareData),DynamicDataSourceType.Method)]
		public void TestXYToSpiralSquare(long exp,int x,int y)
		{
			var p = MathHelpers.XYToSpiralSquare(x,y);
			Assert.AreEqual(exp,p);
		}

		static IEnumerable<object[]> SpiralSquareData()
		{
			yield return new object[] {  0, 0, 0 };
			yield return new object[] {  1, 1, 0 };
			yield return new object[] {  2, 1,-1 };
			yield return new object[] {  3, 0,-1 };
			yield return new object[] {  4,-1,-1 };
			yield return new object[] {  5,-1, 0 };
			yield return new object[] {  6,-1, 1 };
			yield return new object[] {  7, 0, 1 };
			yield return new object[] {  8, 1, 1 };
			yield return new object[] {  9, 2, 1 };
			yield return new object[] { 10, 2, 0 };
			yield return new object[] { 11, 2,-1 };
			yield return new object[] { 12, 2,-2 };
			yield return new object[] { 13, 1,-2 };
			yield return new object[] { 14, 0,-2 };
			yield return new object[] { 15,-1,-2 };
			yield return new object[] { 16,-2,-2 };
			yield return new object[] { 17,-2,-1 };
			yield return new object[] { 18,-2, 0 };
			yield return new object[] { 19,-2, 1 };
			yield return new object[] { 20,-2, 2 };
			yield return new object[] { 21,-1, 2 };
		}
	}
}