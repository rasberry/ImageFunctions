using System.Drawing;
using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class TestTurmites : AbstractFunctionTest
{
	const int TestSizePixels = 256;
	const string MyName = nameof(Plugin.Functions.Turmites);
	public override string FunctionName { get { return MyName; }}

	[TestMethod]
	[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 0.0;
		//info.SaveImage = SaveImageMode.SubjectOnly;
		RunFunctionAndCompare(info);
	}

	public static IEnumerable<object[]> GetData()
	{
		foreach(var info in GetFunctionInfo()) {
			yield return new object[] { info };
		}
	}

	public static IEnumerable<TestFunctionInfo> GetFunctionInfo()
	{
		yield return CreateTestInfo(1, new string[0]);
		yield return CreateTestInfo(2, new string[] { "-p", "LRRL" });
		yield return CreateTestInfo(3, new string[] { "-p", "LRRL", "-i", "1e+7" });
		yield return CreateTestInfo(4, new string[] { "-p", "LRRL", "-e", "reflect", "-i", "1e+7" });
	}

	static TestFunctionInfo CreateTestInfo(int num,string[] args)
	{
		return new TestFunctionInfo {
			Args = args,
			OutName = $"{MyName}-{num}",
			Size = new Size(TestSizePixels, TestSizePixels)
		};
	}
}
