using ImageFunctions.Core;

namespace ImageFunctions.Test;

[TestClass]
public class TestAllColors : AbstractFunctionTest
{
	const int TestSizePixels = 256;
	const string MyName = nameof(Plugin.Functions.AllColors);
	public override string FunctionName { get { return MyName; }}

	[TestMethod]
	[DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
	public void Test(TestFunctionInfo info)
	{
		using var layers = new Layers();
		info.Layers = layers;

		RunFunction(info);
		//info.Options.Engine.Item.Value.SaveImage(layers,"test-" + info.OutName);

		Assert.AreEqual(true, info.Success);
		Assert.AreEqual(0, info.ExitCode);

		GetOrLoadResourceImage(info, info.OutName, "control");
		var dist = CompareTopTwoLayers(info);
		Log.Debug($"{info.OutName} dist = [{dist.R},{dist.G},{dist.B},{dist.A}] total={dist.Total}");

		//TODO data seems to be off by a small but significant amount.. not sure why
		Assert.IsTrue(dist.Total < 0.002,$"Name = {info.OutName} Distance = {dist}");
	}

	public static IEnumerable<object[]> GetData()
	{
		foreach(var info in GetFunctionInfo()) {
			yield return new object[] { info };
		}
	}

	public static IEnumerable<TestFunctionInfo> GetFunctionInfo()
	{
		yield return CreateTestInfo(0,new string[0]);
		yield return CreateTestInfo(1,new string[] { "-l" });
		yield return CreateTestInfo(2,new string[] { "-s","RGB"  ,"-so","1,2,3", "-on", "427296640"});
		yield return CreateTestInfo(3,new string[] { "-s","RGB"  ,"-so","1,2,3", "-l"});
		yield return CreateTestInfo(4,new string[] { "-s","Cmyk" ,"-so","1,2,3" });
		yield return CreateTestInfo(5,new string[] { "-s","HSV"  ,"-so","2,1,3" });
		yield return CreateTestInfo(6,new string[] { "-s","YCbCr","-so","1,2,3" });
		yield return CreateTestInfo(7,new string[] { "-s","YCbCr","-so","3,2,1" });
	}

	static TestFunctionInfo CreateTestInfo(int num,string[] args)
	{
		return new TestFunctionInfo {
			Args = args,
			OutName = $"{MyName}-{num}",
			Size = (TestSizePixels, TestSizePixels)
		};
	}
}
