using ImageFunctions.Core;
using System.Drawing;

namespace ImageFunctions.Test.Plugin.Functions;

[TestClass]
public class TestComplexPlot : AbstractFunctionTest
{
	const int TestSizePixels = 256;
	const string MyName = nameof(ImageFunctions.Plugin.Functions.ComplexPlot);
	public override string FunctionName { get { return MyName; } }

	[TestMethod]
	[DynamicData(nameof(GetData))]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 50.0;
		//info.SaveImage = SaveImageMode.SubjectOnly;
		RunFunctionAndCompare(info);
	}

	public static IEnumerable<object[]> GetData()
	{
		foreach(var info in GetFunctionInfo()) {
			yield return new object[] { info };
		}
	}

	internal override IEnumerable<TestFunctionInfo> GetTestInfo()
	{
		return GetFunctionInfo();
	}

	const string Eq1 = "(z^2+2i)/(z-1-i)";
	const string Eq2 = "z^3+z^2+z+1";
	const string Grad = "Gimp Tube Red";
	public static IEnumerable<TestFunctionInfo> GetFunctionInfo()
	{
		yield return CreateTestInfo(0, new string[] { "-e", Eq1 });
		yield return CreateTestInfo(1, new string[] { "-e", Eq1, "--gradient", Grad, "-gs", "4", "-go", "0.5" });
		yield return CreateTestInfo(2, new string[] { "-e", Eq1, "--gradient", Grad, "-gs", "4", "-mo" });
		yield return CreateTestInfo(3, new string[] { "-e", Eq1, "-f", "1.0" });
		yield return CreateTestInfo(4, new string[] { "-e", Eq1, "-mo" });
		yield return CreateTestInfo(5, new string[] { "-e", Eq2 });
		yield return CreateTestInfo(6, new string[] { "-e", Eq2, "-rx", "-4,4", "-ry", "-4,4" });
		yield return CreateTestInfo(7, new string[] { "-e", Eq2, "-po" });
	}

	static TestFunctionInfo CreateTestInfo(int num, string[] args)
	{
		return new TestFunctionInfo {
			Args = args,
			OutName = $"{MyName}-{num}",
			Size = new Size(TestSizePixels, TestSizePixels)
		};
	}
}
