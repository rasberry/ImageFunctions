using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class TestFloodFill : AbstractFunctionTest
{
	const string MyName = nameof(Plugin.Functions.FloodFill);
	public override string FunctionName { get { return MyName; }}

	[TestMethod]
	[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 263.0;
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
		yield return CreateTestInfo(1, startImg, new string[] { "-p","0,0","-p","50,50" });
		yield return CreateTestInfo(2, startImg, new string[] { "-p","0,0","-p","50,50","-s","0.9" });
		yield return CreateTestInfo(3, startImg, new string[] { "-p","0,0","-p","50,50","-s","0.9","-i" });
		yield return CreateTestInfo(4, startImg, new string[] { "-p","100,100","-s","0.9","-i","-f","DepthFirst","-m","Horizontal"});
	}

	static TestFunctionInfo CreateTestInfo(int index, string startImg, string[] args)
	{
		bool needsSecond = args.Contains("-i");
		return new TestFunctionInfo {
			Args = args,
			OutName = $"{MyName}-{startImg}-{index}",
			ImageNames = needsSecond ? new[] { startImg, "scorpius"} : new[] { startImg }
		};
	}

	public static IEnumerable<string> GetImageNames()
	{
		var list = new string[] { "pool","rainbow","toes-p" };
		return list;
	}
}
