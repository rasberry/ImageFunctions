using ImageFunctions.Core;
using System.Drawing;

namespace ImageFunctions.Test.Plugin.Functions;

[TestClass]
public class TestFibSquares : AbstractFunctionTest
{
	const int TestSizePixelsW = 256;
	const int TestSizePixelsH = 160;
	const string MyName = nameof(ImageFunctions.Plugin.Functions.FibSquares);
	public override string FunctionName { get { return MyName; } }

	[TestMethod]
	[DynamicData(nameof(GetData))]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 39.000;
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

	public static IEnumerable<TestFunctionInfo> GetFunctionInfo()
	{
		yield return CreateTestInfo(0, new string[] { "-rs", "0" });
		yield return CreateTestInfo(1, new string[] { "-rs", "0", "-m", "2" });
		yield return CreateTestInfo(2, new string[] { "-rs", "0", "-s", "-m", "3" });
		yield return CreateTestInfo(3, new string[] { "-rs", "0", "-s", "-m", "0", "-b" });
	}

	static TestFunctionInfo CreateTestInfo(int num, string[] args)
	{
		return new TestFunctionInfo {
			Args = args,
			OutName = $"{MyName}-{num}",
			Size = new Size(TestSizePixelsW, TestSizePixelsH)
		};
	}
}
