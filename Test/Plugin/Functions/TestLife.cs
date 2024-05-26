using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class TestLife : AbstractFunctionTest
{
	const string MyName = nameof(Plugin.Functions.Life);
	public override string FunctionName { get { return MyName; }}

	[TestMethod]
	[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 23.0;
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
		yield return CreateTestInfo(1, startImg, new string[] { "-i","1000","-b","70%","-th","10%" });
		yield return CreateTestInfo(2, startImg, new string[] { "-i","1000","-b","70%","-th","10%","-w" });
		yield return CreateTestInfo(3, startImg, new string[] { "-i","1000","-b","70%","-th","10%","-w","-ch" });
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
		var list = new string[] { "spider","scorpius","skull" };
		return list;
	}
}
