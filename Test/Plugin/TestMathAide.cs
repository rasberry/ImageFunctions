using ImageFunctions.Plugin.Aides;

namespace ImageFunctions.Test.Plugin;

[TestClass]
public class TestMathAide
{
	[TestMethod]
	[DynamicData(nameof(LinearData))]
	public void TestLinearToXY(long p, int exx, int exy)
	{
		var (x, y) = MathAidePlus.LinearToXY(p, 10);
		Assert.AreEqual(exx, x);
		Assert.AreEqual(exy, y);
	}

	[TestMethod]
	[DynamicData(nameof(LinearData))]
	public void TestXYToLinear(long exp, int x, int y)
	{
		var p = MathAidePlus.XYToLinear(x, y, 10);
		Assert.AreEqual(exp, p);
	}

	static IEnumerable<object[]> LinearData()
	{
		yield return new object[] { 0, 0, 0 };
		yield return new object[] { 1, 1, 0 };
		yield return new object[] { 2, 2, 0 };
		yield return new object[] { 3, 3, 0 };
		yield return new object[] { 4, 4, 0 };
		yield return new object[] { 5, 5, 0 };
		yield return new object[] { 6, 6, 0 };
		yield return new object[] { 7, 7, 0 };
		yield return new object[] { 8, 8, 0 };
		yield return new object[] { 9, 9, 0 };
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

	[TestMethod]
	[DynamicData(nameof(LinearDataShift))]
	public void TestLinearToXYShift(long p, int exx, int exy)
	{
		var (x, y) = MathAidePlus.LinearToXY(p, 10, 10, 10);
		Assert.AreEqual(exx, x);
		Assert.AreEqual(exy, y);
	}

	[TestMethod]
	[DynamicData(nameof(LinearDataShift))]
	public void TestXYToLinearShift(long exp, int x, int y)
	{
		var p = MathAidePlus.XYToLinear(x, y, 10, 10, 10);
		Assert.AreEqual(exp, p);
	}

	static IEnumerable<object[]> LinearDataShift()
	{
		yield return new object[] { 0, 10, 10 };
		yield return new object[] { 1, 11, 10 };
		yield return new object[] { 2, 12, 10 };
		yield return new object[] { 3, 13, 10 };
		yield return new object[] { 4, 14, 10 };
		yield return new object[] { 5, 15, 10 };
		yield return new object[] { 6, 16, 10 };
		yield return new object[] { 7, 17, 10 };
		yield return new object[] { 8, 18, 10 };
		yield return new object[] { 9, 19, 10 };
		yield return new object[] { 10, 10, 11 };
		yield return new object[] { 11, 11, 11 };
		yield return new object[] { 12, 12, 11 };
		yield return new object[] { 13, 13, 11 };
		yield return new object[] { 14, 14, 11 };
		yield return new object[] { 15, 15, 11 };
		yield return new object[] { 16, 16, 11 };
		yield return new object[] { 17, 17, 11 };
		yield return new object[] { 18, 18, 11 };
		yield return new object[] { 19, 19, 11 };
		yield return new object[] { 20, 10, 12 };
		yield return new object[] { 21, 11, 12 };
	}

	[TestMethod]
	[DynamicData(nameof(DiagonalData))]
	public void TestDiagonalToXY(long p, int exx, int exy)
	{
		var (x, y) = MathAidePlus.DiagonalToXY(p);
		Assert.AreEqual(exx, x);
		Assert.AreEqual(exy, y);
	}

	[TestMethod]
	[DynamicData(nameof(DiagonalData))]
	public void TestXYToDiagonal(long exp, int x, int y)
	{
		var p = MathAidePlus.XYToDiagonal(x, y);
		Assert.AreEqual(exp, p);
	}

	static IEnumerable<object[]> DiagonalData()
	{
		yield return new object[] { 0, 0, 0 };
		yield return new object[] { 1, 0, 1 };
		yield return new object[] { 2, 1, 0 };
		yield return new object[] { 3, 0, 2 };
		yield return new object[] { 4, 1, 1 };
		yield return new object[] { 5, 2, 0 };
		yield return new object[] { 6, 0, 3 };
		yield return new object[] { 7, 1, 2 };
		yield return new object[] { 8, 2, 1 };
		yield return new object[] { 9, 3, 0 };
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

	[TestMethod]
	[DynamicData(nameof(DiagonalDataShift))]
	public void TestDiagonalToXYShift(long p, int exx, int exy)
	{
		var (x, y) = MathAidePlus.DiagonalToXY(p, 10, 10);
		Assert.AreEqual(exx, x);
		Assert.AreEqual(exy, y);
	}

	[TestMethod]
	[DynamicData(nameof(DiagonalDataShift))]
	public void TestXYToDiagonalShift(long exp, int x, int y)
	{
		var p = MathAidePlus.XYToDiagonal(x, y, 10, 10);
		Assert.AreEqual(exp, p);
	}

	static IEnumerable<object[]> DiagonalDataShift()
	{
		yield return new object[] { 0, 10, 10 };
		yield return new object[] { 1, 10, 11 };
		yield return new object[] { 2, 11, 10 };
		yield return new object[] { 3, 10, 12 };
		yield return new object[] { 4, 11, 11 };
		yield return new object[] { 5, 12, 10 };
		yield return new object[] { 6, 10, 13 };
		yield return new object[] { 7, 11, 12 };
		yield return new object[] { 8, 12, 11 };
		yield return new object[] { 9, 13, 10 };
		yield return new object[] { 10, 10, 14 };
		yield return new object[] { 11, 11, 13 };
		yield return new object[] { 12, 12, 12 };
		yield return new object[] { 13, 13, 11 };
		yield return new object[] { 14, 14, 10 };
		yield return new object[] { 15, 10, 15 };
		yield return new object[] { 16, 11, 14 };
		yield return new object[] { 17, 12, 13 };
		yield return new object[] { 18, 13, 12 };
		yield return new object[] { 19, 14, 11 };
		yield return new object[] { 20, 15, 10 };
		yield return new object[] { 21, 10, 16 };
	}

	[TestMethod]
	[DynamicData(nameof(SpiralSquareData))]
	public void TestSpiralSquareToXY(long p, int exx, int exy)
	{
		var (x, y) = MathAidePlus.SpiralSquareToXY(p);
		Assert.AreEqual(exx, x);
		Assert.AreEqual(exy, y);
	}

	[TestMethod]
	[DynamicData(nameof(SpiralSquareData))]
	public void TestXYToSpiralSquare(long exp, int x, int y)
	{
		var p = MathAidePlus.XYToSpiralSquare(x, y);
		Assert.AreEqual(exp, p);
	}

	static IEnumerable<object[]> SpiralSquareData()
	{
		yield return new object[] { 0, 0, 0 };
		yield return new object[] { 1, 1, 0 };
		yield return new object[] { 2, 1, -1 };
		yield return new object[] { 3, 0, -1 };
		yield return new object[] { 4, -1, -1 };
		yield return new object[] { 5, -1, 0 };
		yield return new object[] { 6, -1, 1 };
		yield return new object[] { 7, 0, 1 };
		yield return new object[] { 8, 1, 1 };
		yield return new object[] { 9, 2, 1 };
		yield return new object[] { 10, 2, 0 };
		yield return new object[] { 11, 2, -1 };
		yield return new object[] { 12, 2, -2 };
		yield return new object[] { 13, 1, -2 };
		yield return new object[] { 14, 0, -2 };
		yield return new object[] { 15, -1, -2 };
		yield return new object[] { 16, -2, -2 };
		yield return new object[] { 17, -2, -1 };
		yield return new object[] { 18, -2, 0 };
		yield return new object[] { 19, -2, 1 };
		yield return new object[] { 20, -2, 2 };
		yield return new object[] { 21, -1, 2 };
	}

	[TestMethod]
	[DynamicData(nameof(SpiralSquareDataShift))]
	public void TestSpiralSquareToXYShift(long p, int exx, int exy)
	{
		var (x, y) = MathAidePlus.SpiralSquareToXY(p, 10, 10);
		Assert.AreEqual(exx, x);
		Assert.AreEqual(exy, y);
	}

	[TestMethod]
	[DynamicData(nameof(SpiralSquareDataShift))]
	public void TestXYToSpiralSquareShift(long exp, int x, int y)
	{
		var p = MathAidePlus.XYToSpiralSquare(x, y, 10, 10);
		Assert.AreEqual(exp, p);
	}

	static IEnumerable<object[]> SpiralSquareDataShift()
	{
		yield return new object[] { 0, 10, 10 };
		yield return new object[] { 1, 11, 10 };
		yield return new object[] { 2, 11, 09 };
		yield return new object[] { 3, 10, 09 };
		yield return new object[] { 4, 09, 09 };
		yield return new object[] { 5, 09, 10 };
		yield return new object[] { 6, 09, 11 };
		yield return new object[] { 7, 10, 11 };
		yield return new object[] { 8, 11, 11 };
		yield return new object[] { 9, 12, 11 };
		yield return new object[] { 10, 12, 10 };
		yield return new object[] { 11, 12, 09 };
		yield return new object[] { 12, 12, 08 };
		yield return new object[] { 13, 11, 08 };
		yield return new object[] { 14, 10, 08 };
		yield return new object[] { 15, 09, 08 };
		yield return new object[] { 16, 08, 08 };
		yield return new object[] { 17, 08, 09 };
		yield return new object[] { 18, 08, 10 };
		yield return new object[] { 19, 08, 11 };
		yield return new object[] { 20, 08, 12 };
		yield return new object[] { 21, 09, 12 };
	}
}
