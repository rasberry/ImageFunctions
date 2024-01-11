using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class TestZoomBlur : AbstractFunctionTest
{
	const string MyName = nameof(Plugin.Functions.ZoomBlur);
	public override string FunctionName { get { return MyName; }}

	[TestMethod]
	[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 57.0;
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
		yield return CreateTestInfo(1, startImg, new string[0]);
		yield return CreateTestInfo(2, startImg, new string[] { "-z", "3" });
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
		var list = new string[] { "zebra","boy","building","creek" };
		return list;
	}
}
