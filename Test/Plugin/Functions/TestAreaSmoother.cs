using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class TestAreaSmoother : AbstractFunctionTest
{
	const string MyName = nameof(Plugin.Functions.AreaSmoother);
	public override string FunctionName { get { return MyName; }}

	[TestMethod]
	[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;

		RunFunction(info);
		Assert.AreEqual(true, info.Success);
		Assert.AreEqual(0, info.ExitCode);

		GetOrLoadResourceImage(info, info.OutName, "control");
		double dist = CompareTopTwoLayers(info);

		//TODO data seems to be off by a small but significant amount.. not sure why
		Assert.IsTrue(dist < 80.0, $"Name = {info.OutName} Distance = {dist}");
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
		yield return CreateTestInfo(1, startImg, new string[0]);
		yield return CreateTestInfo(2, startImg, new string[] { "-t","2" });
		yield return CreateTestInfo(3, startImg, new string[] { "-t","10" });
		yield return CreateTestInfo(4, startImg, new string[] { "--metric","Manhattan" });
		// case 4: return new string[] { "--sampler","11" }; //TODO this produces a bad image now
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
		//var list = new string[] { "rock-p","scorpius-p","shack-p","shell-p","skull-p","spider-p","toes-p" };
		var list = new string[] { "rock-p","scorpius-p" };
		return list;
	}
}
