using System.Drawing;
using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class TestUlamSpiral : AbstractFunctionTest
{
	const int TestSizePixels = 256;
	const string MyName = nameof(Plugin.Functions.UlamSpiral);
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
		yield return CreateTestInfo(1, new string[0]);
		yield return CreateTestInfo(2, new string[] { "-c1","white","-c2","black" });
		yield return CreateTestInfo(3, new string[] { "-m", "1" });
		yield return CreateTestInfo(4, new string[] { "-m", "2" });
		yield return CreateTestInfo(5, new string[] { "-6m" });
		yield return CreateTestInfo(6, new string[] { "-f" });
		yield return CreateTestInfo(7, new string[] { "-f","-s","2","-ds","20.0" });
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
