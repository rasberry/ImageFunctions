using ImageFunctions.Core;
using System.Drawing;

namespace ImageFunctions.Test;

[TestClass]
public class TestSpearGraphic : AbstractFunctionTest
{
	const int TestSizePixels = 256;
	const string MyName = nameof(Plugin.Functions.SpearGraphic);
	public override string FunctionName { get { return MyName; } }

	[TestMethod]
	[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 140.0;
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
		foreach(var info in GetFunctionInfo()) {
			yield return info;
		}
	}

	public static IEnumerable<TestFunctionInfo> GetFunctionInfo()
	{
		yield return CreateTestInfo(1, new string[] { "-g", "First_Twist1" });
		yield return CreateTestInfo(2, new string[] { "-g", "First_Twist2" });
		yield return CreateTestInfo(3, new string[] { "-g", "First_Twist3" });
		yield return CreateTestInfo(4, new string[] { "-g", "Second_Twist3a" });
		yield return CreateTestInfo(5, new string[] { "-g", "Second_Twist3b" });
		yield return CreateTestInfo(6, new string[] { "-g", "Second_Twist3c" });
		yield return CreateTestInfo(7, new string[] { "-g", "Second_Twist4" });
		yield return CreateTestInfo(8, new string[] { "-g", "Third", "-rs", "531" });
		yield return CreateTestInfo(9, new string[] { "-g", "Fourth", "-rs", "531" });
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
