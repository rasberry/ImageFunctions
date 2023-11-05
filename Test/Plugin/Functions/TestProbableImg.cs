using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class TestProbableImg : AbstractFunctionTest
{
	const string MyName = nameof(Plugin.Functions.ProbableImg);
	public override string FunctionName { get { return MyName; }}

	[TestMethod]
	[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;
		info.MaxDiff = 0.0;
		info.SaveImage = SaveImageMode.SubjectOnly;
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

	public static IEnumerable<TestFunctionInfo> GetFunctionInfo(string startImg)
	{
		yield return CreateTestInfo(1,startImg,new string[] { "-rs","321" });
		yield return CreateTestInfo(2,startImg,new string[] { "-rs","321","-n","5" });
		yield return CreateTestInfo(3,startImg,new string[] { "-rs","321","-pp","50%","50%" });
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
		var list = new string[] { "cookie","flower","harddrive" };
		return list;
	}
}
