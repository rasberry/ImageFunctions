using ImageFunctions.Core.Aides;

namespace ImageFunctions.Test.Core;

[TestClass]
public class TestMathAide
{
	[TestMethod]
	public void TestGCD()
	{
		Assert.AreEqual(1, MathAide.GCD(2, 1));
		Assert.AreEqual(2, MathAide.GCD(2, 2));
		Assert.AreEqual(1, MathAide.GCD(2, 3));
		Assert.AreEqual(3, MathAide.GCD(3, 9));
		Assert.AreEqual(3, MathAide.GCD(6, 9));
	}
}
