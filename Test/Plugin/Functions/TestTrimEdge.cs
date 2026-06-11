using ImageFunctions.Core;

namespace ImageFunctions.Test.Plugin.Functions;

[TestClass]
public class TestTrimEdge : AbstractFunctionTest
{
	const string MyName = nameof(ImageFunctions.Plugin.Functions.TrimEdge);
	public override string FunctionName { get { return MyName; } }

	[TestMethod]
	[DynamicData(nameof(GetData))]
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
		if(startImg.StartsWith("s_c") || startImg.StartsWith("s_d")) {
			yield return CreateTestInfo(0, startImg, ["-f", "0.05"]);
		}
		else {
			yield return CreateTestInfo(1, startImg, ["-f", "5%", "-r"]);
		}
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
		var list = new string[] {
			"s_cardusk", "s_carduskc","s_deerlot","s_deerlotc","s_deerwalk",
			"s_deerwalkc","s_sundusk","s_sunduskc","s_treesdusk","s_treesduskc"
		};
		return list;
	}
}
