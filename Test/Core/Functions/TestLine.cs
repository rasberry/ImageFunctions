using ImageFunctions.Core;

namespace ImageFunctions.Test.Core.Functions;

[TestClass]
public class TestLine : AbstractFunctionTest
{
	const string MyName = nameof(ImageFunctions.Core.Functions.Line);
	public override string FunctionName { get { return MyName; } }

	[TestMethod]
	[DynamicData(nameof(GetData))]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 0.6;
		// info.SaveImage = SaveImageMode.SubjectOnly;
		RunFunctionAndCompare(info);
	}

	public static IEnumerable<object[]> GetData()
	{
		foreach(var imgName in GetImageNames()) {
			foreach(var info in GetFunctionInfo(imgName)) {
				yield return new object[] { info };
			}
		}
	}

	internal override IEnumerable<TestFunctionInfo> GetTestInfo()
	{
		foreach(var imgName in GetImageNames()) {
			foreach(var info in GetFunctionInfo(imgName)) {
				yield return info;
			}
		}
	}

	public static IEnumerable<TestFunctionInfo> GetFunctionInfo(string startImg)
	{
		yield return CreateTestInfo(0, startImg, new string[] { "-p", "10,10", "-p", "20,90", "-pp", "0.9,0.9" });
		yield return CreateTestInfo(1, startImg, new string[] { "-m", "XiaolinWu", "-c", "White", "-p", "10,10", "-pp", "0.9,0.5", "-pp", "0.1,0.9" });
	}

	static TestFunctionInfo CreateTestInfo(int index, string startImg, string[] args)
	{
		return new TestFunctionInfo {
			Args = args,
			OutName = $"{MyName}-{startImg}-{index}",
			ImageNames = new[] { startImg }
		};
	}

	public static IEnumerable<string> GetImageNames()
	{
		var list = new string[] { "handle" };
		return list;
	}
}
