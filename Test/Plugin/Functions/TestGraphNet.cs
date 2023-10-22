using System.Drawing;
using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class TestGraphNet : AbstractFunctionTest
{
	const string MyName = nameof(Plugin.Functions.GraphNet);
	public override string FunctionName { get { return MyName; }}

	[TestMethod]
	[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 0.001;
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
		yield return CreateTestInfo(1, new string[] { "-rs","77" });
		yield return CreateTestInfo(2, new string[] { "-rs","88","-p","0.1%" });
		yield return CreateTestInfo(3, new string[] { "-rs","216","-b","256" });
	}

	const int TestSizePixels = 256;
	static TestFunctionInfo CreateTestInfo(int index, string[] args)
	{
		return new TestFunctionInfo {
			Args = args,
			OutName = $"{MyName}-{index}",
			Size = new Size(TestSizePixels, TestSizePixels)
		};
	}
}
